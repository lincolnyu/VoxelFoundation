using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VoxelFoundation.Core;
using VoxelFoundation.World;

namespace VoxelFoundation.Render;

public sealed class ChunkMesh : IDisposable
{
    static readonly Vector3i[] Normals =
    {
        new(-1,0,0), new(1,0,0), new(0,-1,0), new(0,1,0), new(0,0,-1), new(0,0,1)
    };

    // 4 verts per face, local
    static readonly Vector3[,] FaceVerts =
    {
        { new(0,0,0), new(0,0,1), new(0,1,1), new(0,1,0) }, // -X
        { new(1,0,1), new(1,0,0), new(1,1,0), new(1,1,1) }, // +X
        { new(0,0,1), new(0,0,0), new(1,0,0), new(1,0,1) }, // -Y
        { new(0,1,0), new(0,1,1), new(1,1,1), new(1,1,0) }, // +Y
        { new(1,0,0), new(0,0,0), new(0,1,0), new(1,1,0) }, // -Z
        { new(0,0,1), new(1,0,1), new(1,1,1), new(0,1,1) }, // +Z
    };

    public VertexBuffer? Vertices { get; private set; }
    public IndexBuffer? Indices { get; private set; }
    public int IndexCount { get; private set; }

    public void Rebuild(GraphicsDevice gd, WorldGrid world, Chunk chunk, TextureAtlas atlas)
    {
        var verts = new List<VertexPositionNormalTexture>(2048);
        var inds = new List<int>(3072);
        var origin = chunk.Origin;

        for (int y = 0; y < Chunk.Size; y++)
        for (int z = 0; z < Chunk.Size; z++)
        for (int x = 0; x < Chunk.Size; x++)
        {
            var id = chunk.Get(x, y, z);
            if (id.IsAir) continue;
            var def = world.Registry.Get(id);
            var wp = new Vector3i(origin.X + x, origin.Y + y, origin.Z + z);

            for (int f = 0; f < 6; f++)
            {
                var n = Normals[f];
                var nb = world.GetDef(new Vector3i(wp.X + n.X, wp.Y + n.Y, wp.Z + n.Z));
                if (nb.Opaque) continue;

                var texName = def.FaceTexture(f);
                if (!atlas.TryUv(texName, out var uv0, out var uv1))
                    atlas.TryUv(def.Name, out uv0, out uv1);

                var uv = new[]
                {
                    new Vector2(uv0.X, uv1.Y),
                    new Vector2(uv1.X, uv1.Y),
                    new Vector2(uv1.X, uv0.Y),
                    new Vector2(uv0.X, uv0.Y)
                };

                int baseIndex = verts.Count;
                var normal = new Vector3(n.X, n.Y, n.Z);
                for (int i = 0; i < 4; i++)
                {
                    var lv = FaceVerts[f, i];
                    verts.Add(new VertexPositionNormalTexture(
                        new Vector3(wp.X + lv.X, wp.Y + lv.Y, wp.Z + lv.Z),
                        normal, uv[i]));
                }
                // FaceVerts are CCW seen from outside; XNA front faces are CW, so emit reversed.
                inds.Add(baseIndex); inds.Add(baseIndex + 2); inds.Add(baseIndex + 1);
                inds.Add(baseIndex); inds.Add(baseIndex + 3); inds.Add(baseIndex + 2);
            }
        }

        Vertices?.Dispose();
        Indices?.Dispose();
        Vertices = null;
        Indices = null;
        IndexCount = inds.Count;
        if (IndexCount == 0)
            return;

        Vertices = new VertexBuffer(gd, typeof(VertexPositionNormalTexture), verts.Count, BufferUsage.WriteOnly);
        Vertices.SetData(verts.ToArray());
        Indices = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, inds.Count, BufferUsage.WriteOnly);
        Indices.SetData(inds.ToArray());
        chunk.Dirty = false;
    }

    public void Draw(GraphicsDevice gd)
    {
        if (Vertices == null || Indices == null || IndexCount == 0)
            return;
        gd.SetVertexBuffer(Vertices);
        gd.Indices = Indices;
        gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexCount / 3);
    }

    public void Dispose()
    {
        Vertices?.Dispose();
        Indices?.Dispose();
    }
}
