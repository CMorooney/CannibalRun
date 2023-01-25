using Godot;
using System;
using static Utils;

public class Start : Node
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Button _startButton;
    private Button _exitButton;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void _Ready()
    {
        _startButton = GetOrThrow<Button>(this, "ControlContainer/StartButton");
        _exitButton = GetOrThrow<Button>(this, "ControlContainer/ExitButton");


        _startButton.Connect("pressed", this, nameof(StartButtonPressed));
        _exitButton.Connect("pressed", this, nameof(ExitButtonPressed));
    }

    private void StartButtonPressed() => GetTree().ChangeScene("res://Scenes/Game.tscn");

    private void ExitButtonPressed() => GetTree().Quit();
}
