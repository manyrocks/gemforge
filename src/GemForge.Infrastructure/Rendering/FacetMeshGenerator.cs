using System;
using System.Collections.Generic;
using System.Linq;
using GemForge.Domain.Geometry;
using GemForge.Domain.Models;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Generates meshes by triangulating individual facets rather than using convex hull.
/// This produces cleaner geometry that respects the actual facet structure.
/// </summary>
public static class FacetMeshGenerator
{
    public static Mesh GenerateMesh(GemDesign design)
    {
        var allVertices = new List<Vector3D>();
        var allTriangles = new List<int>();
        var allEdges = new HashSet<(int, int)>();

        // Generate all plane instances
        var planes = new List<(Plane3D plane, Facet facet, int indexPos)>();
        foreach (var facet in design.Facets)
        {
            foreach (var indexPos in facet.IndexPositions)
            {
                var plane = Plane3D.FromFacet(facet, indexPos, design.IndexGear);
                planes.Add((plane, facet, indexPos));
            }
        }

        // For each plane, find vertices on it and triangulate
        foreach (var (plane, facet, indexPos) in planes)
        {
            var verticesOnPlane = FindVerticesOnPlane(plane, planes);

            if (verticesOnPlane.Count < 3)
                continue; // Can't make a facet with fewer than 3 vertices

            // Order vertices around the facet perimeter
            var orderedVertices = OrderVerticesOnPlane(verticesOnPlane, plane);

            if (orderedVertices.Count < 3)
                continue;

            // Triangulate this facet
            var baseIndex = allVertices.Count;
            allVertices.AddRange(orderedVertices);

            // Simple fan triangulation from first vertex
            for (int i = 1; i < orderedVertices.Count - 1; i++)
            {
                allTriangles.Add(baseIndex);
                allTriangles.Add(baseIndex + i);
                allTriangles.Add(baseIndex + i + 1);

                // Add edges
                AddEdge(allEdges, baseIndex, baseIndex + i);
                AddEdge(allEdges, baseIndex + i, baseIndex + i + 1);
                if (i == orderedVertices.Count - 2)
                    AddEdge(allEdges, baseIndex + i + 1, baseIndex);
            }
        }

        if (allVertices.Count == 0)
            return new Mesh();

        // Normalize to unit scale
        var mesh = new Mesh
        {
            Vertices = allVertices.ToArray(),
            TriangleIndices = allTriangles.ToArray(),
            Edges = allEdges.ToArray()
        };

        return NormalizeToUnitScale(mesh);
    }

    private static List<Vector3D> FindVerticesOnPlane(Plane3D targetPlane, List<(Plane3D plane, Facet facet, int indexPos)> allPlanes)
    {
        var vertices = new List<Vector3D>();
        var tolerance = 1e-4;

        // Find all 3-plane intersections that include this plane
        for (int i = 0; i < allPlanes.Count; i++)
        {
            if (!ArePlanesEqual(allPlanes[i].plane, targetPlane, tolerance))
                continue;

            for (int j = 0; j < allPlanes.Count; j++)
            {
                if (i == j || ArePlanesEqual(allPlanes[j].plane, targetPlane, tolerance))
                    continue;

                for (int k = j + 1; k < allPlanes.Count; k++)
                {
                    if (i == k || ArePlanesEqual(allPlanes[k].plane, targetPlane, tolerance))
                        continue;

                    var intersection = Plane3D.Intersect(
                        allPlanes[i].plane,
                        allPlanes[j].plane,
                        allPlanes[k].plane
                    );

                    if (intersection.HasValue)
                    {
                        var point = intersection.Value;

                        // Check if this point is valid and not a duplicate
                        if (IsValidVertex(point) && !vertices.Any(v => v.Equals(point, tolerance)))
                        {
                            vertices.Add(point);
                        }
                    }
                }
            }
        }

        return vertices;
    }

    private static bool ArePlanesEqual(Plane3D p1, Plane3D p2, double tolerance)
    {
        // Planes are equal if normals are parallel and distances are equal
        var normalDot = Math.Abs(p1.Normal.Dot(p2.Normal));
        var distanceMatch = Math.Abs(p1.Distance - p2.Distance) < tolerance;

        return normalDot > (1.0 - tolerance) && distanceMatch;
    }

    private static bool IsValidVertex(Vector3D point)
    {
        // Basic sanity checks
        if (point.Length > 10.0 || point.Length < 1e-6)
            return false;

        return true;
    }

    private static List<Vector3D> OrderVerticesOnPlane(List<Vector3D> vertices, Plane3D plane)
    {
        if (vertices.Count < 3)
            return vertices;

        // Calculate centroid
        var centroid = new Vector3D(
            vertices.Average(v => v.X),
            vertices.Average(v => v.Y),
            vertices.Average(v => v.Z)
        );

        // Create a 2D coordinate system on the plane
        var normal = plane.Normal;

        // Find two perpendicular vectors on the plane
        var up = Math.Abs(normal.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0);
        var u = up.Cross(normal).Normalize();
        var v = normal.Cross(u).Normalize();

        // Convert vertices to 2D coordinates and sort by angle
        var vertices2D = vertices.Select(vertex =>
        {
            var relative = vertex - centroid;
            var x = relative.Dot(u);
            var y = relative.Dot(v);
            var angle = Math.Atan2(y, x);
            return (vertex, angle);
        }).OrderBy(item => item.angle).Select(item => item.vertex).ToList();

        return vertices2D;
    }

    private static void AddEdge(HashSet<(int, int)> edges, int v1, int v2)
    {
        var edge = v1 < v2 ? (v1, v2) : (v2, v1);
        edges.Add(edge);
    }

    private static Mesh NormalizeToUnitScale(Mesh mesh)
    {
        if (mesh.Vertices.Length == 0)
            return mesh;

        var maxExtent = mesh.GetMaxExtent();
        if (maxExtent < double.Epsilon)
            return mesh;

        var scale = 1.0 / maxExtent;
        var normalizedVertices = mesh.Vertices.Select(v => v * scale).ToArray();

        return new Mesh
        {
            Vertices = normalizedVertices,
            TriangleIndices = mesh.TriangleIndices,
            Edges = mesh.Edges
        };
    }
}
