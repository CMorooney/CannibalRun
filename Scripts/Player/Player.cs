using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public delegate void Died();

public class Player : KinematicBody2D
{
    public event Died? Died;

    [Export]
    public int Speed = 200;

    [Export]
    public float HealthPerBite = 0.01f;

    [Export]
    public float HealthPerFrame = -0.0007f;

    [Export]
    public int ForwardCollisionThreshold = 14;

    [Export]
    public int SidewaysCollisionThreshold = 6;

    private StateMachine<IPlayerState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private HUD _HUD;
    private const string _hudName = "HUD";

    private List<RayCast2D> _rayCasts;
#pragma warning restore CS8618 // Non-nullable field

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = Constants.Player.MaxHealth;

    private bool _blocked = false;
    private Vector2 _velocity = new Vector2();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IPlayerState>(new OnTheProwl(), OnStateChanged);

        _HUD = GetOrThrow<HUD>(GetParent(), _hudName);

        var rayCast1 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}");
        var rayCast2 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}2");
        var rayCast3 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}3");
        _rayCasts = new List<RayCast2D>()
        {
            rayCast1, rayCast2, rayCast3
        };

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
        SetVelocityToDirectionalInput();
        CheckForCollisions();
        CheckForInteractionInput();

        if (!_blocked && !(_stateMachine!.State is InteractingWithVictim))
        {
            _velocity = MoveAndSlide(_velocity);
        }
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

    private void ReduceHealth() => _HUD.AddHealth(HealthPerFrame);

    private void SetVelocityToDirectionalInput()
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

    private void CheckForCollisions()
    {
        var rc = _velocity.LimitLength(ForwardCollisionThreshold);

        var recast = false;

        // left / right
        if (Math.Abs(_velocity.x) > 0)
        {
            _rayCasts[0].Position = new Vector2(0, SidewaysCollisionThreshold);
            _rayCasts[2].Position = new Vector2(0, -SidewaysCollisionThreshold);
            recast = true;
        }

        // up / down
        if(Math.Abs(_velocity.y) > 0)
        { 
            _rayCasts[0].Position = new Vector2(SidewaysCollisionThreshold, 0);
            _rayCasts[2].Position = new Vector2(-SidewaysCollisionThreshold, 0);
            recast = true;
        }

        _blocked = false;

        foreach (var cast in _rayCasts)
        {
            if(recast)
            {
                cast.ForceRaycastUpdate();
                cast.CastTo = new Vector2(rc.x, rc.y);
            }
            if (cast.IsColliding())
            {
                HandleCollision(cast.GetCollider());
            }
        }
    }

    private void HandleCollision(object collider)
    {
        switch(collider)
        {
            case KinematicBody2D kBody:
                _blocked = true;
                break;
            case TileMap tileMap:
                _blocked = true;
                break;
        };
    }

    private void CheckForInteractionInput()
    {
        if (Input.IsActionPressed("interact"))
        {
            switch (_stateMachine!.State)
            {
                case OnTheProwl:
                    var target = _rayCasts.Where(c => c.IsColliding())
                                          .Select(c => c.GetCollider())
                                          .FirstOrDefault();
                    if(target != null && target is Victim victim)
                    {
                        _stateMachine.Update(new InteractingWithVictim());
                        victim.ShowMenu(BodyPartTaken);
                    }
                    break;
                case ConsumingFlesh fleshState:
                    var food = fleshState.BodyPart;
                    food.Value -= HealthPerBite;
                    _HUD.AddHealth(HealthPerBite);
                    if (food.Value > 0)
                    {
                        _stateMachine!.Update(new ConsumingFlesh(food));
                    }
                    else
                    {
                        _stateMachine!.Update(new OnTheProwl());
                    }

                    break;
            }
        }
    }

    private void BodyPartTaken(IBodyPart bodyPart) => _stateMachine!.Update(new ConsumingFlesh(bodyPart));

    private void HealthChanged(float newValue)
    {
        _health = newValue;

        if (_health <= Constants.Player.MinHealth)
        {
            _stateMachine!.Update(new Dead());
        }
    }
}
