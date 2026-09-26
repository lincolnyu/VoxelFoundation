using VoxelFoundation.Core;
using VoxelFoundation.Player;
using VoxelFoundation.Story;
using VoxelFoundation.World;

namespace VoxelFoundation.Blocks;

public sealed class BuiltinPack : IBlockPack
{
    public string PackName => "builtin";

    public void Register(BlockRegistry registry, GameServices services)
    {
        registry.RegisterNew("dirt", b =>
        {
            b.DisplayName = "Dirt";
            b.TexAll("dirt");
        });
        registry.RegisterNew("grass", b =>
        {
            b.DisplayName = "Grass";
            b.Tex("top", "grass_top");
            b.Tex("bottom", "dirt");
            b.Tex("side", "grass_side");
        });
        registry.RegisterNew("stone", b =>
        {
            b.DisplayName = "Stone";
            b.TexAll("stone");
        });
        registry.RegisterNew("wood", b =>
        {
            b.DisplayName = "Wood";
            b.TexAll("wood");
        });
        registry.RegisterNew("planks", b =>
        {
            b.DisplayName = "Planks";
            b.TexAll("planks");
        });
        registry.RegisterNew("glass", b =>
        {
            b.DisplayName = "Glass";
            b.Opaque = false;
            b.TexAll("glass");
        });
        registry.RegisterNew("inn_door_locked", b =>
        {
            b.DisplayName = "Locked Door";
            b.TexAll("door_locked");
            b.Behavior = new LockedDoorBehavior();
        });
        registry.RegisterNew("inn_door_open", b =>
        {
            b.DisplayName = "Open Door";
            b.Solid = false;
            b.Opaque = false;
            b.TexAll("door_open");
            b.Behavior = new OpenDoorBehavior();
        });
        registry.RegisterNew("guest_bed", b =>
        {
            b.DisplayName = "Guest Bed";
            b.TexAll("bed");
            b.Behavior = new GuestBedBehavior();
        });
        registry.RegisterNew("bell", b =>
        {
            b.DisplayName = "Bell";
            b.Solid = true;
            b.Opaque = false;
            b.TexAll("bell");
            b.Behavior = new BellBehavior();
        });
    }
}

public sealed class LockedDoorBehavior : IBlockBehavior
{
    public void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void Tick(WorldGrid world, Vector3i pos, GameServices s)
    {
    }

    public void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s)
    {
        if (s.Flags["has_key"] || s.Flags["door_unlocked"])
        {
            world.SetBlock(pos, world.Registry.Get("inn_door_open").Id);
            s.Log.Say("The door creaks open.");
        }
        else
        {
            s.Log.Say("It's locked. Ring the bell?");
        }
    }
}

public sealed class OpenDoorBehavior : IBlockBehavior
{
    public void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void Tick(WorldGrid world, Vector3i pos, GameServices s)
    {
    }

    public void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s)
    {
        world.SetBlock(pos, world.Registry.Get("inn_door_locked").Id);
        s.Log.Say("You shut the door.");
    }
}

public sealed class GuestBedBehavior : IBlockBehavior
{
    public void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void Tick(WorldGrid world, Vector3i pos, GameServices s)
    {
    }

    public void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s)
    {
        s.Flags["guest_noticed"] = true;
        s.Log.Say("A guest is trapped here. They look at you.");
    }
}

public sealed class BellBehavior : IBlockBehavior
{
    public void OnPlace(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void OnBreak(WorldGrid world, Vector3i pos, BlockId id, GameServices s)
    {
    }

    public void Tick(WorldGrid world, Vector3i pos, GameServices s)
    {
    }

    public void OnUse(WorldGrid world, Vector3i pos, PlayerController player, GameServices s)
    {
        s.Flags["door_unlocked"] = true;
        s.Log.Say("The bell rings. Somewhere a latch lifts.");
    }
}