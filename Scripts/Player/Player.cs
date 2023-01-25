using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public delegate void HealthChanged(float newValue);
public delegate void InventoryChanged(IBodyPart? bodyPart);

public class Player : KinematicBody2D
{
    public event HealthChanged? HealthChanged;
    public event InventoryChanged? InventoryChanged;

    [Export]
    public int Speed = 200;

    [Export]
    public float EatingSpeedModifier = 0.25f;

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
    private List<RayCast2D> _rayCasts;
#pragma warning restore CS8618 // Non-nullable field

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = Constants.Player.MaxHealth;

    private bool _blocked = false;
    private Vector2 _velocity = new Vector2();

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IPlayerState>(new OnTheProwl(), OnStateChanged);

        var rayCast1 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}");
        var rayCast2 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}2");
        var rayCast3 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}3");
        _rayCasts = new List<RayCast2D>()
        {
            rayCast1, rayCast2, rayCast3
        };
    }

    public override void _PhysicsProcess(float delta)
    {
        AddHealth(HealthPerFrame);
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
        switch(newState)
        {
            case ConsumingFlesh fleshState:
                InventoryChanged?.Invoke(fleshState.BodyPart);
                break;

            default:
                InventoryChanged?.Invoke(null);
                break;
        }
    }

    private void AddHealth(float amount)
    {
        _health = Math.Max(Constants.Player.MinHealth,
                    Math.Min(_health + amount, Constants.Player.MaxHealth));

        HealthChanged?.Invoke(_health);
        if(_health <= 0)
        {
            _stateMachine!.Update(new Dead());
        }
    }

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

        var modifier = _stateMachine!.State is ConsumingFlesh ? EatingSpeedModifier : 1;
        _velocity = _velocity.Normalized() * (Speed * modifier);
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
                    food.Health -= HealthPerBite;
                    if (food.Health > 0)
                    {
                        _stateMachine!.Update(new ConsumingFlesh(food));
                    }
                    else
                    {
                        _stateMachine!.Update(new OnTheProwl());
                    }

                    // probably best to call this last here since
                    // it may also queue a state update (e.g. `Dead`)
                    AddHealth(HealthPerBite);

                    break;
            }
        }
    }

    private void BodyPartTaken(IBodyPart bodyPart) => _stateMachine!.Update(new ConsumingFlesh(bodyPart));
}
