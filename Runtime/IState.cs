using System.Collections.Generic;

namespace Ezhtellar.AI
{
    public interface IState
    {
        public string Name { get; }
        public IState Parent { get; }
        public IEnumerable<Transition> Transitions { get; }

        public void Start();
        public void Stop();
        public void Update();
        public void Transition(IState to);
        
        public void SetParent(IState parent);
        
        public void AddTransition(Transition transition);
    }
}