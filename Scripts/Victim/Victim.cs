using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public class Victim : KinematicBody2D
{
    private StateMachine<IVictimState>? _stateMachine;

    private Stack<Vector2>? _currentPath;

    private readonly List<BodyPart> _bodyParts = BodyParts.All();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IVictimState>(new Waiting(), OnStateChanged);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_currentPath?.Count > 0)
        {
            MoveAndSlide(_currentPath.Pop());
        }
    }

    private void OnStateChanged(IVictimState previousState, IVictimState newState)
    { 
        switch(newState)
        {
            case Wandering:
                _currentPath = new Stack<Vector2>(
                                        Navigation2DServer.MapGetPath(
                                            map:GetWorld2d().NavigationMap,
                                            origin: GlobalPosition,
                                            destination: new Vector2(),
                                            optimize: false,
                                            navigationLayers: 1
                                        )
                );
                break;
        }
    }

    public List<BodyPart> GetAvailableBodyParts() => _bodyParts;

    public void TakeBodyPart(BodyPart taken)
    {
        var bodyPart = _bodyParts.FirstOrDefault(b => b.Name == taken.Name);
        if (bodyPart != null)
        {
            _bodyParts.Remove(bodyPart);
        }
    }
}
