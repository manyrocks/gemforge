using System;
using System.Collections.Generic;
using System.Linq;
using GemForge.Domain.Geometry;
using GemForge.Domain.Models;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Generates gem meshes using CSG (Constructive Solid Geometry) approach.
/// Progressively cuts a cube with facet planes to create the gem shape.
/// </summary>
public static class CsgMeshGenerator
{
    public static Mesh GenerateMesh(GemDesign design)
    {
        // Step 1: Create initial cube (large enough to contain the gem)
        var polygons = CsgOperations.CreateInitialCube(20.0);

        // Step 2: Generate all facet planes
        var planes = GenerateFacetPlanes(design);

        // Step 3: Cut the cube with each plane sequentially
        foreach (var (plane, facet, indexPos) in planes)
        {
            var newPolygons = new List<CsgPolygon>();
            var cutSurfacePoints = new List<Vector3D>();

            // Cut each existing polygon with this plane
            foreach (var polygon in polygons)
            {
                if (!polygon.IsValid())
                    continue;

                // Cut the polygon and collect intersection points
                var intersectionPoints = CsgOperations.CutPolygonByPlane(polygon, plane);
                cutSurfacePoints.AddRange(intersectionPoints);

                // Keep the polygon if it still has vertices after cutting
                if (polygon.IsValid())
                {
                    newPolygons.Add(polygon);
                }
            }

            // Create a new polygon from the cut surface itself
            if (cutSurfacePoints.Count >= 3)
            {
                // Remove duplicates
                var uniquePoints = RemoveDuplicatePoints(cutSurfacePoints);

                if (uniquePoints.Count >= 3)
                {
                    var cutPolygon = new CsgPolygon(uniquePoints);
                    cutPolygon.Label = facet.TierName;
                    cutPolygon.ReorderVertices();
                    newPolygons.Add(cutPolygon);
                }
            }

            polygons = newPolygons;
        }

        // Step 4: Convert polygons to mesh
        return ConvertPolygonsToMesh(polygons);
    }

    private static List<(Plane3D plane, Facet facet, int indexPos)> GenerateFacetPlanes(GemDesign design)
    {
        var planes = new List<(Plane3D, Facet, int)>();

        foreach (var facet in design.Facets)
        {
            foreach (var indexPos in facet.IndexPositions)
            {
                var plane = Plane3D.FromFacet(facet, indexPos, design.IndexGear);
                planes.Add((plane, facet, indexPos));
            }
        }

        return planes;
    }

    private static List<Vector3D> RemoveDuplicatePoints(List<Vector3D> points, double tolerance = 1e-6)
    {
        var unique = new List<Vector3D>();

        foreach (var point in points)
        {
            if (!unique.Any(p => p.Equals(point, tolerance)))
            {
                unique.Add(point);
            }
        }

        return unique;
    }

    private static Mesh ConvertPolygonsToMesh(List<CsgPolygon> polygons)
    {
        var uniqueVertices = new List<Vector3D>();
        var allTriangles = new List<int>();
        var allEdges = new HashSet<(int, int)>();
        const double vertexMergeTolerance = 1e-6;

        foreach (var polygon in polygons)
        {
            if (!polygon.IsValid())
                continue;

            // Map polygon vertex indices to merged vertex indices
            var vertexIndices = new List<int>();

            foreach (var vertex in polygon.Vertices)
            {
                // Try to find existing vertex within tolerance
                int existingIndex = -1;
                for (int i = 0; i < uniqueVertices.Count; i++)
                {
                    if (uniqueVertices[i].Equals(vertex, vertexMergeTolerance))
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    // Reuse existing vertex
                    vertexIndices.Add(existingIndex);
                }
                else
                {
                    // Add new unique vertex
                    vertexIndices.Add(uniqueVertices.Count);
                    uniqueVertices.Add(vertex);
                }
            }

            // Triangulate polygon using merged indices
            var triangles = polygon.Triangulate(0); // Get relative indices
            foreach (var triangle in triangles)
            {
                // Remap to merged vertex indices
                allTriangles.Add(vertexIndices[triangle[0]]);
                allTriangles.Add(vertexIndices[triangle[1]]);
                allTriangles.Add(vertexIndices[triangle[2]]);
            }

            // Add polygon edges using merged indices
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                var v1 = vertexIndices[i];
                var v2 = vertexIndices[(i + 1) % vertexIndices.Count];
                AddEdge(allEdges, v1, v2);
            }
        }

        if (uniqueVertices.Count == 0)
            return new Mesh();

        var mesh = new Mesh
        {
            Vertices = uniqueVertices.ToArray(),
            TriangleIndices = allTriangles.ToArray(),
            Edges = allEdges.ToArray()
        };

        return NormalizeToUnitScale(mesh);
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
