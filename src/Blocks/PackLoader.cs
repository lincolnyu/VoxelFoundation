using System.Reflection;
using VoxelFoundation.Story;

namespace VoxelFoundation.Blocks;

public static class PackLoader
{
    public static void LoadDirectory(string dir, BlockRegistry registry, GameServices services)
    {
        if (!Directory.Exists(dir))
            return;
        foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var type in asm.GetExportedTypes())
                {
                    if (type.IsAbstract || !typeof(IBlockPack).IsAssignableFrom(type))
                        continue;
                    if (Activator.CreateInstance(type) is IBlockPack pack)
                        pack.Register(registry, services);
                }
            }
            catch (Exception ex)
            {
                services.Log.Say($"Plugin failed: {Path.GetFileName(dll)} ({ex.GetType().Name})");
            }
        }
    }
}
