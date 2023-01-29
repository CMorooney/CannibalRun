using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public delegate void DestinationReached(Victim victim, Vector2 position);

public class Victim : KinematicBody2D
{
    public event DestinationReached? DestinationReached;

    private StateMachine<IVictimState>? _stateMachine;

    private readonly List<BodyPart> _bodyParts = BodyParts.All();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IVictimState>(new Waiting(), OnStateChanged);
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(_stateMachine!.State)
        {
            case Wandering wanderingState:
                if (wanderingState.Path?.Count <= 0)
                {
                    _stateMachine!.Update(new Waiting());
                }
                else if(wanderingState.Path != null)
                {
                    MoveAndSlide(ToLocal(wanderingState.Path.Pop()));
                    _stateMachine!.Update(new Wandering(wanderingState.Path));
                }
                break;
        }
    }

    private void OnStateChanged(IVictimState oldState, IVictimState newState)
    { 
        switch(newState)
        {
            case Waiting:
                DestinationReached?.Invoke(this, GlobalPosition);
                break;
        }
    }

    private Vector2[] CreateNewPath(Vector2 destination) =>
        Navigation2DServer.MapGetPath(
            map: GetWorld2d().NavigationMap,
            origin: GlobalPosition,
            destination: destination,
            optimize: false,
            navigationLayers: 1
        );

    public void SetNewDestination(Vector2 destination)
    {
        var path = new Stack<Vector2>(CreateNewPath(destination));
        _stateMachine!.Update(new Wandering(path));
    }

    public List<BodyPart> GetAvailableBodyParts() => _bodyParts;

    public BodyPart? TakeBodyPart(BodyPart toTake)
    {
        DestinationReached?.Invoke(this, GlobalPosition);
        var bodyPart = _bodyParts.FirstOrDefault(b => b.Name == toTake.Name);
        if (bodyPart != null)
        {
            _bodyParts.Remove(bodyPart);
            return bodyPart;
        }

        return null;
    }
}

