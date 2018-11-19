using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class TypeSwitch : IDecisionConfigurator
    {
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
            if (_checkableDictionary.TryGetValue(typeof(T), out var checkables))
            {
                foreach (var checkable in checkables.Where(x => x.Check(@object)).Cast<IInvokable<T>>())
                {
                    if (checkable.Invoke(@object, out var result))
                    {
                        return result;
                    }
                }
            }

            return _defaultFunc?.Invoke(@object);
        }

        public virtual object Handle(object @object)
        {
            if (_checkableDictionary.TryGetValue(@object.GetType(), out var checkables))
            {
                foreach (var checkable in checkables.Where(x => x.Check(@object)).Cast<IInvokable>())
                {
                    if (checkable.Invoke(@object, out var result))
                    {
                        return result;
                    }
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
                checkables = new List<ICheckable>();
                _checkableDictionary[typeof(T)] = checkables;
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