namespace VoxelFoundation.Story;

/// <summary>Stable façade plugins/behaviours talk to. Grow this instead of reaching into GameApp.</summary>
public sealed class GameServices
{
    public FlagBoard Flags { get; } = new();
    public HudLog Log { get; } = new();
}

public sealed class FlagBoard
{
    readonly Dictionary<string, bool> _flags = new(StringComparer.OrdinalIgnoreCase);
    public bool this[string name]
    {
        get => _flags.TryGetValue(name, out var v) && v;
        set => _flags[name] = value;
    }
}

public sealed class HudLog
{
    public string Line { get; private set; } = "LMB break  RMB place  E use  1-9 hotbar  Esc mouse";
    DateTime _until;

    public void Say(string text, double seconds = 3)
    {
        Line = text;
        _until = DateTime.UtcNow.AddSeconds(seconds);
    }

    public string Current
    {
        get
        {
            if (_until != default && DateTime.UtcNow > _until)
                return "LMB break  RMB place  E use  1-9 hotbar";
            return Line;
        }
    }
}
