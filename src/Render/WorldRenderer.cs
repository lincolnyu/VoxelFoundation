using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VoxelFoundation.World;

namespace VoxelFoundation.Render;

public sealed class WorldRenderer : IDisposable
{
    readonly Dictionary<(int,int,int), ChunkMesh> _meshes = new();
    readonly GraphicsDevice _gd;
    public TextureAtlas Atlas { get; }
    public BasicEffect Effect { get; }

    public WorldRenderer(GraphicsDevice gd, TextureAtlas atlas)
    {
        _gd = gd;
        Atlas = atlas;
        Effect = new BasicEffect(gd)
        {
            TextureEnabled = true,
            Texture = atlas.Texture,
            LightingEnabled = true,
            PreferPerPixelLighting = false,
            VertexColorEnabled = false
        };
        Effect.EnableDefaultLighting();
        Effect.AmbientLightColor = new Vector3(0.35f);
        Effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0.4f, -1f, 0.3f));
    }

    public void Sync(WorldGrid world)
    {
        foreach (var chunk in world.Chunks)
        {
            var key = (chunk.Coord.X, chunk.Coord.Y, chunk.Coord.Z);
            if (!_meshes.TryGetValue(key, out var mesh))
            {
                mesh = new ChunkMesh();
                _meshes[key] = mesh;
                chunk.Dirty = true;
            }
            if (chunk.Dirty)
                mesh.Rebuild(_gd, world, chunk, Atlas);
        }
    }

    public void Draw(Matrix view, Matrix proj)
    {
        Effect.View = view;
        Effect.Projection = proj;
        Effect.World = Matrix.Identity;
        Effect.Texture = Atlas.Texture;
        _gd.SamplerStates[0] = SamplerState.PointClamp;
        _gd.RasterizerState = RasterizerState.CullCounterClockwise;
        _gd.DepthStencilState = DepthStencilState.Default;
        _gd.BlendState = BlendState.Opaque;
        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            foreach (var mesh in _meshes.Values)
                mesh.Draw(_gd);
        }
    }

    public void Dispose()
    {
        foreach (var m in _meshes.Values)
            m.Dispose();
        Effect.Dispose();
        Atlas.Dispose();
    }
}
