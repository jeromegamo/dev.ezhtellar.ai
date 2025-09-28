using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Ezhtellar.AI
{
    public class StateMachine: IState
    {
        private List<IState> m_states;
        private Action m_onEnter;
        private Action m_onUpdate;
        private Action m_onExit;
        private string m_initialState;
        private List<Transition> m_transitions;
        private StateMachine m_parent;
        
        public string Name { get; private set; }
        public IState Parent => m_parent;
        public IEnumerable<IState> States => m_states;
        
        public IEnumerable<Transition> Transitions => m_transitions;
        public IState ActiveChild { get; private set; }
        
        private StateMachine(Builder builder)
        {
            Name = builder.Name;
            m_initialState = builder.InitialState;
            this.m_onEnter = builder.OnEnter;
            this.m_onUpdate = builder.OnUpdate;
            this.m_onExit = builder.OnExit;
            m_transitions = new List<Transition>();
            m_states = new List<IState>();
        }

        public void SetActiveChild(IState child) { ActiveChild = child; }

        public void Start()
        {
            if (ActiveChild == null)
            {
                throw new InvalidOperationException($"{Name} has no active child");
            }

            m_onEnter?.Invoke();

            ActiveChild?.Start();
        }

        public void Stop()
        {
            ActiveChild?.Stop();
            m_onExit?.Invoke();
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
            m_onUpdate?.Invoke();
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
            transition.SetOrigin(this);
            m_transitions.Add(transition);
        }

        public void AddState(IState state)
        {
            if (m_initialState == state.Name)
            {
                SetActiveChild(state);
            }
            state.SetParent(this);
            m_states.Add(state);
        }
        
        public void Transition(IState to)
        {
            var lca = FindLowestCommonAncestor(to);
            if (lca != null)
            {
                // exit from this state up to the lca not including lca
                IState currentExitNode = this;
                while (currentExitNode != lca)
                { 
                    currentExitNode.Stop(); 
                    currentExitNode = currentExitNode.Parent; 
                }
                
                Stack<IState> stack = new Stack<IState>();
                IState currentEnterNode = to;
                while (currentEnterNode != lca.Parent)
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
        }

        private IState FindLowestCommonAncestor(IState to)
        {
            var ancestors = new HashSet<IState>();
            // walk up the three up to root not including the root
            IState current = Parent;
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
                else
                {
                    targetAncestor = targetAncestor.Parent;
                }
            }
            return null;
        }

        public class Builder
        {
            public string Name { get; private set; }
            public string InitialState { get; private set; }
            
            public Action OnEnter { get; private set; }
            public Action OnUpdate { get; private set; }
            public Action OnExit { get; private set; }
            public Builder WithName(string name) { Name = name; return this; }
            public Builder WithInitialState(string initialState) { InitialState = initialState; return this; }
            
            public Builder WithOnEnter(Action onEnter) { OnEnter = onEnter; return this; }
            public Builder WithOnUpdate(Action onUpdate) { OnUpdate = onUpdate; return this; }
            public Builder WithOnExit(Action onExit) { OnExit = onExit; return this; }
            
            public StateMachine Build()
            {
                return new StateMachine(this);
            }
        }
    }
}