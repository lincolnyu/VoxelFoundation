using VoxelFoundation.Blocks;
using VoxelFoundation.Core;

namespace VoxelFoundation.World;

public static class WorldGen
{
    public static void FlatInn(WorldGrid world)
    {
        var dirt = world.Registry.Get("dirt").Id;
        var grass = world.Registry.Get("grass").Id;
        var stone = world.Registry.Get("stone").Id;
        var planks = world.Registry.Get("planks").Id;
        var wood = world.Registry.Get("wood").Id;
        var door = world.Registry.Get("inn_door_locked").Id;
        var bed = world.Registry.Get("guest_bed").Id;
        var bell = world.Registry.Get("bell").Id;
        var glass = world.Registry.Get("glass").Id;

        const int ground = 8;
        for (var x = -16; x < 32; x++)
        for (var z = -16; z < 32; z++)
        {
            for (var y = 0; y < ground; y++)
                world.SetBlock(new Vector3i(x, y, z), dirt);
            world.SetBlock(new Vector3i(x, ground, z), grass);
        }

        // inn box 7x5x7 at origin
        int x0 = 2, z0 = 2, y0 = ground + 1;
        int x1 = 8, z1 = 8, y1 = ground + 5;
        for (var x = x0; x <= x1; x++)
        for (var z = z0; z <= z1; z++)
        for (var y = y0; y <= y1; y++)
        {
            var wall = x == x0 || x == x1 || z == z0 || z == z1 || y == y1;
            if (wall)
                world.SetBlock(new Vector3i(x, y, z), y == y1 ? wood : planks);
        }

        // doorway
        world.SetBlock(new Vector3i(5, y0, z0), BlockId.Air);
        world.SetBlock(new Vector3i(5, y0 + 1, z0), door);

        // windows
        world.SetBlock(new Vector3i(x0, y0 + 2, 5), glass);
        world.SetBlock(new Vector3i(x1, y0 + 2, 5), glass);

        world.SetBlock(new Vector3i(7, y0, 7), bed);
        world.SetBlock(new Vector3i(3, y0, 3), bell);
        world.SetBlock(new Vector3i(5, ground, 0), stone);
    }
}