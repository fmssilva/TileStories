using UnityEngine;

namespace TileStories
{
    // The inside-out sphere a 360 picture is painted on (_3.1 step 10A.4.1): the camera stands at its centre and looks out at the
    // picture. Every vertex sits where EquirectRule says its picture point looks, and carries that point as its uv, so "ahead"
    // (+Z) is the middle of the picture exactly as the default-library generator drew it, with no mirroring to undo. Pure (plain
    // arrays, no Mesh), so the geometry is a unit test; CardPreviewStage only copies the arrays into a Mesh.
    public static class PanoramaSphereRule
    {
        public const int DefaultSegments = 64;
        public const int DefaultRings = 32;

        public readonly struct Geometry
        {
            public readonly Vector3[] Vertices;
            public readonly Vector2[] Uvs;
            public readonly int[] Triangles;

            public Geometry(Vector3[] vertices, Vector2[] uvs, int[] triangles)
            {
                Vertices = vertices;
                Uvs = uvs;
                Triangles = triangles;
            }
        }

        // `segments` around (yaw) and `rings` from the bottom pole to the top pole (pitch), a grid of (segments + 1) x (rings + 1)
        // vertices: the first and last column hold the same direction with u = 0 and u = 1, so the picture's seam never stretches
        // a texel across the wrap. Triangles wind clockwise seen from INSIDE, which is the side Unity draws.
        public static Geometry Build(float radius, int segments = DefaultSegments, int rings = DefaultRings)
        {
            segments = Mathf.Max(3, segments);
            rings = Mathf.Max(2, rings);
            int columns = segments + 1;
            var vertices = new Vector3[columns * (rings + 1)];
            var uvs = new Vector2[vertices.Length];
            for (int row = 0; row <= rings; row++)
            {
                for (int column = 0; column <= segments; column++)
                {
                    float u = (float)column / segments;
                    float v = (float)row / rings;
                    int index = row * columns + column;
                    vertices[index] = EquirectRule.DirectionOf(u, v) * radius;
                    uvs[index] = new Vector2(u, v);
                }
            }

            var triangles = new int[segments * rings * 6];
            int t = 0;
            for (int row = 0; row < rings; row++)
            {
                for (int column = 0; column < segments; column++)
                {
                    int bottomLeft = row * columns + column;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + columns;
                    int topRight = topLeft + 1;
                    // - seen from the centre u grows to the right and v grows up: bottom-left -> top-left -> top-right is clockwise
                    triangles[t++] = bottomLeft; triangles[t++] = topLeft; triangles[t++] = topRight;
                    triangles[t++] = bottomLeft; triangles[t++] = topRight; triangles[t++] = bottomRight;
                }
            }
            return new Geometry(vertices, uvs, triangles);
        }
    }
}
