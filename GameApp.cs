using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VoxelFoundation.Blocks;
using VoxelFoundation.Core;
using VoxelFoundation.Player;
using VoxelFoundation.Render;
using VoxelFoundation.Story;
using VoxelFoundation.World;

namespace VoxelFoundation;

public sealed class GameApp : Game
{
    readonly GraphicsDeviceManager _gdm;
    SpriteBatch _sb = null!;
    SpriteFont? _font;
    Texture2D _white = null!;

    BlockRegistry _registry = null!;
    GameServices _services = null!;
    WorldGrid _world = null!;
    WorldRenderer _renderer = null!;
    PlayerController _player = null!;
    Hotbar _hotbar = null!;

    MouseState _prevMouse;
    KeyboardState _prevKb;
    bool _mouseLook = true;
    int _centerX, _centerY;

    public GameApp()
    {
        _gdm = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            SynchronizeWithVerticalRetrace = true
        };
        IsMouseVisible = false;
        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        Window.Title = "VoxelFoundation";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sb = new SpriteBatch(GraphicsDevice);
        _white = new Texture2D(GraphicsDevice, 1, 1);
        _white.SetData(new[] { Color.White });

        _services = new GameServices();
        _registry = new BlockRegistry();
        new BuiltinPack().Register(_registry, _services);

        var dataRoot = FindDataRoot();
        JsonBlockLoader.LoadFolder(Path.Combine(dataRoot, "blocks"), _registry);
        PackLoader.LoadDirectory(Path.Combine(dataRoot, "Plugins"), _registry, _services);

        var texNames = _registry.All
            .SelectMany(d => d.Textures.Values)
            .Concat(_registry.All.Select(d => d.Name));
        var atlas = new TextureAtlas(GraphicsDevice, texNames);

        _world = new WorldGrid(_registry);
        WorldGen.FlatInn(_world);
        _renderer = new WorldRenderer(GraphicsDevice, atlas);
        _renderer.Sync(_world);

