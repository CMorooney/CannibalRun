using System;
using System.Collections.Generic;
using Godot;

public interface IVictimState : IMachineState<IVictimState> { }

public record Wandering : IVictimState
{
    public Stack<Vector2> Path { get; private set; }

    public Wandering(Stack<Vector2> path)
    {
        Path = path;
    }

    public bool CanTransitionTo(IVictimState state) => true;
}

public record Waiting : IVictimState
{
    public bool CanTransitionTo(IVictimState state) => true;
}

