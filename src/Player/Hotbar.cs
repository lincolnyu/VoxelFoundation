using VoxelFoundation.Blocks;

namespace VoxelFoundation.Player;

public sealed class Hotbar
{
    public BlockId[] Slots { get; }
    public int Selected { get; private set; }

    public Hotbar(IEnumerable<BlockId> ids)
    {
        Slots = ids.Take(9).ToArray();
        if (Slots.Length == 0)
            Slots = new[] { BlockId.Air };
    }

    public BlockId Current => Slots[Math.Clamp(Selected, 0, Slots.Length - 1)];

    public void SelectIndex(int i)
    {
        if (i >= 0 && i < Slots.Length)
            Selected = i;
    }

    public void Scroll(int delta)
    {
        if (Slots.Length == 0) return;
        Selected = (Selected + delta) % Slots.Length;
        if (Selected < 0) Selected += Slots.Length;
    }
}

public static class PaletteFile
{
    public static Hotbar Load(string path, BlockRegistry registry)
    {
        var ids = new List<BlockId>();
        if (File.Exists(path))
        {
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;
                if (registry.TryGet(line, out var def) && !def.Id.IsAir)
                    ids.Add(def.Id);
            }
        }
        if (ids.Count == 0)
        {
            foreach (var d in registry.All)
            {
                if (!d.Id.IsAir && d.Solid)
                    ids.Add(d.Id);
                if (ids.Count >= 9) break;
            }
        }
        return new Hotbar(ids);
    }
}
