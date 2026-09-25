using VoxelFoundation.Core;
using VoxelFoundation.Player;
using VoxelFoundation.Story;
using VoxelFoundation.World;

namespace VoxelFoundation.Blocks;

/// <summary>Per-type behaviour. No per-cell objects — keep the grid as ids only.</summary>
public interface IBlockBehavior
{
    void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s);
    void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s);
    void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s);
    void Tick(WorldGrid world, Vector3i pos, GameServices s);
}

public sealed class NullBehavior : IBlockBehavior
{
    public static readonly NullBehavior Instance = new();
    public void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s) { }
    public void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s) { }
    public void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s) { }
    public void Tick(WorldGrid world, Vector3i pos, GameServices s) { }
}
