using Godot;
using System;
using System.Collections.Generic;

using Array = Godot.Collections.Array;

public class ActionMenu<T> : VBoxContainer where T : INameable
{
    public delegate void ItemSelectedDelegate(T item);
    public event ItemSelectedDelegate? ItemSelected;

    public void Show(ISet<T> items)
    {
        Button? previousButton = null;

        foreach (var item in items)
        {
            if (item == null) continue;
            var button = new Button();
            button.Text = item.Name;
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

    private void ButtonPressed(T item)
    {
        ItemSelected?.Invoke(item);

        Visible = false;

        foreach(Node child in GetChildren())
        {
            RemoveChild(child);
	    }
	}
}
