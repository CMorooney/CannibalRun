using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    [Export]
    public int Speed = 200;

    [Signal]
    private delegate void Died();

    private bool _dead;

    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = Constants.Player.MaxHealth;

    private Vector2 _velocity = new Vector2();

    public override void _Ready()
    {
        ConnectToSignals();
    }

    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        _velocity = MoveAndSlide(_velocity);
    }

	private void GetInput()
    {
        _velocity = new Vector2
        {
            x = 0, y = 0
        };

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
