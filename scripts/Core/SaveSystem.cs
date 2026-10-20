using System;
using Godot;

namespace GhettoBeasts.Core
{
    public static class SaveSystem
    {
        public const string SavePath = "user://savegame.json";

        public static SaveData Load()
        {
            if (!FileAccess.FileExists(SavePath)) return new SaveData();

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file is null)
            {
                GD.PrintErr($"[SaveSystem] Kann {SavePath} nicht lesen: {FileAccess.GetOpenError()}");
                return new SaveData();
            }

            try
            {
                return SaveSerializer.Deserialize(file.GetAsText());
            }
            catch (InvalidOperationException ex)
            {
                GD.PrintErr($"[SaveSystem] {ex.Message} Starte mit neuem Spielstand.");
                return new SaveData();
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
