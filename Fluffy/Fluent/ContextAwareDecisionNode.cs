using System;

namespace Fluffy.Fluent
{
    public class ContextAwareDecisionNode<TValue, TContext> :
            IDecisionNode<TValue, TContext>,
            IInvokable<TValue>, IInvokable,
            IContextAwareInvokable<TValue, TContext>, IContextAwareInvokable<TContext>,
            ICheckable, IContextAwareCheckable<TContext>
    {
        private readonly IDecisionConfigurator<TContext> _configurable;
        private TContext _context;
        private protected readonly Predicate<TValue> _valueCondition;
        private readonly Predicate<TContext> _contextCondition;
        private protected Func<TValue, TContext, object> _func;

        internal ContextAwareDecisionNode(IDecisionConfigurator<TContext> configurable, TContext context)
            : this(configurable, x => true, x => true, context)
        {
        }

        internal ContextAwareDecisionNode(IDecisionConfigurator<TContext> configurable, Predicate<TValue> valueCondition, Predicate<TContext> contextCondition, TContext context)
        {
            _configurable = configurable;
            _valueCondition = valueCondition;
            _contextCondition = contextCondition;
            _context = context;
        }

        public IDecisionConfigurator<TContext> Do(Action<TValue> action)
        {
            return Do((value, context) =>
            {
                action(value);
                return default;
            });
        }

        public IDecisionConfigurator<TContext> Do(Func<TValue, object> func)
        {
            return Do((value, context) => func(value));
        }

        public IDecisionConfigurator<TContext> Do(Action<TValue, TContext> action)
        {
            return Do((value, context) =>
            {
                action(value, context);
                return null;
            });
        }

        public IDecisionConfigurator<TContext> Do(Func<TValue, TContext, object> func)
        {
            _func = func;
            return _configurable;
        }

        public bool Check(object value)
        {
            return value is TValue typedValue && _valueCondition(typedValue) && _contextCondition(_context);
        }

        public bool Check(TContext context, object value)
        {
            return value is TValue typedValue && _valueCondition(typedValue) && _contextCondition(context);
        }

        public bool Invoke(TValue value)
        {
            return Invoke(value, out _);
        }

        public bool Invoke(object value)
        {
            return Invoke(value, out _);
        }

        public bool Invoke(TValue value, out object result)
        {
            if (_func == null)
            {
                result = null;
                return false;
            }
            result = _func.Invoke(value, _context);
            return true;
        }

        public bool Invoke(object value, out object result)
        {
            if (_func != null && value is TValue invokeValue)
            {
                result = _func.Invoke(invokeValue, _context);
                return true;
            }

            result = null;
            return false;
        }

        public bool Invoke(TValue value, TContext context)
        {
            return Invoke(value, context, out _);
        }

        public bool Invoke(TValue value, TContext context, out object result)
        {
            if (_func == null)
            {
                result = null;
                return false;
            }
            result = _func.Invoke(value, context);
            return true;
        }

        public bool Invoke(object value, TContext context)
        {
            return Invoke(value, context, out _);
        }

        public bool Invoke(object value, TContext context, out object result)
        {
            if (_func != null && value is TValue invokeValue)
            {
                result = _func.Invoke(invokeValue, context);
                return true;
            }

            result = null;
            return false;
        }
    }
}