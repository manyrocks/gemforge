using GemForge.Domain.Geometry;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Represents a triangulated 3D mesh with vertices, triangles, and edges.
/// </summary>
public class Mesh
{
    /// <summary>
    /// Array of 3D vertex positions.
    /// </summary>
    public Vector3D[] Vertices { get; set; } = Array.Empty<Vector3D>();

    /// <summary>
    /// Triangle indices (groups of 3 vertex indices).
    /// Each triplet (i, i+1, i+2) defines one triangle.
    /// </summary>
    public int[] TriangleIndices { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Edge pairs (vertex index tuples).
    /// Represents sharp edges that should be rendered as wireframe.
    /// </summary>
    public (int, int)[] Edges { get; set; } = Array.Empty<(int, int)>();

    /// <summary>
    /// Gets the number of vertices in the mesh.
    /// </summary>
    public int VertexCount => Vertices.Length;

    /// <summary>
    /// Gets the number of triangles in the mesh.
    /// </summary>
    public int TriangleCount => TriangleIndices.Length / 3;

    /// <summary>
    /// Gets the number of edges in the mesh.
    /// </summary>
    public int EdgeCount => Edges.Length;

    /// <summary>
    /// Calculates the bounding box of the mesh.
    /// </summary>
    public (Vector3D min, Vector3D max) GetBounds()
    {
        if (Vertices.Length == 0)
            return (Vector3D.Zero, Vector3D.Zero);

        var minX = Vertices.Min(v => v.X);
        var minY = Vertices.Min(v => v.Y);
        var minZ = Vertices.Min(v => v.Z);
        var maxX = Vertices.Max(v => v.X);
        var maxY = Vertices.Max(v => v.Y);
        var maxZ = Vertices.Max(v => v.Z);

        return (new Vector3D(minX, minY, minZ), new Vector3D(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Calculates the center point of the mesh.
    /// </summary>
    public Vector3D GetCenter()
    {
        if (Vertices.Length == 0)
            return Vector3D.Zero;

        return new Vector3D(
            Vertices.Average(v => v.X),
            Vertices.Average(v => v.Y),
            Vertices.Average(v => v.Z)
        );
    }

    /// <summary>
    /// Calculates the maximum extent (half-size) of the mesh.
    /// </summary>
    public double GetMaxExtent()
    {
        if (Vertices.Length == 0)
            return 0;

        var (min, max) = GetBounds();
        var size = max - min;
        return Math.Max(Math.Max(Math.Abs(size.X), Math.Abs(size.Y)), Math.Abs(size.Z)) / 2.0;
    }

    /// <summary>
    /// Validates the mesh structure.
    /// </summary>
    public bool IsValid()
    {
        // Check triangle indices are in valid range
        if (TriangleIndices.Any(i => i < 0 || i >= Vertices.Length))
            return false;

        // Check triangle count is multiple of 3
        if (TriangleIndices.Length % 3 != 0)
            return false;

        // Check edge indices are valid
        if (Edges.Any(e => e.Item1 < 0 || e.Item1 >= Vertices.Length ||
                           e.Item2 < 0 || e.Item2 >= Vertices.Length))
            return false;

        return true;
    }

    public override string ToString()
        => $"Mesh(Vertices={VertexCount}, Triangles={TriangleCount}, Edges={EdgeCount})";
}
