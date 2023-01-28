using System;
using Godot;

public interface IPlayerState : IMachineState<IPlayerState> { }

public record OnTheProwl : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => true;
}

public record InteractingWithVictim : IPlayerState
{
    public Victim Victim { get; private set; }

    public InteractingWithVictim(Victim victim)
    {
        Victim = victim;
    }

    public bool CanTransitionTo(IPlayerState state) => state is ConsumingFlesh ||
                                                       state is OnTheProwl     ||
                                                       state is Dead;
}

public record ConsumingFlesh : IPlayerState
{
    public BodyPart BodyPart { get; private set; }

    public ConsumingFlesh(BodyPart _bodyPart)
    {
        BodyPart = _bodyPart;
    }

   public bool CanTransitionTo(IPlayerState state) => state is OnTheProwl ||
                                                       state is Dead;
}

public record Dead : IPlayerState
{
    public bool CanTransitionTo(IPlayerState state) => !(state is Dead);
}
