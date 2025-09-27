using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ezhtellar.Funk;
using Ezhtellar.AI;

namespace Tests.Runtime
{
 
    public class StateMachineTests
    {
    
        [Test]
        public void ShouldThrowIfInitialStateNotFound()
        {
            var machine = new StateMachine(
                name: "Alive",
                initialState: "Idle"
            );
        
            Assert.Throws<InvalidOperationException>(() => machine.Start());
        }
        
        [Test]
        public void ShouldCallStateChildOnEnter()
        {
            var childCalled = false;
            var parentCalled = false;
            var machine = new StateMachine(
                name: "Alive",
                initialState: "Idle",
                onEnter: () => { parentCalled = true; }
            );

            var state = new State(
                name: "Idle",
                onEnter: () => { childCalled = true; }
            );
            
            machine.AddState(state);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            machine.Start();
            Assert.AreEqual("Idle", machine.ActiveChild.Name);
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }
        

        [Test]
        public void ShouldCallStateChildOnExit()
        {
            var childCalled = false;
            var parentCalled = false;
            var machine = new StateMachine(
                name: "Alive",
                initialState: "Idle",
                onExit: () => { parentCalled = true; }
            );

            var state = new State(
                name: "Idle",
                onExit: () => { childCalled = true; }
            );
            
            machine.AddState(state);
            
            machine.Start();
            Assert.AreEqual("Idle", machine.ActiveChild.Name);
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            machine.Stop();
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }

        [Test]
        public void ShouldCallStateChildOnUpdate()
        {
            var childCalled = false;
            var parentCalled = false;
            var machine = new StateMachine(
                name: "Alive",
                initialState: "Idle",
                onUpdate: () => { parentCalled = true; }
            );

            var state = new State(
                name: "Idle",
                onUpdate: () => { childCalled = true; }
            );
            
            machine.AddState(state);
            
            Assert.IsFalse(childCalled);
            Assert.IsFalse(parentCalled);
            machine.Start();
            Assert.AreEqual("Idle", machine.ActiveChild.Name);
            machine.Update();
            Assert.IsTrue(childCalled);
            Assert.IsTrue(parentCalled);
        }
    }
}