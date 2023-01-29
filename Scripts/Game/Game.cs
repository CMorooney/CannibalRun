using Godot;
using static Utils;

public class Game : Node
{
    private StateMachine<IGameState>? _stateMachine;

#pragma warning disable CS8618 // Non-nullable field
    private Player _player;
    private Victim _victim;
    private TileMapsContainer _tileMapsContainer;
    private HUD _hud;
    private GameOverCanvas _gameOverCanvas;
#pragma warning disable CS8618 // Non-nullable field

    public override void _Ready()
    {
        _player = GetOrThrow<Player>(this, "Player");
        _victim = GetOrThrow<Victim>(this, "Victim");
        _tileMapsContainer = GetOrThrow<TileMapsContainer>(this, "LDtk");
        _hud = GetOrThrow<HUD>(this, "HUD");
        _gameOverCanvas = GetOrThrow<GameOverCanvas>(this, "GameOver");

        _stateMachine = new StateMachine<IGameState>(new Playing(), OnStateChanged);

        ConnectEvents();
    }

    private void VictimReachedWanderingDestination(Victim victim, Vector2 position)
    {
        var newDestination = _tileMapsContainer.GetRandomSidewalkTile();
        if (newDestination.HasValue)
        {
            victim.SetNewDestination(newDestination.Value);
        }
    }

    private void HandlePlayerHealthChanged(float newValue)
    {
        _hud.SetPlayerHealth(newValue);
        if (newValue <= 0)
        {
            _stateMachine!.Update(new GameOver());
        }
    }

    private void OnStateChanged(IGameState previousState, IGameState newState) => _gameOverCanvas.Visible = newState is GameOver;

    private void ExitGame() => GetTree().Quit();

    private void RestartGame() => GetTree().ReloadCurrentScene();

    private void PlayerInventoryChanged(BodyPart? bodyPart) => _hud.SetBodyPart(bodyPart);

    public override void _ExitTree() => DisconnectEvents();

    private void ConnectEvents()
    {
        _player.HealthChanged += HandlePlayerHealthChanged;
        _player.InventoryChanged += PlayerInventoryChanged;
        _victim.DestinationReached += VictimReachedWanderingDestination;
        _gameOverCanvas.Exit += ExitGame;
        _gameOverCanvas.PlayAgain += RestartGame;
    }

    private void DisconnectEvents()
    {
        _player.HealthChanged -= HandlePlayerHealthChanged;
        _player.InventoryChanged -= PlayerInventoryChanged;
        _victim.DestinationReached -= VictimReachedWanderingDestination;
        _gameOverCanvas.Exit -= ExitGame;
        _gameOverCanvas.PlayAgain -= RestartGame;
    }
}

