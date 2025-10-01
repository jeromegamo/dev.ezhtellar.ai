using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
                Transition(ActiveChild, t.Target);
                return;
            }
            
            ActiveChild?.Update();
            m_OwnState.Update();
        }

        public void Transition(IState to)
        {
            Transition(this, to);
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
        
        private void Transition(IState from, IState to)
        {
            var lca = FindLowestCommonAncestor(from, to);
            
            Stack<IState> stack = new Stack<IState>();
            IState currentEnterNode = to;
            
            if (lca != null)
            {
                // exit from this state up to the lca not including lca
                IState currentExitNode = from;
                while (currentExitNode != lca)
                { 
                    currentExitNode.Stop(); 
                    currentExitNode = currentExitNode.Parent; 
                }
                
            }
            
            // build the node path from target up to the lca or root
            // if it is null then it is the root
            while (currentEnterNode != lca?.Parent)
            {
                stack.Push(currentEnterNode); 
                currentEnterNode = currentEnterNode.Parent;
            }

            // start all nodes up to the target
            while (stack.Count > 0)
            {
                var state = stack.Pop();
                if (state is StateMachine machine)
                {
                    machine.SetActiveChild(stack.Peek());
                }
                state.Start();
            }
        }

        private IState FindLowestCommonAncestor(IState from, IState to)
        {
            var ancestors = new HashSet<IState>();
            // walk up the three up to root not including the root
            IState current = from.Parent;
            while (current != null)
            {
                ancestors.Add(current);
                current = current.Parent;
            }

            IState targetAncestor = to.Parent;
            while (targetAncestor != null)
            {
                if (ancestors.Contains(targetAncestor))
                {
                    return targetAncestor;
                }

                targetAncestor = targetAncestor.Parent;
            }

            return null;
        }

        public string PrintActivePath()
        {
            IState current = this;
            var path = new List<string>();
            path.Add(Name);
            while (current != null)
            {
                switch (current)
                {
                   case StateMachine machine:
                       current = machine.ActiveChild;
                       path.Add(current.Name);
                       break;
                   case State:
                       current = null;
                       break;
                }
            }

            return string.Join(" > ", path.ToArray());

        }
    }
}