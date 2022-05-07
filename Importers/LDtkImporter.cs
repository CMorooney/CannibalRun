﻿#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using System;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public static class LDtkImporter
    {
        public static void Import(string ldtkFilePath, string settingsFilePath, out string outputDirectory)
        {
            var worldJson = JsonFile.Parse<WorldJson>(ldtkFilePath);
            var settings = JsonFile.Parse<LDtkImportSettings>(settingsFilePath);

            using var outputDir = new Directory();
            outputDir.MakeDirRecursive(settings.OutputDirectory);

            ImportTileSets(ldtkFilePath, settings.OutputDirectory, worldJson);

            ImportLevels(ldtkFilePath, settings, worldJson);

            // TODO: world scene

            outputDirectory = settings.OutputDirectory;
        }

        private static void ImportLevels(string ldtkFilePath, LDtkImportSettings importSettings, WorldJson worldJson)
        {
            Func<LevelJson, LevelJson> getLevelJson = worldJson.ExternalLevels
                ? levelInfo => JsonFile.Parse<LevelJson>($"{ldtkFilePath.GetBaseDir()}/{levelInfo.ExternalRelPath}")
                : fullLevel => fullLevel;

            foreach (var levelInfo in worldJson.Levels)
            {
                var levelJson = getLevelJson(levelInfo);

                LevelImporter.Import(new LevelImportContext(ldtkFilePath, importSettings, worldJson, levelJson));
            }
        }

        private static void ImportTileSets(string ldtkFilePath, string outputDir, WorldJson worldJson)
        {
            var tileSetsDir = outputDir + "/tilesets";

            using var dir = new Directory();
            dir.MakeDir(tileSetsDir);

            foreach (var tileSetJson in worldJson.Definitions.TileSets)
            {
                var savePath = $"{tileSetsDir}/{tileSetJson.Identifier}.tres";

                TileSet tileSet;

                using var file = new File();

                if (file.FileExists(savePath))
                {
                    tileSet = GD.Load<TileSet>(savePath);
                    TileSetImporter.ApplyChanges(ldtkFilePath, tileSetJson, tileSet);
                }
                else
                {
                    tileSet = TileSetImporter.CreateNew(ldtkFilePath, tileSetJson);
                }

                if (ResourceSaver.Save(savePath, tileSet) is not Error.Ok)
                {
                    throw LDtkImportException.FailedToImportTileSet(ldtkFilePath, tileSetJson.Identifier);
                }
            }
        }
    }
}

#endif
