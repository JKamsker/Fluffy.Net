using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class TypeSwitch : IConfigurable
    {
        private readonly List<ICheckable> _ceckables;
        private Action _defaultAction;

        public TypeSwitch()
        {
            _ceckables = new List<ICheckable>();
        }

        public void Handle<T>(T @object)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(@object)).Cast<IInvokable<T>>())
            {
                if (checkable.Invoke(@object))
                {
                    return;
                }
            }
            _defaultAction?.Invoke();
        }

        public void Handle(object @object)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(@object)).Cast<IInvokable>())
            {
                if (checkable.Invoke(@object))
                {
                    return;
                }
            }

            _defaultAction?.Invoke();
        }

        public void Default(Action action)
        {
            _defaultAction = action;
        }

        public ICanDo<T> On<T>()
        {
            return On<T>(x => true);
        }

        public ICanDo<T> On<T>(Predicate<T> condition)
        {
            var doSomething = new CanDo<T>(this, condition);
            _ceckables.Add(doSomething);
            return doSomething;
        }
    }
}