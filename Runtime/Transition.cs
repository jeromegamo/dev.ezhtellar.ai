using System;

namespace Ezhtellar.AI
{
    // add OnTransition action
    public class Transition
    {
        public IState Origin { get; private set; }
        public IState Target { get; private set; }
        
        public Func<bool> Condition { get; private set; }

        public Transition(IState target, Func<bool> condition)
        {
            Target = target;
            Condition = condition;
        }
        
        public void SetOrigin(IState origin)
        {
            Origin = origin;
        }
    }
}