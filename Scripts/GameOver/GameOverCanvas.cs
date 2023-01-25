using Godot;
using System;
using static Utils;

public delegate void PlayAgain();
public delegate void Exit();

public class GameOverCanvas : CanvasLayer
{
    public event PlayAgain? PlayAgain;
    public event Exit? Exit;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Button _playAgainButton;
    private Button _exitButton;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void _Ready()
    {
        _playAgainButton = GetOrThrow<Button>(this, "ControlContainer/PlayAgain");
        _exitButton = GetOrThrow<Button>(this, "ControlContainer/Exit");

        _playAgainButton.Connect("pressed", this, nameof(PlayAgainPressed));
        _exitButton.Connect("pressed", this, nameof(ExitPressed));
    }

    private void PlayAgainPressed() => PlayAgain?.Invoke();
    private void ExitPressed() => Exit?.Invoke();
}
