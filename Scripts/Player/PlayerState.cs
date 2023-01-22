using System;

public interface IPlayerState : IMachineState<IPlayerState> { }

public record OnTheProwl : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => true;
}

public record InteractingWithVictim : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) =>
                                                        state is OnTheProwl;
}

public record ConsumingFlesh : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) =>
                                                        state is InteractingWithVictim ||
                                                        state is GobblingFlesh;
}

public record GobblingFlesh : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) =>
                                                        state is ConsumingFlesh;
}

public record Dead : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => !(state is Dead);
}
