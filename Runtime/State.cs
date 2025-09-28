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
            else
            {
                // we are in the root
                Stack<IState> stack = new Stack<IState>();
                IState currentEnterNode = to;
                while (currentEnterNode != null)
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
                
                targetAncestor = targetAncestor.Parent;
            }
            return null;
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