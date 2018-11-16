using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class TypeSwitch : IDecisionConfigurator
    {
        private readonly List<ICheckable> _ceckables;
        private Action _defaultAction;

        public TypeSwitch()
        {
            _ceckables = new List<ICheckable>();
        }

        public object Handle<T>(T @object)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(@object)).Cast<IInvokable<T>>())
            {
                if (checkable.Invoke(@object, out var result))
                {
                    return result;
                }
            }
            _defaultAction?.Invoke();
            return default;
        }

        public object Handle(object @object)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(@object)).Cast<IInvokable>())
            {
                if (checkable.Invoke(@object, out var result))
                {
                    return result;
                }
            }

            _defaultAction?.Invoke();
            return default;
        }

        public IDecisionNode<T> On<T>()
        {
            return On<T>(x => true);
        }

        public IDecisionNode<T> On<T>(Predicate<T> condition)
        {
            var doSomething = new DecisionNode<T>(this, condition);
            _ceckables.Add(doSomething);
            return doSomething;
        }

        public IDecisionConfigurator Default(Action action)
        {
            _defaultAction = action;
            return this;
        }
    }
}