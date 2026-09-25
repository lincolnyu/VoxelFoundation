# VoxelFoundation

First-person voxel kit for MonoGame 3.8.5 (DesktopGL). Built so you can add blocks later without rewriting the world.

## Run

```
dotnet restore
dotnet run
```

Needs .NET 8+ and the `MonoGame.Framework.DesktopGL` package (restored from NuGet). No content pipeline.

## Controls

- Mouse look (Esc frees the cursor)
- WASD, Shift sprint, Space jump
- LMB break, RMB place, E use
- 1–9 / scroll hotbar

Demo map: grass field + a small inn. Ring the **bell**, then **E** on the locked door. **E** on the bed.

## Layout

| Path | Role |
|---|---|
| `src/Blocks` | `BlockId`, `BlockDef`, registry, JSON loader, `IBlockPack` plugins |
| `src/World` | 16³ chunks, get/set, inn generator |
| `src/Render` | runtime color atlas + greedy-enough face mesher |
| `src/Player` | FPS body, DDA raycast, hotbar |
| `src/Story` | `FlagBoard` + HUD log (guest/door flags live here) |
| `Data/blocks/*.json` | extra dumb blocks (no C#) |
| `Data/palette.txt` | what kids see on the bar |
| `Data/Plugins/*.dll` | optional `IBlockPack` assemblies |

## Add a block

**Dumb (JSON):** drop `Data/blocks/foo.json` and a line in `palette.txt`. Atlas color is hashed from the texture name.

**Smart (C#):** in `BuiltinPack` or a new `IBlockPack`:

```csharp
registry.RegisterNew("trap_hatch", b =>
{
    b.DisplayName = "Trap Hatch";
    b.TexAll("trap");
    b.Behavior = new TrapHatchBehavior();
});
```

Implement `IBlockBehavior` (`OnUse` / `OnPlace` / `OnBreak` / `Tick`). Talk to the world only through `WorldGrid` + `GameServices` (flags, log).

**Later DLL:** implement `IBlockPack`, build a class library that references this project, copy the dll to `Data/Plugins`.

## Not in v1 (safe to add)

- Save/load (store block **names**, not raw ids)
- Lighting flood fill
- Entities overlay (`Dictionary<Vector3i, T>`) for walking guests
- SpriteFont HUD
- PNG atlas instead of generated colors
- Greedy meshing
- Multiplayer

Do not put objects in the voxel grid. Ids only; extra state goes on `FlagBoard` or a side dictionary.
