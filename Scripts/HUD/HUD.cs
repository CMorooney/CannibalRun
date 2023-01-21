using Godot;
using System;

public class HUD : CanvasLayer
{
   [Signal]
    private delegate void HealthChanged(float newValue);

    private float _healthValue = Constants.Player.MaxHealth;

#pragma warning disable CS8618 // Non-nullable field

    private TextureProgress _healthIndicator;
    private const string _indicatorName = "Health";

#pragma warning restore CS8618 // Non-nullable field

    public override void _Ready()
    {
        _healthIndicator = GetNode<TextureProgress>($"{_indicatorName}");
        if (_healthIndicator == null)
        {
            var message = $"[{nameof(HUD)} : {nameof(_Ready)}] Couldn't find health indicator by name of {_indicatorName}. Exiting.";
            GD.Print(message);
            throw new ApplicationException(message);
        }
    }

    public void AddHealth(float amount)
    {
        _healthValue = Math.Max(Constants.Player.MinHealth,
	                            Math.Min(amount, Constants.Player.MaxHealth));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        float healthPercent = _healthValue / Constants.Player.MaxHealth;
        _healthIndicator.Value = _healthIndicator.MaxValue * healthPercent;
        EmitSignal(nameof(HealthChanged), _healthValue);
    }
}
