#if TOOLS

using Godot;
using System.Linq;
using System.Collections.Generic;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using static Picalines.Godot.LDtkImport.Json.WorldJson.TileSetDefinition;
using System;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class TileSetImporter
    {
        public static TileSet CreateNew(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson)
        {
            TileSet tileSet = new();

            ApplyChanges(ldtkFilePath, tileSetJson, tileSet);

            return tileSet;
        }

        public static void ApplyChanges(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson, TileSet tileSet)
        {
            var texture = GD.Load<Texture>(GetTexturePath(ldtkFilePath, tileSetJson));
            var textureImage = texture.GetData();

            int tileSize = tileSetJson.TileGridSize;
            int tileFullSize = tileSize + tileSetJson.Spacing;
            int gridWidth = ((tileSetJson.PxWidth - tileSetJson.Padding) / tileFullSize) + 1;
            int gridHeight = ((tileSetJson.PxHeight - tileSetJson.Padding) / tileFullSize) + 1;

            int gridSize = gridWidth * gridHeight;

            var blockTileIds = tileSetJson.EnumTags.First<TileEnumTag>(t => t.EnumValueId == "Block").TileIds;

            var usedTileIds = tileSet.GetTilesIds();

            int tileId = 0;
            int indexer = 0;
            while (tileId < gridSize)
            {
                var tileRegion = GetTileRegion(indexer, tileSetJson);

                if (!textureImage.GetRect(tileRegion).IsInvisible())
                {
                    if (usedTileIds.Contains(tileId))
                    {
                        tileSet.RemoveTile(tileId);
                    }
                
                    tileSet.CreateTile(tileId);
                    tileSet.TileSetTileMode(tileId, TileSet.TileMode.SingleTile);
                    tileSet.TileSetTexture(tileId, texture);
                    tileSet.TileSetRegion(tileId, tileRegion);

                    if (blockTileIds.Contains(tileId))
                    {
                        var x = new List<int[]> { new[] { 0, 0 }, new [] { tileSize, 0 }, new [] { tileSize, tileSize } , new [] { 0, tileSize } };
                        var colliderShape = CreateCollisionShapeFromPoints(x);
                        tileSet.TileAddShape(tileId, colliderShape, new Transform2D(), false);
                    }

                    tileId++;
                }

                indexer++;
            }
        }

        private static ConvexPolygonShape2D CreateCollisionShapeFromPoints(List<int[]> points)
        {
            var shape = new ConvexPolygonShape2D();
            var ps = new List<Vector2>();
            foreach (var point in points)
            {
                ps.Add(new Vector2(point[0], point[1]));
                shape.Points = ps.ToArray();
            }

            return shape;
        }

        private static string GetTexturePath(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson)
        {
            return ldtkFilePath.GetBaseDir() + "/" + tileSetJson.TextureRelPath;
        }

        private static Rect2 GetTileRegion(int tileId, WorldJson.TileSetDefinition tileSetJson) => new()
        {
            Position = TileCoord.IdToPx(tileId, tileSetJson),
            Size = tileSetJson.TileGridSizeV,
        };
    }
}

#endif