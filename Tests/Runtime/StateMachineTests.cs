using System;
using Ezhtellar.AI;
using NUnit.Framework;

namespace Tests.Runtime
{
 
    public class StateMachineTests
    {
    
        [Test]
        public void ShouldThrowIfInitialStateNotFound()
        {
            var machineState = new State.Builder()
                .WithName("Alive")
                .Build();
            
            var machine = StateMachine.FromState(machineState);
        
            Assert.Throws<InvalidOperationException>(() => machine.Start());
            
            var idle = new State.Builder()
                .WithName("Idle")
                .Build();            
            
            machine.AddState(idle);
            
            Assert.Throws<InvalidOperationException>(() => machine.Start());
        }
        
        [Test]
        public void ShouldThrowIfInitialStateIsAlreadySet()
        {
            var machineState = new State.Builder()
                .WithName("Alive")
                .Build();
            
            var machine = StateMachine.FromState(machineState);
            
            var idle = new State.Builder()
                .WithName("Idle")
                .Build();            
            
            var running = new State.Builder()
                .WithName("Running")
                .Build();            
            
            machine.AddState(idle, isInitial: true);
            
            Assert.Throws<ArgumentException>(() => 
                machine.AddState(running, isInitial: true)
            );
        }       
        
        [Test]
        public void ShouldCallStateChildOnEnter()
        {
            var childCalled = false;
            var parentCalled = false;
            
            var rootState = new State.Builder()
                .WithName("Root")
                .WithOnEnter(() => { parentCalled = true; })
                .Build();
            
            var rootMachine = StateMachine.FromState(rootState);

            var idle = new State.Builder()
                .WithName("Idle")
                .WithOnEnter(() => { childCalled = true; })
                .Build();
            
            rootMachine.AddState(idle, isInitial: true);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            rootMachine.Start();
            Assert.AreEqual("Idle", rootMachine.ActiveChild.Name);
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }
        

        [Test]
        public void ShouldCallStateChildOnExit()
        {
            var childCalled = false;
            var parentCalled = false;
            
            var idle = new State.Builder()
                .WithName("Idle")
                .WithOnExit(() => { childCalled = true; })
                .Build();
            
            var rootState = new State.Builder()
                .WithName("Alive")
                .WithOnExit(() => { parentCalled = true; })
                .Build();
            
            var rootMachine = StateMachine.FromState(rootState);
            
            rootMachine.AddState(idle, isInitial: true);
            
            rootMachine.Start();
            Assert.AreEqual("Idle", rootMachine.ActiveChild.Name);
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            rootMachine.Stop();
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }

        [Test]
        public void ShouldCallStateChildOnUpdate()
        {
            var childCalled = false;
            var parentCalled = false;
            
            var rootState = new State.Builder()
                .WithName("Root")
                .WithOnUpdate(() => { parentCalled = true; })
                .Build();
            
            var rootMachine = StateMachine.FromState(rootState);

            var state = new State.Builder()
                .WithName("Idle")
                .WithOnUpdate(() => { childCalled = true; })
                .Build();
            
            rootMachine.AddState(state, isInitial: true);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            rootMachine.Start();
            Assert.AreEqual("Idle", rootMachine.ActiveChild.Name);
            rootMachine.Update();
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }

        [Test]
        public void ShouldThrowIfParentIsSetToNonMachine()
        {
            var idle = new State.Builder()
                .WithName("Idle")
                .Build();
            
            var running = new State.Builder()
                .WithName("Running")
                .Build();
            
            Assert.Throws<InvalidOperationException>(() => running.SetParent(idle));
            
            var movementState = new State.Builder()
                .WithName("Movement")
                .Build();
            
            var machine = StateMachine.FromState(movementState);
            
            Assert.Throws<InvalidOperationException>(() => machine.SetParent(running));
        }

