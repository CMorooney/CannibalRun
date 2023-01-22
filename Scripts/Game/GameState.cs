public interface IGameState : IMachineState<IGameState> { }

public record Playing : IGameState
{
    public bool CanTransitionTo(IGameState state) => !(state is Playing);
}

public record GameOver : IGameState
{
    public bool CanTransitionTo(IGameState state) => state is Playing;
}