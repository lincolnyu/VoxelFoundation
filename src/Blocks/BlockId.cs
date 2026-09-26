namespace VoxelFoundation.Blocks;

public readonly record struct BlockId(ushort Value)
{
    public static readonly BlockId Air = new(0);
    public bool IsAir => Value == 0;

    public override string ToString()
    {
        return Value.ToString();
    }
}