        _player = new PlayerController();
        _hotbar = PaletteFile.Load(Path.Combine(dataRoot, "palette.txt"), _registry);
        _services.Log.Say("Inn night. Ring the bell, then open the door.");
    }

    static string FindDataRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(dir, "Data");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetFullPath(Path.Combine(dir, ".."));
        }
        return Path.Combine(AppContext.BaseDirectory, "Data");
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        var ms = Mouse.GetState();
        _centerX = GraphicsDevice.Viewport.Width / 2;
        _centerY = GraphicsDevice.Viewport.Height / 2;

        if (kb.IsKeyDown(Keys.Escape) && _prevKb.IsKeyUp(Keys.Escape))
        {
            _mouseLook = !_mouseLook;
            IsMouseVisible = !_mouseLook;
            if (_mouseLook)
                Mouse.SetPosition(_centerX, _centerY);
        }

        if (kb.IsKeyDown(Keys.F4) && kb.IsKeyDown(Keys.LeftAlt))
            Exit();

        if (_mouseLook && IsActive)
        {
            float sens = 0.0022f;
            _player.UpdateLook((ms.X - _centerX) * sens, (ms.Y - _centerY) * sens);
            Mouse.SetPosition(_centerX, _centerY);
        }

        _player.UpdateMove(gameTime, _world, kb);

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (kb.IsKeyDown(key) && _prevKb.IsKeyUp(key))
                _hotbar.SelectIndex(i);
        }
        int scroll = Math.Sign(ms.ScrollWheelValue - _prevMouse.ScrollWheelValue);
        if (scroll != 0)
            _hotbar.Scroll(-scroll);

        if (VoxelRaycast.Cast(_world, _player.Eye, _player.Forward, 6f, out var hit))
        {
            bool lmb = ms.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool rmb = ms.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released;
            bool use = kb.IsKeyDown(Keys.E) && _prevKb.IsKeyUp(Keys.E);

            if (lmb)
            {
                var id = _world.GetBlock(hit.Block);
                var def = _registry.Get(id);
                def.Behavior.OnBreak(_world, hit.Block, id, _services);
                _world.SetBlock(hit.Block, BlockId.Air);
            }
            else if (rmb)
            {
                var place = hit.Before;
                if (!_world.IsSolid(place) && !WouldEmbedPlayer(place))
                {
                    var id = _hotbar.Current;
                    if (!id.IsAir)
                    {
                        _world.SetBlock(place, id);
                        _registry.Get(id).Behavior.OnPlace(_world, place, id, _services);
                    }
                }
            }
            else if (use)
            {
                var id = _world.GetBlock(hit.Block);
                _registry.Get(id).Behavior.OnUse(_world, hit.Block, _player, _services);
            }
        }

        _renderer.Sync(_world);
        _prevMouse = Mouse.GetState();
        _prevKb = kb;
        base.Update(gameTime);
    }

    bool WouldEmbedPlayer(Vector3i cell)
    {
        var p = _player.Position;
        float hx = _player.Size.X * 0.5f, hz = _player.Size.Z * 0.5f;
        return cell.X >= MathF.Floor(p.X - hx) && cell.X <= MathF.Floor(p.X + hx)
            && cell.Y >= MathF.Floor(p.Y) && cell.Y <= MathF.Floor(p.Y + _player.Size.Y - 0.01f)
            && cell.Z >= MathF.Floor(p.Z - hz) && cell.Z <= MathF.Floor(p.Z + hz);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(160, 200, 230));
        var proj = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70),
            GraphicsDevice.Viewport.AspectRatio,
            0.08f, 256f);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        _renderer.Draw(_player.View, proj);

        _sb.Begin(samplerState: SamplerState.PointClamp);
        int cx = GraphicsDevice.Viewport.Width / 2;
        int cy = GraphicsDevice.Viewport.Height / 2;
        _sb.Draw(_white, new Rectangle(cx - 8, cy - 1, 16, 2), Color.White);
        _sb.Draw(_white, new Rectangle(cx - 1, cy - 8, 2, 16), Color.White);

        DrawHotbar();
        DrawString(_services.Log.Current, 16, 16);
        var look = VoxelRaycast.Cast(_world, _player.Eye, _player.Forward, 6f, out var h)
            ? _registry.Get(_world.GetBlock(h.Block)).DisplayName
            : "";
        if (look.Length > 0)
            DrawString(look, 16, 40);
        _sb.End();
        base.Draw(gameTime);
    }

    void DrawHotbar()
    {
        int w = 44, pad = 6;
        int total = _hotbar.Slots.Length * (w + pad);
        int x0 = (GraphicsDevice.Viewport.Width - total) / 2;
        int y = GraphicsDevice.Viewport.Height - w - 20;
        for (int i = 0; i < _hotbar.Slots.Length; i++)
        {
            int x = x0 + i * (w + pad);
            var col = i == _hotbar.Selected ? Color.White : new Color(30, 30, 30, 180);
            _sb.Draw(_white, new Rectangle(x, y, w, w), col * 0.35f);
            _sb.Draw(_white, new Rectangle(x, y, w, 2), col);
            _sb.Draw(_white, new Rectangle(x, y + w - 2, w, 2), col);
            var def = _registry.Get(_hotbar.Slots[i]);
            DrawString($"{i + 1}", x + 4, y + 4);
            DrawString(Short(def.DisplayName), x + 4, y + w - 18);
        }
    }

    static string Short(string s) => s.Length <= 6 ? s : s[..6];

    void DrawString(string text, int x, int y)
    {
        // No SpriteFont in a pipeline-free proto — draw as bars so it still runs.
        // Replace with a loaded font when you add MGCB.
        int px = x;
        foreach (var ch in text)
        {
            if (ch == ' ') { px += 8; continue; }
            _sb.Draw(_white, new Rectangle(px, y, 6, 10), new Color(20, 20, 20, 180));
            _sb.Draw(_white, new Rectangle(px, y, 5, 9), Color.White);
            px += 7;
        }
    }

    protected override void UnloadContent()
    {
        _renderer?.Dispose();
        _white?.Dispose();
        _sb?.Dispose();
        base.UnloadContent();
    }
}
