namespace VoxelFoundation.Blocks;

public sealed class BlockRegistry
{
    readonly Dictionary<BlockId, BlockDef> _byId = new();
    readonly Dictionary<string, BlockDef> _byName = new(StringComparer.OrdinalIgnoreCase);
    ushort _next = 1;

    public BlockRegistry()
    {
        Register(new BlockDef
        {
            Id = BlockId.Air,
            Name = "air",
            DisplayName = "Air",
            Solid = false,
            Opaque = false,
            Textures = new Dictionary<string, string>()
        });
    }

    public IEnumerable<BlockDef> All => _byId.Values;
    public int Count => _byId.Count;

    public BlockDef Get(BlockId id) =>
        _byId.TryGetValue(id, out var d) ? d : _byId[BlockId.Air];

    public BlockDef Get(string name) =>
        _byName.TryGetValue(name, out var d) ? d : _byId[BlockId.Air];

    public bool TryGet(string name, out BlockDef def) => _byName.TryGetValue(name, out def!);

    public BlockId Register(BlockDef def)
    {
        if (def.Id.Value == 0 && def.Name != "air")
            throw new InvalidOperationException("Only air may use id 0.");
        _byId[def.Id] = def;
        _byName[def.Name] = def;
        if (def.Id.Value >= _next)
            _next = (ushort)(def.Id.Value + 1);
        return def.Id;
    }

    public BlockId AllocateId() => new(_next++);

    public BlockId RegisterNew(string name, Action<BlockDefBuilder> configure)
    {
        var b = new BlockDefBuilder(this, name, AllocateId());
        configure(b);
        return Register(b.Build());
    }
}

public sealed class BlockDefBuilder
{
    readonly BlockRegistry _reg;
    readonly string _name;
    readonly BlockId _id;
    readonly Dictionary<string, string> _tex = new(StringComparer.OrdinalIgnoreCase);

    public BlockDefBuilder(BlockRegistry reg, string name, BlockId id)
    {
        _reg = reg;
        _name = name;
        _id = id;
        DisplayName = name;
    }

    public string DisplayName { get; set; }
    public bool Solid { get; set; } = true;
    public bool Opaque { get; set; } = true;
    public byte LightEmission { get; set; }
    public IBlockBehavior Behavior { get; set; } = NullBehavior.Instance;

    public BlockDefBuilder Tex(string slot, string atlasName)
    {
        _tex[slot] = atlasName;
        return this;
    }

    public BlockDefBuilder TexAll(string atlasName)
    {
        _tex["all"] = atlasName;
        return this;
    }

    public BlockDef Build()
    {
        if (_tex.Count == 0)
            _tex["all"] = _name;
        return new BlockDef
        {
            Id = _id,
            Name = _name,
            DisplayName = DisplayName,
            Solid = Solid,
            Opaque = Opaque,
            LightEmission = LightEmission,
            Textures = _tex,
            Behavior = Behavior
        };
    }
}
