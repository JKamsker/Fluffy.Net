using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class TypeSwitch<TContext> : TypeSwitch, IDecisionConfigurator
    {
        private readonly TContext _context;

        public TypeSwitch(TContext context) : base()
        {
            _context = context;
        }

        public override IDecisionNode<T> On<T>(Predicate<T> condition)
        {
            var doSomething = new DecisionNode<T, TContext>(this, condition, _context);

            _ceckables.Add(doSomething);
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

    public class TypeSwitch : IDecisionConfigurator
    {
        private protected readonly List<ICheckable> _ceckables;
        private protected Func<object, object> _defaultFunc;

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

            return _defaultFunc?.Invoke(@object);
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

            return _defaultFunc?.Invoke(@object);
        }

        public IDecisionNode<T> On<T>()
        {
            return On<T>(x => true);
        }

        public virtual IDecisionNode<T> On<T>(Predicate<T> condition)
        {
            var doSomething = new DecisionNode<T>(this, condition);
            _ceckables.Add(doSomething);
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