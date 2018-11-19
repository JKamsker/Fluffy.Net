using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluffy.Fluent
{
    public class ContextAwareTypeSwitch<TContext> : IDecisionConfigurator<TContext>
    {
        private readonly TContext _context;
        private Func<object, TContext, object> _defaultFunc;
        private protected readonly List<ICheckable> _ceckables;

        public ContextAwareTypeSwitch(TContext context)
        {
            _ceckables = new List<ICheckable>();
            _context = context;
        }

        public IDecisionNode<TValue, TContext> On<TValue>()
        {
            return On<TValue>(x => true);
        }

        public IDecisionNode<TValue, TContext> On<TValue>(Predicate<TValue> valueCondition)
        {
            return On<TValue>(valueCondition, x => true);
        }

        public IDecisionNode<TValue, TContext> On<TValue>(Predicate<TValue> valueCondition, Predicate<TContext> contextCondition)
        {
            var doSomething = new ContextAwareDecisionNode<TValue, TContext>(this, valueCondition, contextCondition, _context);

            _ceckables.Add(doSomething);
            return doSomething;
        }

        public IDecisionConfigurator<TContext> Default(Action<object, TContext> action)
        {
            _defaultFunc = (value, context) =>
            {
                action(value, context);
                return default;
            };
            return this;
        }

        public IDecisionConfigurator<TContext> Default(Action<object> action)
        {
            return Default((value, context) => action(value));
        }

        public IDecisionConfigurator<TContext> Default(Func<object, TContext, object> action)
        {
            _defaultFunc = action;
            return this;
        }

        public object Handle<T>(T @object)
        {
            return Handle<T>(@object, _context);
        }

        public object Handle<TValue>(TValue value, TContext context)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(value)).Cast<IContextAwareInvokable<TValue, TContext>>())
            {
                if (checkable.Invoke(value, context, out var result))
                {
                    return result;
                }
            }

            return _defaultFunc?.Invoke(value, context);
        }

        public object Handle(object @object)
        {
            return Handle(@object, _context);
        }

        public object Handle(object @object, TContext context)
        {
            foreach (var checkable in _ceckables.Where(x => x.Check(@object)).Cast<IContextAwareInvokable<TContext>>())
            {
                if (checkable.Invoke(@object, context, out var result))
                {
                    return result;
                }
            }

            return _defaultFunc?.Invoke(@object, _context);
        }
    }
}