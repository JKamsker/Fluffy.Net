using System;

namespace Fluffy.Fluent
{
    public class DecisionNode<T> : IDecisionNode<T>, IInvokable<T>, IInvokable, ICheckable
    {
        private readonly IDecisionConfigurator _configurable;
        private readonly Predicate<T> _condition;
        private Func<T, object> _func;

        internal DecisionNode(IDecisionConfigurator configurable)
            : this(configurable, x => true)
        {
        }

        internal DecisionNode(IDecisionConfigurator typeSwitch, Predicate<T> condition)
        {
            _configurable = typeSwitch;
            _condition = condition;
        }

        public IDecisionConfigurator Do(Action<T> action)
        {
            return Do(x =>
            {
                action(x);
                return null;
            });
        }

        public IDecisionConfigurator Do(Func<T, object> func)
        {
            _func = func;
            return _configurable;
        }

        public bool Check(object value)
        {
            return value is T typedValue && _condition(typedValue);
        }

        public bool Invoke(T value, out object result)
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
            if (_func != null && value is T invokeValue)
            {
                result = _func.Invoke(invokeValue);
                return true;
            }

            result = null;
            return false;
        }

        public bool Invoke(T value)
        {
            return Invoke(value, out var result);
        }

        public bool Invoke(object value)
        {
            return Invoke(value, out var result);
        }
    }
}