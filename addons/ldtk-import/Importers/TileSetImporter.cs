#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using Array = Godot.Collections.Array;
using static Picalines.Godot.LDtkImport.Json.WorldJson.TileSetDefinition;

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

            uint tileSize = (uint)(tileSetJson.TileGridSize);
            uint tileFullSize = (uint)(tileSize + tileSetJson.Spacing);
            uint gridWidth = (uint)((tileSetJson.PxWidth - tileSetJson.Padding) / tileFullSize) + 1;
            uint gridHeight = (uint)((tileSetJson.PxHeight - tileSetJson.Padding) / tileFullSize) + 1;

            uint gridSize = gridWidth * gridHeight;

            var blockTileIds = tileSetJson.EnumTags.First<TileEnumTag>(t => t.EnumValueId == "Block").TileIds;
            var traversableTilsIds = tileSetJson.EnumTags.First<TileEnumTag>(t => t.EnumValueId == "Walkable").TileIds;

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
                        var colliderShape = ShapeForTile(tileSize);
                        tileSet.TileSetShape(tileId, tileId, colliderShape);
                    }
                    else if(traversableTilsIds.Contains(tileId))
                    {
                        var navigationShape = NavShapeForTile(tileSize);
                        tileSet.TileSetNavigationPolygon(tileId, navigationShape);
                    }

                    tileId++;
                }

                indexer++;
            }
        }

        private static NavigationPolygon NavShapeForTile(uint size)
        {
            var polygon = new NavigationPolygon();
            polygon.AddOutline(new[] {
                                new Vector2(0, 0),
                                new Vector2(size, 0),
                                new Vector2(size, size),
                                new Vector2(0, size)
                            });

            polygon.MakePolygonsFromOutlines();

            return polygon;
        }

        private static ConvexPolygonShape2D ShapeForTile(uint size)
        {
            var shape = new ConvexPolygonShape2D();
            var points = new List<Vector2> {
                                new Vector2(0, 0),
                                new Vector2(size, 0),
                                new Vector2(size, size),
                                new Vector2(0, size)
                        };

            shape.Points = points.ToArray();
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
