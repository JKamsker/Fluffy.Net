using System;

namespace Fluffy.Fluent
{
    public class DecisionNode<T> : IDecisionNode<T>, IInvokable<T>, IInvokable, ICheckable
    {
        private readonly IDecisionConfigurator _configurable;
        private readonly Predicate<T> _condition;
        private Action<T> _action;

        internal DecisionNode(IDecisionConfigurator configurable) : this(configurable, x => true)
        {
        }

        internal DecisionNode(IDecisionConfigurator typeSwitch, Predicate<T> condition)
        {
            _configurable = typeSwitch;
            _condition = condition;
        }

        public IDecisionConfigurator Do(Action<T> action)
        {
            _action = action;
            return _configurable;
        }

        public bool Check(object value)
        {
            return value is T typedValue && _condition(typedValue);
        }

        public bool Invoke(T value)
        {
            if (_action == null)
            {
                return false;
            }
            _action.Invoke(value);
            return true;
        }

        public bool Invoke(object value)
        {
            if (_action == null)
            {
                return false;
            }

            if (value is T invokeValue)
            {
                _action.Invoke(invokeValue);
                return true;
            }

            return false;
        }
    }
}