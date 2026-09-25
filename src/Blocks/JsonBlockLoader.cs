using System.Text.Json;

namespace VoxelFoundation.Blocks;

public static class JsonBlockLoader
{
    public static void LoadFolder(string folder, BlockRegistry registry)
    {
        if (!Directory.Exists(folder))
            return;
        foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
            LoadFile(file, registry);
    }

    public static void LoadFile(string path, BlockRegistry registry)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var r = doc.RootElement;
        var name = r.GetProperty("name").GetString() ?? Path.GetFileNameWithoutExtension(path);
        if (registry.TryGet(name, out _))
            return; // C# pack wins over JSON of the same name

        var tex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (r.TryGetProperty("textures", out var t))
        {
            foreach (var p in t.EnumerateObject())
                tex[p.Name] = p.Value.GetString() ?? name;
        }
        if (tex.Count == 0)
            tex["all"] = name;

        registry.Register(new BlockDef
        {
            Id = registry.AllocateId(),
            Name = name,
            DisplayName = r.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? name : name,
            Solid = !r.TryGetProperty("solid", out var s) || s.GetBoolean(),
            Opaque = !r.TryGetProperty("opaque", out var o) || o.GetBoolean(),
            LightEmission = r.TryGetProperty("light", out var l) ? l.GetByte() : (byte)0,
            Textures = tex
        });
    }
}
