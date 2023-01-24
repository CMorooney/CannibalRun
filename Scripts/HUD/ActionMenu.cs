using Godot;
using System;
using System.Collections.Generic;

using Array = Godot.Collections.Array;

public class ActionMenu : VBoxContainer
{
    public delegate void ItemSelectedDelegate(string item);
    public event ItemSelectedDelegate? ItemSelected;

    public void Show(ISet<string> items)
    {
        Button? previousButton = null;

        foreach (var item in items)
        {
            if (item == null) continue;
            var button = new Button();
            button.Text = item;
            AddChild(button);
            button.Owner = this;

            button.Connect("pressed", this, nameof(ButtonPressed), new Array() { item });

            button.FocusMode = FocusModeEnum.All;
            var buttonPath = GetPathTo(button);
            if(previousButton == null)
            {
                FocusNeighbourBottom = buttonPath;
            }

            previousButton = button;
        }

        FocusMode = FocusModeEnum.All;
        GrabFocus();
        Visible = true;
    }

    private void ButtonPressed(string item)
    {
        ItemSelected?.Invoke(item);

        Visible = false;

        foreach(Node child in GetChildren())
        {
            RemoveChild(child);
	    }
	}
}
