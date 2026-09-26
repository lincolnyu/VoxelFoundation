using VoxelFoundation.Story;

namespace VoxelFoundation.Blocks;

/// <summary>
///     Future plugin entry. A later assembly can implement this and be loaded by PackLoader.
/// </summary>
public interface IBlockPack
{
    string PackName { get; }
    void Register(BlockRegistry registry, GameServices services);
}