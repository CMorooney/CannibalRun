using Godot;
using static Utils;

public class Game : Node
{
    private StateMachine<IGameState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private Player _player;
    private HUD _hud;
    private GameOverCanvas _gameOverCanvas;
#pragma warning disable CS8618 // Non-nullable field

    public override void _Ready()
    {
        _player = GetOrThrow<Player>(this, "Player");
        _hud = GetOrThrow<HUD>(this, "HUD");
        _gameOverCanvas = GetOrThrow<GameOverCanvas>(this, "GameOver");

        _stateMachine = new StateMachine<IGameState>(new Playing(), OnStateChanged);

        ConnectEvents();
    }

    public override void _ExitTree() => DisconnectEvents();

    private void ConnectEvents()
    {
        _player.HealthChanged += HandlePlayerHealthChanged;
        _player.InventoryChanged += PlayerInventoryChanged;
    }

    private void DisconnectEvents()
    {
        _player.HealthChanged -= HandlePlayerHealthChanged;
        _player.InventoryChanged -= PlayerInventoryChanged;
    }

    private void PlayerInventoryChanged(IBodyPart? bodyPart) => _hud.SetBodyPart(bodyPart);

    private void HandlePlayerHealthChanged(float newValue)
    {
        _hud.SetPlayerHealth(newValue);
        if (newValue <= 0)
        {
            _stateMachine!.Update(new GameOver());
        }
    }

    private void OnStateChanged(IGameState newState)
    {
        _gameOverCanvas.Visible = newState is GameOver;
    }
}

