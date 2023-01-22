using Godot;
using System;
using System.Collections.Generic;
using static Utils;

public delegate void Died();

public class Player : KinematicBody2D
{
    public event Died? Died;

    [Export]
    public int Speed = 200;

    [Export]
    public int ForwardCollisionThreshold = 50;

    private StateMachine<IPlayerState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private HUD _HUD;
    private const string _hudName = "HUD";

    private RayCast2D _rayCast;
#pragma warning disable CS8618 // Non-nullable field

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = Constants.Player.MaxHealth;

    private Vector2 _velocity = new Vector2();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IPlayerState>(new OnTheProwl(), OnStateChanged);

        _HUD = GetOrThrow<HUD>(GetParent(), _hudName);
        _rayCast = GetOrThrow<RayCast2D>(this, nameof(RayCast2D));

        ConnectEvents();
    }

    public override void _ExitTree()
    {
        DisconnectEvents();
    }

    private void ConnectEvents()
    {
        _HUD.HealthChanged += HealthChanged;
    }

    private void DisconnectEvents()
    { 
        _HUD.HealthChanged -= HealthChanged;
    }

    public override void _PhysicsProcess(float delta)
    {
        ReduceHealth();
        GetInput();
        _velocity = MoveAndSlide(_velocity);
    }

    private void OnStateChanged(IPlayerState newState)
    { 
        switch (newState)
        {
            case Dead:
            Died?.Invoke();
                break;
        }
    }

    private void ReduceHealth()
    {
        _HUD.AddHealth(-0.00001f);
    }

    //TODO: break this up
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

        if (Math.Abs(_velocity.x) > 0 || Math.Abs(_velocity.y) > 0)
        {
            var rc = _velocity.LimitLength(20);
            _rayCast.CastTo = new Vector2(rc.x, rc.y);
        }
    }

    private void HealthChanged(float newValue)
    {
        _health = newValue;

        if (_health <= Constants.Player.MinHealth)
        {
            _stateMachine!.Update(new Dead());
        }
    }
}
