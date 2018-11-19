using System;

namespace Fluffy.Fluent
{
    public class DecisionNode<TValue, TContext> : DecisionNode<TValue>, IDecisionNode<TValue>
    {
        private TContext _context;

        internal DecisionNode(IDecisionConfigurator configurable, TContext context)
            : this(configurable, x => true, context)
        {
        }

        internal DecisionNode(IDecisionConfigurator typeSwitch, Predicate<TValue> condition, TContext context)
            : base(typeSwitch, condition)
        {
            _context = context;
        }

        public IDecisionConfigurator Do(Action<TValue, TContext> action)
        {
            return Do((value, context) =>
            {
                action(value, context);
                return null;
            });
        }

        public IDecisionConfigurator Do(Func<TValue, TContext, object> func)
        {
            _func = value => func(value, _context);
            return _configurable;
        }
    }

    public class DecisionNode<TValue> : IDecisionNode<TValue>, IInvokable<TValue>, IInvokable, ICheckable
    {
        private protected readonly IDecisionConfigurator _configurable;
        private protected readonly Predicate<TValue> _condition;
        private protected Func<TValue, object> _func;

        internal DecisionNode(IDecisionConfigurator configurable)
            : this(configurable, x => true)
        {
        }

        internal DecisionNode(IDecisionConfigurator typeSwitch, Predicate<TValue> condition)
        {
            _configurable = typeSwitch;
            _condition = condition;
        }

        public IDecisionConfigurator Do(Action<TValue> action)
        {
            return Do(x =>
            {
                action(x);
                return null;
            });
        }

        public IDecisionConfigurator Do(Func<TValue, object> func)
        {
            _func = func;
            return _configurable;
        }

        public bool Check(object value)
        {
            return value is TValue typedValue && _condition(typedValue);
        }

        public bool Invoke(TValue value, out object result)
        {
            if (_func == null)
            {
                result = null;
                return false;
            }
            result = _func.Invoke(value);
            return true;
        }

        public bool Invoke(object value, out object result)
        {
            if (_func != null && value is TValue invokeValue)
            {
                result = _func.Invoke(invokeValue);
                return true;
            }

            result = null;
            return false;
        }

        public bool Invoke(TValue value)
        {
            return Invoke(value, out var result);
        }

        public bool Invoke(object value)
        {
            return Invoke(value, out var result);
        }
    }
}