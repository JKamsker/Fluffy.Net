using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class TypeSwitch : IDecisionConfigurator
    {
        private object _lockObject = new object();

        private protected readonly Dictionary<Type, List<ICheckable>> _checkableDictionary;

        //    private protected readonly List<ICheckable> _ceckables;
        private protected Func<object, object> _defaultFunc;

        public TypeSwitch()
        {
            _checkableDictionary = new Dictionary<Type, List<ICheckable>>();
            //  _ceckables = new List<ICheckable>();
        }

        public virtual object Handle<T>(T @object)
        {
            if (!_checkableDictionary.TryGetValue(typeof(T), out var checkables))
            {
                return _defaultFunc?.Invoke(@object);
            }

            foreach (var checkable in checkables.Where(x => x.Check(@object)).Cast<IInvokable<T>>())
            {
                if (checkable.Invoke(@object, out var result))
                {
                    return result;
                }
            }

            return _defaultFunc?.Invoke(@object);
        }

        // Old & Slow
        //public virtual object Handle(object @object)
        //{
        //    if (!_checkableDictionary.TryGetValue(@object.GetType(), out var checkables))
        //    {
        //        return _defaultFunc?.Invoke(@object);
        //    }

        //    foreach (var checkable in checkables.Where(x => x.Check(@object)).Cast<IInvokable>())
        //    {
        //        if (checkable.Invoke(@object, out var result))
        //        {
        //            return result;
        //        }
        //    }

        //    return _defaultFunc?.Invoke(@object);
        //}

        public virtual object Handle(object @object)
        {
            if (!_checkableDictionary.TryGetValue(@object.GetType(), out var checkables))
            {
                return _defaultFunc?.Invoke(@object);
            }

            foreach (var checkable in checkables)
            {
                if (!(checkable is IInvokable invokable))
                {
                    continue;
                }

                if (!checkable.Check(@object))
                {
                    continue;
                }

                if (invokable.Invoke(@object, out var result))
                {
                    return result;
                }
            }

            return _defaultFunc?.Invoke(@object);
        }

        public IDecisionNode<T> On<T>()
        {
            return On<T>(x => true);
        }

        public virtual IDecisionNode<T> On<T>(Predicate<T> condition)
        {
            if (!_checkableDictionary.TryGetValue(typeof(T), out var checkables))
            {
                lock (_lockObject)
                {
                    if (!_checkableDictionary.TryGetValue(typeof(T), out checkables))
                    {
                        checkables = new List<ICheckable>();
                        _checkableDictionary[typeof(T)] = checkables;
                    }
                }
            }

            var doSomething = new DecisionNode<T>(this, condition);
            checkables.Add(doSomething);
            return doSomething;
        }

        public IDecisionConfigurator Default(Action<object> action)
        {
            _defaultFunc = x => action;
            return this;
        }

        public IDecisionConfigurator Default(Func<object, object> action)
        {
            _defaultFunc = action;
            return this;
        }
    }
}