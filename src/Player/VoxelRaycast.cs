using Microsoft.Xna.Framework;
using VoxelFoundation.Core;
using VoxelFoundation.World;

namespace VoxelFoundation.Player;

public readonly record struct RayHit(Vector3i Block, Vector3i Before, Vector3i Normal, float Distance);

public static class VoxelRaycast
{
    public static bool Cast(WorldGrid world, Vector3 origin, Vector3 dir, float maxDist, out RayHit hit)
    {
        hit = default;
        dir.Normalize();
        int x = (int)MathF.Floor(origin.X);
        int y = (int)MathF.Floor(origin.Y);
        int z = (int)MathF.Floor(origin.Z);

        int stepX = dir.X >= 0 ? 1 : -1;
        int stepY = dir.Y >= 0 ? 1 : -1;
        int stepZ = dir.Z >= 0 ? 1 : -1;

        float tDeltaX = dir.X == 0 ? float.PositiveInfinity : MathF.Abs(1f / dir.X);
        float tDeltaY = dir.Y == 0 ? float.PositiveInfinity : MathF.Abs(1f / dir.Y);
        float tDeltaZ = dir.Z == 0 ? float.PositiveInfinity : MathF.Abs(1f / dir.Z);

        float nextX = dir.X >= 0 ? MathF.Floor(origin.X) + 1 : MathF.Floor(origin.X);
        float nextY = dir.Y >= 0 ? MathF.Floor(origin.Y) + 1 : MathF.Floor(origin.Y);
        float nextZ = dir.Z >= 0 ? MathF.Floor(origin.Z) + 1 : MathF.Floor(origin.Z);

        float tMaxX = dir.X == 0 ? float.PositiveInfinity : (nextX - origin.X) / dir.X;
        float tMaxY = dir.Y == 0 ? float.PositiveInfinity : (nextY - origin.Y) / dir.Y;
        float tMaxZ = dir.Z == 0 ? float.PositiveInfinity : (nextZ - origin.Z) / dir.Z;

        Vector3i last = new(x, y, z);
        float t = 0;
        for (int i = 0; i < 256 && t <= maxDist; i++)
        {
            var p = new Vector3i(x, y, z);
            if (!world.GetBlock(p).IsAir)
            {
                var n = new Vector3i(last.X - x, last.Y - y, last.Z - z);
                hit = new RayHit(p, last, n, t);
                return true;
            }
            last = p;
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                t = tMaxX;
                tMaxX += tDeltaX;
                x += stepX;
            }
            else if (tMaxY < tMaxZ)
            {
                t = tMaxY;
                tMaxY += tDeltaY;
                y += stepY;
            }
            else
            {
                t = tMaxZ;
                tMaxZ += tDeltaZ;
                z += stepZ;
            }
        }
        return false;
    }
}
