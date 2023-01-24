using Godot;
using System;
using static Utils;

public class HUD : CanvasLayer
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TextureProgress _healthIndicator;
    private const string _indicatorName = "Health";
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private float _playerHealth = Constants.Player.MaxHealth;

    public override void _Ready()
    {
        _healthIndicator = GetOrThrow<TextureProgress>(this, $"{_indicatorName}");
    }

    public void SetPlayerHealth(float amount)
    {
        _playerHealth = amount;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        float healthPercent = _playerHealth / Constants.Player.MaxHealth;
       _healthIndicator.Value = _healthIndicator.MaxValue * healthPercent;
    }
}
