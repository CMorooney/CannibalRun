using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public class Victim : KinematicBody2D
{
    private StateMachine<IVictimState>? _stateMachine;

    private Stack<Vector2>? _currentPath;
    private Action<IBodyPart>? _callback;

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

#pragma warning disable CS8618 // Non-nullable field
    private ActionMenu _actionMenu;
#pragma warning restore CS8618 // Non-nullable field

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IVictimState>(new Waiting(), OnStateChanged);
        _actionMenu = GetOrThrow<ActionMenu>(this, nameof(ActionMenu));
        ConnectEvents();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_currentPath?.Count > 0)
        {
            MoveAndSlide(_currentPath.Pop());
        }
        else if(!(_stateMachine!.State is Waiting))
        { 

        }
    }

    public override void _ExitTree() => DisconnectEvents();

    private void ConnectEvents()
    {
        _actionMenu.ItemSelected += MenuItemSelected;
    }

    private void DisconnectEvents()
    { 
        _actionMenu.ItemSelected -= MenuItemSelected;
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

    private void MenuItemSelected(string item)
    {
        var bodyPart = _bodyParts.FirstOrDefault(b => b.Name == item);

        if (_callback != null)
        {
            _callback(bodyPart);
        }

        _bodyParts.Remove(bodyPart);
        _callback = null;
    }

    public void ShowMenu(Action<IBodyPart> callback)
    {
        _stateMachine!.Update(new Wandering());
        _actionMenu.Show(_bodyParts.Select(b => b.Name).ToHashSet<string>());
        _callback = callback;
    }
}
