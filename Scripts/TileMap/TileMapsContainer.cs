using Godot;
using System;
using System.Linq;
using static Utils;

public class TileMapsContainer : Node2D
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private TileMap _sidewalks;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private readonly Random _random = new Random();

    public override void _Ready()
    {
        _sidewalks = GetOrThrow<TileMap>(this, "Sidewalk");
    }

    public Vector2? GetRandomSidewalkTile()
    {
        var sidewalkCellLocations = _sidewalks.GetUsedCells().Cast<Vector2>().ToArray();

        if(sidewalkCellLocations != null)
        {
            var tilePosition = sidewalkCellLocations[_random.Next(0, sidewalkCellLocations.Length - 1)];
            return _sidewalks.ToGlobal(_sidewalks.MapToWorld(tilePosition));
        }

        return null;
    }
}