        [Test]
        public void ShouldTransitionToSibling()
        {
            var idleOnExitCalled = false;
            var runningOnEnterCalled = false;
            
            var idle = new State.Builder()
                .WithName("Idle")
                .WithOnExit(() => { idleOnExitCalled = true; })
                .Build();
            
            var running = new State.Builder()
                .WithName("Running")
                .WithOnEnter(() => { runningOnEnterCalled = true; })
                .Build();
            
            idle.AddTransition(new Transition(running, () => true));
            
            var rootState = new State.Builder()
                .WithName("Root")
                .Build();
           
            var rootMachine = StateMachine.FromState(rootState);
            
            rootMachine.AddState(idle, isInitial: true);
            rootMachine.AddState(running);
            
            rootMachine.Start();
            Assert.AreEqual("Idle", rootMachine.ActiveChild.Name);
            rootMachine.Update();
            Assert.IsTrue(idleOnExitCalled);
            Assert.IsTrue(runningOnEnterCalled);
            Assert.AreEqual("Running", rootMachine.ActiveChild.Name);
        }

        [Test]
        public void ShouldTransitionToAnyStateFromLeaf()
        {
            var movementIdle = new State.Builder()
                .WithName("Movement.Idle")
                .Build();
            
            var running = new State.Builder()
                .WithName("Running")
                .Build();
            
            var movementState = new State.Builder()
                .WithName("Movement")
                .Build();
            
            var movement = StateMachine.FromState(movementState);
            
            movement.AddState(movementIdle, isInitial: true);
            movement.AddState(running);

            var combatIdle = new State.Builder()
                .WithName("Combat.Idle")
                .Build();
            var attacking = new State.Builder()
                .WithName("Attacking")
                .Build();
            
            var combatState = new State.Builder()
                .WithName("Combat")
                .Build();
            
            var combat = StateMachine.FromState(combatState);
            
            combat.AddState(combatIdle, isInitial: true);
            combat.AddState(attacking);
            
            var rootState = new State.Builder()
                .WithName("Root")
                .Build();
            
            var rootMachine = StateMachine.FromState(rootState);
            
            rootMachine.AddState(movement, isInitial: true);
            rootMachine.AddState(combat); 
            
            rootMachine.Start();
            
            Assert.AreEqual("Movement", rootMachine.ActiveChild.Name);
            Assert.AreEqual("Movement.Idle", movement.ActiveChild.Name);
            
            movementIdle.Transition(attacking);
            
            Assert.AreEqual("Attacking", combat.ActiveChild.Name);
        }
        
        [Test]
        public void ShouldTransitionToAnyStateFromRoot()
        {
            var movementIdle = new State.Builder()
                .WithName("Movement.Idle")
                .Build();
            
            var running = new State.Builder()
                .WithName("Running")
                .Build();
            
            var movementState = new State.Builder()
                .WithName("Movement")
                .Build();
            
            var movement = StateMachine.FromState(movementState);
            
            movement.AddState(movementIdle, isInitial: true);
            movement.AddState(running);

            var combatIdle = new State.Builder()
                .WithName("Combat.Idle")
                .Build();
            var attacking = new State.Builder()
                .WithName("Attacking")
                .Build();
            
            var combatState = new State.Builder()
                .WithName("Combat")
                .Build();
            
            var combat = StateMachine.FromState(combatState);
            
            combat.AddState(combatIdle, isInitial: true);
            combat.AddState(attacking);
            
            var rootState = new State.Builder()
                .WithName("Root")
                .Build();
            
            var rootMachine = StateMachine.FromState(rootState);
            
            rootMachine.AddState(movement, isInitial: true);
            rootMachine.AddState(combat); 
            
            rootMachine.Start();
            
            Assert.AreEqual("Movement", rootMachine.ActiveChild.Name);
            Assert.AreEqual("Movement.Idle", movement.ActiveChild.Name);
            
            rootMachine.Transition(attacking);
            
            Assert.AreEqual("Attacking", combat.ActiveChild.Name);
        } 
    }
}