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
    }

    private void OnStateChanged(IGameState newState)
    {
    }
}

