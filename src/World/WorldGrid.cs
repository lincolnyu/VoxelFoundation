using VoxelFoundation.Blocks;
using VoxelFoundation.Core;

namespace VoxelFoundation.World;

public sealed class WorldGrid
{
    readonly Dictionary<(int, int, int), Chunk> _chunks = new();
    public BlockRegistry Registry { get; }
    public event Action<Vector3i>? BlockChanged;

    public WorldGrid(BlockRegistry registry) => Registry = registry;

    public IEnumerable<Chunk> Chunks => _chunks.Values;

    public Chunk GetOrCreateChunk(Vector3i coord)
    {
        var key = (coord.X, coord.Y, coord.Z);
        if (!_chunks.TryGetValue(key, out var c))
        {
            c = new Chunk(coord);
            _chunks[key] = c;
        }
        return c;
    }

    public bool TryGetChunk(Vector3i coord, out Chunk chunk) =>
        _chunks.TryGetValue((coord.X, coord.Y, coord.Z), out chunk!);

    public static Vector3i ChunkCoord(Vector3i world)
    {
        static int Div(int v) => v >= 0 ? v / Chunk.Size : (v - (Chunk.Size - 1)) / Chunk.Size;
        return new Vector3i(Div(world.X), Div(world.Y), Div(world.Z));
    }

    public static Vector3i Local(Vector3i world)
    {
        static int Mod(int v)
        {
            int m = v % Chunk.Size;
            return m < 0 ? m + Chunk.Size : m;
        }
        return new Vector3i(Mod(world.X), Mod(world.Y), Mod(world.Z));
    }

    public BlockId GetBlock(Vector3i p)
    {
        var cc = ChunkCoord(p);
        if (!TryGetChunk(cc, out var chunk))
            return BlockId.Air;
        var l = Local(p);
        return chunk.Get(l.X, l.Y, l.Z);
    }

    public BlockDef GetDef(Vector3i p) => Registry.Get(GetBlock(p));

    public bool SetBlock(Vector3i p, BlockId id)
    {
        var cc = ChunkCoord(p);
        var chunk = GetOrCreateChunk(cc);
        var l = Local(p);
        if (!chunk.Set(l.X, l.Y, l.Z, id))
            return false;
        MarkNeighborDirty(p, l);
        BlockChanged?.Invoke(p);
        return true;
    }

    void MarkNeighborDirty(Vector3i world, Vector3i local)
    {
        void Touch(int dx, int dy, int dz)
        {
            if (TryGetChunk(new Vector3i(ChunkCoord(world).X + dx, ChunkCoord(world).Y + dy, ChunkCoord(world).Z + dz), out var c))
                c.Dirty = true;
        }
        if (local.X == 0) Touch(-1, 0, 0);
        if (local.X == Chunk.Size - 1) Touch(1, 0, 0);
        if (local.Y == 0) Touch(0, -1, 0);
        if (local.Y == Chunk.Size - 1) Touch(0, 1, 0);
        if (local.Z == 0) Touch(0, 0, -1);
        if (local.Z == Chunk.Size - 1) Touch(0, 0, 1);
    }

    public bool IsSolid(Vector3i p) => GetDef(p).Solid;
}
