using System;
using System.Collections.Generic;
using System.Linq;
using GemForge.Domain.Geometry;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Represents a polygon for CSG (Constructive Solid Geometry) operations.
/// Used to progressively cut a cube to create gem geometry.
/// </summary>
public class CsgPolygon
{
    public List<Vector3D> Vertices { get; private set; }
    public string? Label { get; set; }

    public CsgPolygon()
    {
        Vertices = new List<Vector3D>();
    }

    public CsgPolygon(IEnumerable<Vector3D> vertices)
    {
        Vertices = new List<Vector3D>(vertices);
    }

    public void AddVertex(Vector3D vertex)
    {
        Vertices.Add(vertex);
    }

    public void RemoveVertex(int index)
    {
        if (index >= 0 && index < Vertices.Count)
        {
            Vertices.RemoveAt(index);
        }
    }

    /// <summary>
    /// Reorders vertices to be in consistent winding order around the polygon.
    /// Orders vertices by angle around the centroid and ensures outward-facing normal.
    /// </summary>
    public void ReorderVertices()
    {
        if (Vertices.Count < 3)
            return;

        // Calculate centroid
        var centroid = new Vector3D(
            Vertices.Average(v => v.X),
            Vertices.Average(v => v.Y),
            Vertices.Average(v => v.Z)
        );

        // Calculate normal (average of all triangle normals)
        var normal = Vector3D.Zero;
        for (int i = 0; i < Vertices.Count; i++)
        {
            var v1 = Vertices[i] - centroid;
            var v2 = Vertices[(i + 1) % Vertices.Count] - centroid;
            normal = normal + v1.Cross(v2);
        }
        normal = normal.Normalize();

        // Create basis vectors on the polygon plane
        var up = Math.Abs(normal.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0);
        var u = up.Cross(normal).Normalize();
        var v = normal.Cross(u).Normalize();

        // Sort vertices by angle around centroid
        var verticesWithAngles = Vertices.Select(vertex =>
        {
            var relative = vertex - centroid;
            var x = relative.Dot(u);
            var y = relative.Dot(v);
            var angle = Math.Atan2(y, x);
            return (vertex, angle);
        }).OrderBy(item => item.angle).ToList();

        Vertices = verticesWithAngles.Select(item => item.vertex).ToList();
    }

    /// <summary>
    /// Triangulates this polygon using fan triangulation.
    /// </summary>
    public List<int[]> Triangulate(int baseIndex)
    {
        var triangles = new List<int[]>();

        if (Vertices.Count < 3)
            return triangles;

        // Fan triangulation from first vertex
        for (int i = 1; i < Vertices.Count - 1; i++)
        {
            triangles.Add(new[] { baseIndex, baseIndex + i, baseIndex + i + 1 });
        }

        return triangles;
    }

    public bool IsValid()
    {
        return Vertices.Count >= 3;
    }
}
