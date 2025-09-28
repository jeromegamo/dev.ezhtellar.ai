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
            var machine = new StateMachine.Builder()
                .WithName("Alive")
                .WithInitialState("Idle")
                .Build();
        
            Assert.Throws<InvalidOperationException>(() => machine.Start());
        }
        
        [Test]
        public void ShouldCallStateChildOnEnter()
        {
            var childCalled = false;
            var parentCalled = false;
            var root = new StateMachine.Builder()
                .WithName("Root")
                .WithInitialState("Idle")
                .WithOnEnter(() => { parentCalled = true; })
                .Build();

            var idle = new State.Builder()
                .WithName("Idle")
                .WithOnEnter(() => { childCalled = true; })
                .Build();
            
            root.AddState(idle);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            root.Start();
            Assert.AreEqual("Idle", root.ActiveChild.Name);
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
            
            var root = new StateMachine.Builder()
                .WithName("Alive")
                .WithInitialState("Idle")
                .WithOnExit(() => { parentCalled = true; })
                .Build();
            
            root.AddState(idle);
            root.Start();
            Assert.AreEqual("Idle", root.ActiveChild.Name);
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            root.Stop();
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }

        [Test]
        public void ShouldCallStateChildOnUpdate()
        {
            var childCalled = false;
            var parentCalled = false;
            
            var root = new StateMachine.Builder()
                .WithName("Root")
                .WithInitialState("Idle")
                .WithOnUpdate(() => { parentCalled = true; })
                .Build();

            var state = new State.Builder()
                .WithName("Idle")
                .WithOnUpdate(() => { childCalled = true; })
                .Build();
            
            root.AddState(state);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            root.Start();
            Assert.AreEqual("Idle", root.ActiveChild.Name);
            root.Update();
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
            
            var movement = new StateMachine.Builder()
                .WithName("Movement")
                .Build();
            
            Assert.Throws<InvalidOperationException>(() => movement.SetParent(running));
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
            
            var root = new StateMachine.Builder()
                .WithName("Root")
                .WithInitialState("Idle")
                .Build();
            
            root.AddState(idle);
            root.AddState(running);
            
            root.Start();
            Assert.AreEqual("Idle", root.ActiveChild.Name);
            root.Update();
            Assert.IsTrue(idleOnExitCalled);
            Assert.IsTrue(runningOnEnterCalled);
            Assert.AreEqual("Running", root.ActiveChild.Name);
        }

        [Test]
        public void ShouldTransitionToParent()
        {
            var movementIdle = new State.Builder()
                .WithName("Movement.Idle")
                .Build();
            
            var running = new State.Builder()
                .WithName("Running")
                .Build();
            
            var movement = new StateMachine.Builder()
                .WithName("Movement")
                .WithInitialState("Movement.Idle")
                .Build();
            
            movement.AddState(movementIdle);
            movement.AddState(running);

            var combatIdle = new State.Builder()
                .WithName("Combat.Idle")
                .Build();
            var attacking = new State.Builder()
                .WithName("Attacking")
                .Build();
            
            var combat = new StateMachine.Builder()
                .WithName("Combat")
                .WithInitialState("Combat.Idle")
                .Build();
            
            combat.AddState(combatIdle);
            combat.AddState(attacking);
            
            var root = new StateMachine.Builder()
                .WithName("Root")
                .WithInitialState("Movement")
                .Build();
            
            root.AddState(movement);
            root.AddState(combat); 
            
            root.Start();
            
            Assert.AreEqual("Movement", root.ActiveChild.Name);
            Assert.AreEqual("Movement.Idle", movement.ActiveChild.Name);
            
            movementIdle.Transition(attacking);
            
            Assert.AreEqual("Attacking", combat.ActiveChild.Name);
        }
    }
}