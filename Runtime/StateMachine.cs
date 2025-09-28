using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Ezhtellar.AI
{
    public class StateMachine: IState
    {
        private List<IState> m_states;
        private IState m_OwnState;
        private IState m_initialState;

        public string Name => m_OwnState.Name;
        public IState Parent => m_OwnState.Parent;
        public IEnumerable<Transition> Transitions => m_OwnState.Transitions;
        public IEnumerable<IState> States => m_states;
        public IState ActiveChild { get; private set; }

        public static StateMachine FromState(State state)
        {
            return new StateMachine(state);
        }

        public void SetActiveChild(IState child) { ActiveChild = child; }

        public void Start()
        {
            if (Parent != null && Parent is StateMachine parent)
            {
                parent.SetActiveChild(this);
            }
            
            if (m_initialState == null)
            {
                throw new InvalidOperationException($"No initialState found for {m_OwnState.Name}");
            }
            
            if (ActiveChild == null)
            {
                throw new InvalidOperationException($"{Name} has no active child");
            }

            m_OwnState.Start();
            ActiveChild?.Start();
        }

        public void Stop()
        {
            ActiveChild?.Stop();
            m_OwnState.Stop();
        }

        public void Update()
        {
            foreach (Transition t in ActiveChild.Transitions)
            {
                if (!t.Condition()) { continue; }
                ActiveChild.Stop();
                ActiveChild = t.Target;
                ActiveChild.Start();
                break;
            }
            
            ActiveChild?.Update();
            m_OwnState.Update();
        }

        public void Transition(IState to)
        {
            m_OwnState.Transition(to);
        }


        public void SetParent(IState parent)
        {
            m_OwnState.SetParent(parent);
        }

        public void AddTransition(Transition transition)
        {
            m_OwnState.AddTransition(transition);
        }

        public void AddState(IState state, bool isInitial = false)
        {
            state.SetParent(this);
            m_states.Add(state);
            if (!isInitial) { return; }
            
            if (m_initialState != null)
            {
                throw new ArgumentException($@"
                        {state.Name} is being set as initial state when
                        {m_initialState.Name} is the current
                    ");
            }
            
            m_initialState = state;
            SetActiveChild(m_initialState);
        }
        
        private StateMachine(IState state)
        {
            m_OwnState = state;
            m_states = new List<IState>();
        }
    }
}