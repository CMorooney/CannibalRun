using Godot;
using System;
using System.Collections.Generic;

using Array = Godot.Collections.Array;

public class ActionMenu<T> : VBoxContainer
{
    public delegate void ItemSelectedDelegate(T item);
    public event ItemSelectedDelegate? ItemSelected;

    public void Show(List<T> items)
    {
        foreach (var item in items)
        {
            if (item == null) continue;
            var button = new Button();
            button.Connect("pressed", this, "ButtonPressed", new Array(item));
            AddChild(button);
        }

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
