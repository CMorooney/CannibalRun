using Godot;
using System;
using static Utils;

public delegate void HealthChanged(float newValue);

public class HUD : CanvasLayer
{
    public event HealthChanged? HealthChanged;

    private float _healthValue = Constants.Player.MaxHealth;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TextureProgress _healthIndicator;
    private const string _indicatorName = "Health";
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void _Ready()
    {
        _healthIndicator = GetOrThrow<TextureProgress>(this, $"{_indicatorName}");
    }

    public void AddHealth(float amount)
    {
        _healthValue = Math.Max(Constants.Player.MinHealth,
	                            Math.Min(_healthValue + amount, Constants.Player.MaxHealth));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        float healthPercent = _healthValue / Constants.Player.MaxHealth;
        _healthIndicator.Value = _healthIndicator.MaxValue * healthPercent;
        HealthChanged?.Invoke(_healthValue);
    }
}
