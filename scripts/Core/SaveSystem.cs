using System;
using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>
    /// JSON savegames via Newtonsoft.Json (fast, human-readable).
    /// For a LiteDB NoSQL file use <see cref="LiteDbSaveStore"/> from GhettoBeasts.Core.
    /// </summary>
    public static class SaveSystem
    {
        public const string SavePath = "user://savegame.json";

        public static SaveData Load()
        {
            if (!FileAccess.FileExists(SavePath)) return SaveData.CreateDefault();

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file is null)
            {
                GD.PrintErr($"[SaveSystem] Kann {SavePath} nicht lesen: {FileAccess.GetOpenError()}");
                return SaveData.CreateDefault();
            }

            try
            {
                return SaveSerializer.Deserialize(file.GetAsText());
            }
            catch (InvalidOperationException ex)
            {
                GD.PrintErr($"[SaveSystem] {ex.Message} Starte mit neuem Spielstand.");
                return SaveData.CreateDefault();
            }
        }

        public static void Write(SaveData data)
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file is null)
            {
                GD.PrintErr($"[SaveSystem] Kann {SavePath} nicht schreiben: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(SaveSerializer.Serialize(data));
        }

        public static void Delete()
        {
            if (FileAccess.FileExists(SavePath))
                DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
        }
    }
}
