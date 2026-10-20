using System;
using System.IO;
using LiteDB;

namespace GhettoBeasts.Core
{
    /// <summary>
    /// Optional LiteDB-backed save store for inventory-heavy later steps.
    /// The Godot <c>SaveSystem</c> currently uses JSON; call this when you need a single-file NoSQL store.
    /// </summary>
    public sealed class LiteDbSaveStore : IDisposable
    {
        private readonly LiteDatabase database;
        private readonly ILiteCollection<SaveData> collection;

        public LiteDbSaveStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Pfad fehlt.", nameof(filePath));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");
            database = new LiteDatabase(filePath);
            collection = database.GetCollection<SaveData>("saves");
            collection.EnsureIndex(x => x.Version);
        }

        public SaveData LoadOrCreate(int id = 1)
        {
            var existing = collection.FindById(id);
            if (existing is not null) return existing;
            var created = new SaveData();
            collection.Upsert(id, created);
            return created;
        }

        public void Write(SaveData data, int id = 1) => collection.Upsert(id, data);

        public void Dispose() => database.Dispose();
    }
}
