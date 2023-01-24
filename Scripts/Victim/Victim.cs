using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public class Victim : KinematicBody2D
{
    //TODO: state machine

    public Action<IBodyPart>? _callback;

    private List<IBodyPart> _bodyParts = BodyParts.All();

#pragma warning disable CS8618 // Non-nullable field
    private ActionMenu _actionMenu;
#pragma warning restore CS8618 // Non-nullable field

    public override void _Ready()
    {
        _actionMenu = GetOrThrow<ActionMenu>(this, nameof(ActionMenu));
        ConnectEvents();
    }

    public override void _ExitTree()
    {
        DisconnectEvents();
    }

    private void ConnectEvents()
    {
        _actionMenu.ItemSelected += MenuItemSelected;
    }

    private void DisconnectEvents()
    { 
        _actionMenu.ItemSelected -= MenuItemSelected;
    }

    private void MenuItemSelected(string item)
    {
        var bodyPart = _bodyParts.FirstOrDefault(b => b.Name == item);

        if (_callback != null)
        {
            _callback(bodyPart);
        }

        _bodyParts.Remove(bodyPart);
        _callback = null;
    }

    public void ShowMenu(Action<IBodyPart> callback)
    {
        _actionMenu.Show(_bodyParts.Select(b => b.Name).ToHashSet<string>());
        _callback = callback;
    }
}
