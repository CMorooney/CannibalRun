using Godot;
using System;
using System.Collections.Generic;
using static Utils;

public class Player : KinematicBody2D
{
    [Export]
    public int Speed = 200;

    private StateMachine<IPlayerState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private HUD _HUD;
    private const string _hudName = "HUD";
#pragma warning disable CS8618 // Non-nullable field

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = Constants.Player.MaxHealth;

    private Vector2 _velocity = new Vector2();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IPlayerState>(new OnTheProwl(), OnStateChanged);

        _HUD = GetOrThrow<HUD>(GetParent(), _hudName);

        ConnectToSignals();
    }

    public override void _PhysicsProcess(float delta)
    {
        ReduceHealth();
        GetInput();
        _velocity = MoveAndSlide(_velocity);
    }

    private void OnStateChanged(IPlayerState newState)
    { 
    }

    private void ReduceHealth()
    {
        _HUD.AddHealth(-0.00001f);
    }

    private void GetInput()
    {
        _velocity = new Vector2();

        if (Input.IsActionPressed("right"))
        {
            _velocity.x += 1;
        }

        if (Input.IsActionPressed("left"))
        {
            _velocity.x -= 1;
        }

        if (Input.IsActionPressed("down"))
        {
            _velocity.y += 1;
        }

        if (Input.IsActionPressed("up"))
        {
            _velocity.y -= 1;
        }

        _velocity = _velocity.Normalized() * Speed;
    }

    private void HealthChanged(float newValue)
    {
        _health = newValue;

        if (_health <= Constants.Player.MinHealth)
        {
            if (!_dead)
            {
                _dead = true;
                EmitSignal(nameof(Died));
            }
	    }
    }

    private void ConnectToSignals()
    {
        Connect("HealthChanged", this, nameof(HealthChanged));
    }
}
