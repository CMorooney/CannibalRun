using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public delegate void HealthChanged(float newValue);
public delegate void InventoryChanged(BodyPart? bodyPart);

public class Player : KinematicBody2D
{
    public event HealthChanged? HealthChanged;
    public event InventoryChanged? InventoryChanged;

    #region Exported Properties

    [Export]
    public int Speed = 200;

    [Export]
    public float EatingSpeedModifier = 0.25f;

    [Export]
    public float GobbleThreshold = 100;// in ms

    [Export]
    public float GobbleHealthModifer = 0.5f;

    [Export]
    public float HealthPerBite = 0.01f;

    [Export]
    public float HealthPerFrame = -0.0007f;

    [Export]
    public int ForwardCollisionThreshold = 14;

    [Export]
    public int SidewaysCollisionThreshold = 6;

    #endregion

    private StateMachine<IPlayerState>? _stateMachine;


#pragma warning disable CS8618 // Non-nullable field
    private List<RayCast2D> _rayCasts;
    private AnimatedSprite _animatedSprite;
#pragma warning restore CS8618 // Non-nullable field

    // going to instantiate this here rather than scene view since it's generic
    private ActionMenu<BodyPart> _actionMenu = new ActionMenu<BodyPart>();

    private float _health = Constants.Player.MaxHealth;

    private long _lastGobbleButtonPress;

    private bool _blocked = false;

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IPlayerState>(new OnTheProwl(), OnStateChanged);

        _animatedSprite = GetOrThrow<AnimatedSprite>(this, nameof(AnimatedSprite));

        AddChild(_actionMenu);
        _actionMenu.Owner = this;

        var rayCast1 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}");
        var rayCast2 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}2");
        var rayCast3 = GetOrThrow<RayCast2D>(this, $"{nameof(RayCast2D)}3");
        _rayCasts = new List<RayCast2D>()
        {
            rayCast1, rayCast2, rayCast3
        };

        ConnectEvents();
    }

    public override void _ExitTree() => DisconnectEvents();

    private void ConnectEvents() => _actionMenu.ItemSelected += ActionMenuItemSelected;
    private void DisconnectEvents() => _actionMenu.ItemSelected -= ActionMenuItemSelected;

    public override void _PhysicsProcess(float delta)
    {
        AddHealth(HealthPerFrame);

        var velocity = GetVelocityForInput();
        CheckForCollisions(velocity);

        CheckForInteractionInput();

        if (!_blocked                       &&
            !(_stateMachine!.State is Dead) &&
            !(_stateMachine!.State is InteractingWithVictim))
        {
            MoveAndSlide(velocity);
        }
    }

    private void OnStateChanged(IPlayerState previousState, IPlayerState newState)
    {
        switch(newState)
        {
            case ConsumingFlesh fleshState:
                InventoryChanged?.Invoke(fleshState.BodyPart);
                break;
        }

        if(previousState is ConsumingFlesh && !(newState is ConsumingFlesh))
        { 
            InventoryChanged?.Invoke(null);
        }
    }

    #region Movement/Collisions

    private Vector2 GetVelocityForInput()
    {
        var velocity = new Vector2();

        if (Input.IsActionPressed("right"))
        {
            _animatedSprite.Play("idle-side");
            _animatedSprite.FlipH = false;
            velocity.x += 1;
        }

        if (Input.IsActionPressed("left"))
        {
            _animatedSprite.Play("idle-side");
            _animatedSprite.FlipH = true;
            velocity.x -= 1;
        }

        if (Input.IsActionPressed("down"))
        {
            _animatedSprite.Play("idle-front");
            velocity.y += 1;
        }

        if (Input.IsActionPressed("up"))
        {
            _animatedSprite.Play("idle-back");
            velocity.y -= 1;
        }

        var gobbling = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastGobbleButtonPress < GobbleThreshold;
        var modifier = _stateMachine!.State is ConsumingFlesh && !gobbling ? EatingSpeedModifier : 1;

        return velocity.Normalized() * (Speed * modifier);
    }

    private void CheckForCollisions(Vector2 velocity)
    {
        var rc = velocity.LimitLength(ForwardCollisionThreshold);

        var recast = false;

        // left / right
        if (Math.Abs(velocity.x) > 0)
        {
            _rayCasts[0].Position = new Vector2(0, SidewaysCollisionThreshold);
            _rayCasts[2].Position = new Vector2(0, -SidewaysCollisionThreshold);
            recast = true;
        }

        // up / down
        if(Math.Abs(velocity.y) > 0)
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

    #endregion

    #region Interactions

    private void CheckForInteractionInput()
    {
        if (Input.IsActionPressed("interact"))
        {
            HandleInteractPressed();
        }
        else if (Input.IsActionPressed("interact2"))
        {
            HandleInteract2Pressed();
        }
    }

    private void HandleInteractPressed()
    {
        switch(_stateMachine!.State)
        {
            case OnTheProwl:
                var target = _rayCasts.Where(c => c.IsColliding())
                                      .Select(c => c.GetCollider())
                                      .FirstOrDefault();
                if (target != null && target is Victim victim)
                {
                    _stateMachine.Update(new InteractingWithVictim(victim));
                    _actionMenu.Show(victim.GetAvailableBodyParts().ToHashSet<BodyPart>());
                }
                break;
            case ConsumingFlesh fleshState:
                TakeBite(fleshState.BodyPart);
                break;
        }
    }

    private void HandleInteract2Pressed()
    {
        switch(_stateMachine!.State)
        {
            case ConsumingFlesh fleshState:
                _lastGobbleButtonPress = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                TakeBite(fleshState.BodyPart);
                break;
        }
    }

    #endregion

    private void ActionMenuItemSelected(BodyPart item)
    {
        if (_stateMachine!.State is InteractingWithVictim interactingState)
        {
            _stateMachine!.Update(new ConsumingFlesh(item));
            interactingState.Victim.TakeBodyPart(item);
        }
    }

    private void TakeBite(BodyPart bodyPart)
    {
        bodyPart.Health -= HealthPerBite;
        if (bodyPart.Health > 0)
        {
            _stateMachine!.Update(new ConsumingFlesh(bodyPart));
        }
        else
        {
            _stateMachine!.Update(new OnTheProwl());
        }

        AddHealth(HealthPerBite * GobbleHealthModifer);
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

    private void BodyPartTaken(BodyPart bodyPart) => _stateMachine!.Update(new ConsumingFlesh(bodyPart));
}
