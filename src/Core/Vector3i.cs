namespace VoxelFoundation.Core;

public readonly record struct Vector3i(int X, int Y, int Z)
{
    public static readonly Vector3i Zero = new(0, 0, 0);

    public static Vector3i operator +(Vector3i a, Vector3i b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3i operator -(Vector3i a, Vector3i b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3i operator *(Vector3i a, int s) => new(a.X * s, a.Y * s, a.Z * s);

    public override string ToString() => $"({X},{Y},{Z})";
}
