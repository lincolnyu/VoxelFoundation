namespace VoxelFoundation.Blocks;

public sealed class BlockDef
{
    public required BlockId Id { get; init; }
    public required string Name { get; init; }
    public string DisplayName { get; init; } = "";
    public bool Solid { get; init; } = true;
    public bool Opaque { get; init; } = true;
    public byte LightEmission { get; init; }

    /// <summary>Atlas tile names: all / top / bottom / side / north / south / east / west.</summary>
    public required IReadOnlyDictionary<string, string> Textures { get; init; }

    public IBlockBehavior Behavior { get; init; } = NullBehavior.Instance;

    public string FaceTexture(int face)
    {
        // face: 0=-X 1=+X 2=-Y 3=+Y 4=-Z 5=+Z
        var key = face switch
        {
            2 => "bottom",
            3 => "top",
            _ => "side"
        };
        if (Textures.TryGetValue(face switch
            {
                0 => "west",
                1 => "east",
                2 => "bottom",
                3 => "top",
                4 => "north",
                5 => "south",
                _ => "all"
            }, out var named))
            return named;
        if (Textures.TryGetValue(key, out var side))
            return side;
        if (Textures.TryGetValue("all", out var all))
            return all;
        return Name;
    }
}