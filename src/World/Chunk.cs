using VoxelFoundation.Blocks;
using VoxelFoundation.Core;

namespace VoxelFoundation.World;

public sealed class Chunk
{
    public const int Size = 16;

    public Vector3i Coord { get; }
    public bool Dirty { get; set; } = true;
    readonly BlockId[] _blocks = new BlockId[Size * Size * Size];

    public Chunk(Vector3i coord) => Coord = coord;

    public static int Index(int x, int y, int z) => x + Size * (z + Size * y);

    public BlockId Get(int x, int y, int z) => _blocks[Index(x, y, z)];

    public bool Set(int x, int y, int z, BlockId id)
    {
        var i = Index(x, y, z);
        if (_blocks[i] == id)
            return false;
        _blocks[i] = id;
        Dirty = true;
        return true;
    }

    public Vector3i Origin => new(Coord.X * Size, Coord.Y * Size, Coord.Z * Size);
}
