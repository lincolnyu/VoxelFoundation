using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelFoundation.Render;

public sealed class TextureAtlas : IDisposable
{
    public const int TilePx = 16;
    public Texture2D Texture { get; }
    public int TilesPerRow { get; }
    readonly Dictionary<string, int> _index = new(StringComparer.OrdinalIgnoreCase);

    public TextureAtlas(GraphicsDevice gd, IEnumerable<string> names)
    {
        var list = names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
        if (list.Count == 0)
            list.Add("missing");
        TilesPerRow = (int)Math.Ceiling(Math.Sqrt(list.Count));
        int dim = TilesPerRow * TilePx;
        Texture = new Texture2D(gd, dim, dim);
        var data = new Color[dim * dim];

        for (int i = 0; i < list.Count; i++)
        {
            _index[list[i]] = i;
            PaintTile(data, dim, i, ColorFor(list[i]), list[i]);
        }
        Texture.SetData(data);
    }

    public bool TryUv(string name, out Vector2 uv0, out Vector2 uv1)
    {
        if (!_index.TryGetValue(name, out var i))
        {
            uv0 = uv1 = Vector2.Zero;
            return false;
        }
        int tx = i % TilesPerRow;
        int ty = i / TilesPerRow;
        float s = 1f / TilesPerRow;
        // inset to hide bleeding
        float pad = 0.5f / Texture.Width;
        uv0 = new Vector2(tx * s + pad, ty * s + pad);
        uv1 = new Vector2((tx + 1) * s - pad, (ty + 1) * s - pad);
        return true;
    }

    static void PaintTile(Color[] data, int dim, int index, Color baseCol, string name)
    {
        int tiles = dim / TilePx;
        int tx = (index % tiles) * TilePx;
        int ty = (index / tiles) * TilePx;
        for (int y = 0; y < TilePx; y++)
        for (int x = 0; x < TilePx; x++)
        {
            bool border = x == 0 || y == 0 || x == TilePx - 1 || y == TilePx - 1;
            bool hatch = ((x + y) & 4) == 0;
            var c = border ? Darken(baseCol, 0.55f) : hatch ? baseCol : Darken(baseCol, 0.85f);
            data[(ty + y) * dim + (tx + x)] = c;
        }
    }

    static Color Darken(Color c, float m) => new(
        (byte)(c.R * m), (byte)(c.G * m), (byte)(c.B * m), (byte)255);

    static Color ColorFor(string name) => name.ToLowerInvariant() switch
    {
        "dirt" => new Color(120, 80, 45),
        "grass_top" => new Color(70, 160, 55),
        "grass_side" => new Color(90, 130, 50),
        "stone" => new Color(130, 130, 135),
        "wood" => new Color(90, 55, 25),
        "planks" => new Color(170, 130, 70),
        "glass" => new Color(180, 220, 230),
        "door_locked" => new Color(80, 40, 20),
        "door_open" => new Color(160, 100, 40),
        "bed" => new Color(160, 50, 50),
        "bell" => new Color(220, 190, 50),
        _ => HashColor(name)
    };

    static Color HashColor(string name)
    {
        unchecked
        {
            int h = name.GetHashCode();
            return new Color(
                (byte)(80 + (h & 127)),
                (byte)(80 + ((h >> 7) & 127)),
                (byte)(80 + ((h >> 14) & 127)));
        }
    }

    public void Dispose() => Texture.Dispose();
}
