using System;
using System.Collections.Generic;
using System.Linq;

namespace Ezhtellar.AI
{
    public class State: IState
    {
        private Action m_onEnter;
        private Action m_onUpdate;
        private Action m_onExit;
        private List<Transition> m_transitions;
        
        public string Name { get; private set; }
        private StateMachine m_parent;
        public IState Parent => m_parent;
        public IEnumerable<Transition> Transitions => m_transitions;
        
        private State(Builder builder)
        {
            Name = builder.Name;
            m_onEnter = builder.OnEnter;
            m_onUpdate = builder.OnUpdate;
            m_onExit = builder.OnExit;
            m_transitions = new List<Transition>();
        }

        public void SetParent(IState parent)
        {
            if (parent is StateMachine stateMachine)
            {
                m_parent = stateMachine;
            }
            else
            {
                throw new InvalidOperationException($"{parent.Name} is not a state machine");
            }
        }

        public void AddTransition(Transition transition)
        {
            m_transitions.Add(transition);
        }

        public void Start()
        {
            m_onEnter?.Invoke();
        } 
        public void Stop() { m_onExit?.Invoke(); } 
        public void Update() { m_onUpdate?.Invoke(); }
        
        public void Transition(IState to)
        {
            Parent?.Transition(to);
        } 
        
        public class Builder
        {
            public string Name { get; private set; }
            
            public Action OnEnter { get; private set; }
            public Action OnUpdate { get; private set; }
            public Action OnExit { get; private set; }
            public Builder WithName(string name) { Name = name; return this; }
            public Builder WithOnEnter(Action onEnter) { OnEnter = onEnter; return this; }
            public Builder WithOnUpdate(Action onUpdate) { OnUpdate = onUpdate; return this; }
            public Builder WithOnExit(Action onExit) { OnExit = onExit; return this; }
            
            public State Build()
            {
                return new State(this);
            }
        }
        
    }
}