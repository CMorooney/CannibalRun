using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    [Export]
    public int Speed = 200;

    private bool _interactingWithVictim;
    private readonly List<IBodyPart> _bodyParts = BodyParts.All();

    private float _health = 1f;// 0 - 1 for now

    private Vector2 _velocity = new Vector2();

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

    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        _velocity = MoveAndSlide(_velocity);
    }
}
