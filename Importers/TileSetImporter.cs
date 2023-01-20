#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

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

            int tileFullSize = tileSetJson.TileGridSize + tileSetJson.Spacing;
            int gridWidth = ((tileSetJson.PxWidth - tileSetJson.Padding) / tileFullSize) + 1;
            int gridHeight = ((tileSetJson.PxHeight - tileSetJson.Padding) / tileFullSize) + 1;

            int gridSize = gridWidth * gridHeight;

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
                    tileId++;
                }

                indexer++;
            }
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