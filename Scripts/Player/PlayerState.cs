using System;
using Godot;

public interface IPlayerState : IMachineState<IPlayerState> { }

public record OnTheProwl : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => true;
}

public record InteractingWithVictim : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => state is ConsumingFlesh ||
                                                       state is OnTheProwl     ||
	                                                   state is Dead;
}

public record ConsumingFlesh : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => state is GobblingFlesh ||
                                                       state is OnTheProwl ||
                                                       state is Dead;
}

public record GobblingFlesh : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) =>
                                                        state is ConsumingFlesh ||
                                                        state is OnTheProwl     ||
                                                        state is Dead;
}

public record Dead : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => !(state is Dead);
}
