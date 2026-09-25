using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VoxelFoundation.Core;
using VoxelFoundation.World;

namespace VoxelFoundation.Player;

public sealed class PlayerController
{
    public Vector3 Position = new(5.5f, 12f, -2f);
    public float Yaw;
    public float Pitch = -0.15f;
    public float EyeHeight = 1.62f;
    public Vector3 Size = new(0.6f, 1.8f, 0.6f);
    public Vector3 Velocity;
    public bool Grounded;

    public Vector3 Eye => Position + new Vector3(0, EyeHeight, 0);
    public Vector3 Forward
    {
        get
        {
            float cy = MathF.Cos(Yaw), sy = MathF.Sin(Yaw);
            float cp = MathF.Cos(Pitch), sp = MathF.Sin(Pitch);
            return Vector3.Normalize(new Vector3(sy * cp, -sp, cy * cp));
        }
    }

    public Matrix View => Matrix.CreateLookAt(Eye, Eye + Forward, Vector3.Up);

    public void UpdateLook(float dx, float dy)
    {
        // +Yaw turns toward +X, which is screen-left in XNA's right-handed space.
        Yaw -= dx;
        Pitch = MathHelper.Clamp(Pitch + dy, -1.54f, 1.54f);
    }

    public void UpdateMove(GameTime time, WorldGrid world, KeyboardState kb)
    {
        float dt = (float)time.ElapsedGameTime.TotalSeconds;
        var f = Forward; f.Y = 0;
        if (f.LengthSquared() > 0.0001f) f.Normalize();
        var r = Vector3.Cross(f, Vector3.Up);
        if (r.LengthSquared() > 0.0001f) r.Normalize();

        var wish = Vector3.Zero;
        if (kb.IsKeyDown(Keys.W)) wish += f;
        if (kb.IsKeyDown(Keys.S)) wish -= f;
        if (kb.IsKeyDown(Keys.A)) wish -= r;
        if (kb.IsKeyDown(Keys.D)) wish += r;
        if (wish.LengthSquared() > 1f) wish.Normalize();

        float speed = kb.IsKeyDown(Keys.LeftShift) ? 8f : 5f;
        Velocity.X = wish.X * speed;
        Velocity.Z = wish.Z * speed;
        Velocity.Y -= 20f * dt;
        if (Grounded && kb.IsKeyDown(Keys.Space))
            Velocity.Y = 7.5f;

        MoveAxis(world, Velocity.X * dt, 0);
        Grounded = false;
        MoveAxis(world, Velocity.Y * dt, 1);
        MoveAxis(world, Velocity.Z * dt, 2);
    }

    void MoveAxis(WorldGrid world, float delta, int axis)
    {
        if (delta == 0) return;
        var p = Position;
        if (axis == 0) p.X += delta;
        else if (axis == 1) p.Y += delta;
        else p.Z += delta;

        if (!Overlaps(world, p))
        {
            Position = p;
            return;
        }

        if (axis == 1)
        {
            if (delta < 0)
            {
                // Snap feet onto the block top instead of stopping short of it.
                var snapped = Position;
                snapped.Y = MathF.Floor(p.Y) + 1f;
                if (snapped.Y <= Position.Y && !Overlaps(world, snapped))
                    Position = snapped;
                Grounded = true;
            }
            Velocity.Y = 0;
        }
        else if (axis == 0) Velocity.X = 0;
        else Velocity.Z = 0;
    }

    bool Overlaps(WorldGrid world, Vector3 pos)
    {
        float hx = Size.X * 0.5f, hz = Size.Z * 0.5f;
        int minX = (int)MathF.Floor(pos.X - hx);
        int maxX = (int)MathF.Floor(pos.X + hx);
        int minY = (int)MathF.Floor(pos.Y);
        int maxY = (int)MathF.Floor(pos.Y + Size.Y - 0.01f);
        int minZ = (int)MathF.Floor(pos.Z - hz);
        int maxZ = (int)MathF.Floor(pos.Z + hz);
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
        for (int x = minX; x <= maxX; x++)
        {
            if (world.IsSolid(new Vector3i(x, y, z)))
                return true;
        }
        return false;
    }
}
