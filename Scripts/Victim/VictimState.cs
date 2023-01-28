using System;
using System.Collections.Generic;
using Godot;

public interface IVictimState : IMachineState<IVictimState> { }

public record Wandering : IVictimState
{
    public bool CanTransitionTo(IVictimState state) => state is Wandering ||
                                                       state is Waiting;
}

public record Waiting : IVictimState
{
    public bool CanTransitionTo(IVictimState state) => state is Wandering;
}

