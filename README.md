This is a result of studying and making my own implementation of Hierarchical State Machine. Note that this is currently 
for personal use as it may evolve in the future based on my undertanding and needs. This is also not battle tested yet in
terms of performance. I am currently using this in my personal project ([dev.ezhtellar.genesis](https://github.com/jeromegamo/dev.ezhtellar.genesis)). 
There you will see how i am using it. You can also check the unit tests for this package. This is a UPM-compatible package 
and i haven't published it yet.

# How to use

You can create state machines without needing to create a class.

To only way to build the state is through the builder. This allows to
set only what is needed.
```csharp
var idle = new State.Builder()
    .WithName("Idle")
    .WithOnEnter(() => {})
    .WithOnExit(() => {})
    .WithOnUpdate(() => {})
    .Build();
```

To create a state machine, you create the state first and then convert it
to a state machine.
```csharp
var player = new State.Builder()
    .WithName("Player")
    .Build();

var playerMachine = StateMachine.FromState(player);
```
and then add the states to the machine. Note that you need to specify which is
the intial state.
```csharp
var idle = new State.Builder()
    .WithName("Idle")
    .Build();
var moving = new State.Builder()
    .WithName("Moving")
    .Build();
            
playerMachine.AddState(idle, isInitial: true);
playerMachine.AddState(moving);
```
To configure how the states will transition between each other, you only need to
set the reference of the target state and the condition that triggers the transition
if the returned value is true.
```csharp
var idle = new State.Builder()
    .WithName("Idle")
    .Build();
var moving = new State.Builder()
    .WithName("Moving")
    .Build();

 idle.AddTransition(new Transition(moving, () => {...}));
 moving.AddTransition(new Transition(idle, () => {...}));
```
To force a transition, there is a `.Transition(targetRef)` method in State and StateMachine classes
```csharp
playerMachine.Transition(moving); // transition from root to a leaf

moving.Transition(idle) //or transition to any state or state machine
```
You can create a hierarchy of state machines.
```csharp
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
```
To use it in unity,

```csharp
class Player: MonoBehavior
{
    StateMachine m_machine;
    
    void Awake()
    {
        BuildMachine()
    }
    
    void Start()
    {
        m_machine.Start();
    }
    
    void OnDestroy()
    {
        m_machine.Stop();
    }
    
    void Update()
    {
        m_machine.Update();
    }
    
    void BuildMachine()
    {
       var player = new State.Builder()
            .WithName("Player")
            .Build();

       m_machine = StateMachine.FromState(player); 
    }
}
```
Right now, the only debugging helper available is to log the active path. 
This will be improved in the future.
```csharp
string m_lastActivePath = "";
void Update()
{
    var path = m_playerMachine.PrintActivePath();
    if (m_lastActivePath != path)
    {
        Debug.Log(path);
        m_lastActivePath = path;
    }
}
// output: Player > Movement > Idle
```

# Sample

```csharp
public void BuildPlayerMachine()
{
    m_playerMachine = StateMachine.FromState(new State.Builder()
        .WithName("Player")
        .WithOnEnter(() => Debug.Log("Player Machine Started"))
        .WithOnExit(() => Debug.Log("Player Machine Stopped"))
        .Build());

    var aliveMachine = StateMachine.FromState(new State.Builder()
        .WithName("Alive")
        .Build());

    var dead = new State.Builder()
        .WithName("Dead")
        .Build();

    m_playerMachine.AddState(aliveMachine, isInitial: true);
    m_playerMachine.AddState(dead);

    var idle = new State.Builder()
        .WithName("Idle")
        .Build();

    var movingToLocation = new State.Builder()
        .WithName("MovingToLocation")
        .WithOnEnter(() =>
        {
            if (m_targetMoveLocation.HasValue)
            {
                m_agent.SetDestination(m_targetMoveLocation.Value);
            }
        })
        .WithOnExit(() => m_targetMoveLocation = null)
        .Build();

    idle.AddTransition(new Transition(movingToLocation, () => m_targetMoveLocation.HasValue));

    movingToLocation.AddTransition(new Transition(idle,
        () => m_targetMoveLocation.HasValue && 
              Vector3.Distance(m_targetMoveLocation.Value, m_agent.transform.position) <= m_agent.stoppingDistance));

    var attacking = new State.Builder()
        .WithName("Attacking")
        .Build();

    aliveMachine.AddState(idle, isInitial: true);
    aliveMachine.AddState(movingToLocation);
    aliveMachine.AddState(attacking);
}
```