using Godot;
using System;
using static Utils;

public class HUD : CanvasLayer
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TextureProgress _playerHealthIndicator;
    private const string _playerHealthIndicatorName = "PlayerHealth";

    private ProgressBar _organHealthIndicator;
    private const string _organHealthIndicatorName = "OrganHealth";

    private RichTextLabel _organLabel;
    private const string _organLabelName = "OrganText";
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private float _playerHealth = Constants.Player.MaxHealth;
    private IBodyPart? _bodyPart;

    public override void _Ready()
    {
        _playerHealthIndicator = GetOrThrow<TextureProgress>(this, _playerHealthIndicatorName);
        _organHealthIndicator = GetOrThrow<ProgressBar>(this, _organHealthIndicatorName);
        _organLabel = GetOrThrow<RichTextLabel>(this, _organLabelName);
    }

    public void SetPlayerHealth(float amount) => _playerHealth = amount;
    public void SetBodyPart(IBodyPart? bodyPart) => _bodyPart = bodyPart;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        UpdatePlayerHealthIndicator();
        UpdateOrganHealthIndicator();
    }

    private void UpdatePlayerHealthIndicator()
    { 
        float playerHealthPercent = _playerHealth / Constants.Player.MaxHealth;
        _playerHealthIndicator.Value = _playerHealthIndicator.MaxValue * playerHealthPercent;
    }

    private void UpdateOrganHealthIndicator()
    {
        var bodyPartExists = _bodyPart != null;

        _organHealthIndicator.Visible = bodyPartExists;
        _organLabel.Visible = bodyPartExists;

        if(bodyPartExists)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            float organHealthPrecent = _bodyPart.Health / _bodyPart.MaxHealth;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            _organHealthIndicator.Value = _organHealthIndicator.MaxValue * organHealthPrecent;
            _organLabel.Text = _bodyPart.Name;
        }
    }
}
