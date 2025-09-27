using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Ezhtellar.AI
{
    public interface IState
    {
        public string Name { get; }
        public IState Parent { get; }
        
        public void SetParent(IState parent);

        public void Start();
        public void Stop();
        public void Update();
    }
    public class StateMachine: IState
    {
        private List<IState> m_states;
        private Action onEnter;
        private Action onUpdate;
        private Action onExit;
        private string m_initialState;
        
        public string Name { get; private set; }
        public IState Parent { get; private set; }
        public IState ActiveChild { get; private set; }
        
        public StateMachine(
            string name,
            string initialState,
            Action onEnter = null, 
            Action onUpdate = null, 
            Action onExit = null)
        {
            Name = name;
            m_initialState = initialState;
            m_states = new List<IState>();
            this.onEnter = onEnter;
            this.onUpdate = onUpdate;
            this.onExit = onExit;
        }

        public void SetParent(IState parent) { Parent = parent; }

        public void Start()
        {
            IState initialState = m_states.First(s => s.Name == m_initialState);
            ActiveChild = initialState ?? throw new InvalidOperationException($"{m_initialState} is not found.");
            this.onEnter?.Invoke();
            ActiveChild.Start();
        }

        public void Stop()
        {
            ActiveChild?.Stop();
            onExit?.Invoke();
        }

        public void Update()
        {
            ActiveChild?.Update();
            onUpdate?.Invoke();
        }
        
        public void AddState(IState state)
        {
            state.SetParent(this);
            m_states.Add(state);
        }
    }

    public class State: IState
    {
        public string Name { get; private set; }
        public IState Parent { get; private set; }

        private Action onEnter;
        private Action onUpdate;
        private Action onExit;
        
        public State(
            string name, 
            Action onEnter = null, 
            Action onUpdate = null, 
            Action onExit = null)
        {
            Name = name;
            this.onEnter = onEnter;
            this.onUpdate = onUpdate;
            this.onExit = onExit;
        }

        public void SetParent(IState parent)
        {
            Parent = parent;
        }

        public void Start() { onEnter?.Invoke(); } 
        public void Stop() { onExit?.Invoke(); } 
        public void Update() { onUpdate?.Invoke(); }
    }
}