using Godot;
using static Utils;

public class Game : Node
{
    private StateMachine<IGameState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private Player _player;
    private const string _playerName = "Player";

    private HUD _hud;
    private const string _hudName = "HUD";
#pragma warning disable CS8618 // Non-nullable field

    public override void _Ready()
    {
        _stateMachine = new StateMachine<IGameState>(new Playing(), OnStateChanged);
        _player = GetOrThrow<Player>(this, _playerName);
        _hud = GetOrThrow<HUD>(this, _hudName);

        ConnectEvents();
    }

    public override void _ExitTree()
    {
        DisconnectEvents();
    }

    private void ConnectEvents()
    {
        _player.HealthChanged += HandlePlayerHealthChanged;
    }

    private void DisconnectEvents()
    {
        _player.HealthChanged -= HandlePlayerHealthChanged;
    }

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
    }
}

