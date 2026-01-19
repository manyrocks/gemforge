using System;
using System.Collections.Generic;
using GemForge.Domain.Geometry;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// CSG (Constructive Solid Geometry) operations for cutting polygons with planes.
/// Based on the GemCad polygon cutting algorithm.
/// </summary>
public static class CsgOperations
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Cuts a polygon by a plane, keeping only the portion on the positive side of the plane.
    /// Returns the intersection points where the plane crossed the polygon edges.
    /// The input polygon is modified in place.
    /// </summary>
    public static List<Vector3D> CutPolygonByPlane(CsgPolygon polygon, Plane3D plane)
    {
        var intersectionPoints = new List<Vector3D>();

        if (polygon.Vertices.Count < 3)
            return intersectionPoints;

        // Step 1: Find all edge-plane intersections
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            var v0 = polygon.Vertices[i];
            var v1 = polygon.Vertices[(i + 1) % polygon.Vertices.Count];

            var intersection = FindEdgePlaneIntersection(v0, v1, plane);
            if (intersection.HasValue)
            {
                intersectionPoints.Add(intersection.Value);
            }
        }

        // Step 2: Remove vertices on the positive side of the plane (cut away the outside)
        // For inward-pointing normals, positive side is outside the gem
        for (int i = polygon.Vertices.Count - 1; i >= 0; i--)
        {
            var distance = plane.DistanceToPoint(polygon.Vertices[i]);
            if (distance > Tolerance) // On positive side - remove it (outside)
            {
                polygon.RemoveVertex(i);
            }
        }

        // Step 3: Add intersection points
        foreach (var point in intersectionPoints)
        {
            polygon.AddVertex(point);
        }

        // Step 4: Reorder vertices if we still have a valid polygon
        if (polygon.Vertices.Count >= 3)
        {
            polygon.ReorderVertices();
        }

        return intersectionPoints;
    }

    /// <summary>
    /// Finds the intersection point between a line segment (v0 to v1) and a plane.
    /// Returns null if no intersection exists within the segment.
    /// </summary>
    private static Vector3D? FindEdgePlaneIntersection(Vector3D v0, Vector3D v1, Plane3D plane)
    {
        var direction = v1 - v0;
        var normal = plane.Normal;

        // Check if edge is parallel to plane
        var denominator = normal.Dot(direction);
        if (Math.Abs(denominator) < Tolerance)
            return null;

        // Calculate intersection parameter t
        var numerator = plane.Distance - normal.Dot(v0);
        var t = numerator / denominator;

        // Check if intersection is within the segment [0, 1]
        if (t < -Tolerance || t > 1.0 + Tolerance)
            return null;

        // Clamp t to [0, 1] to handle floating point errors at endpoints
        t = Math.Max(0, Math.Min(1, t));

        // Calculate intersection point
        var intersection = v0 + direction * t;

        return intersection;
    }

    /// <summary>
    /// Creates the initial cube that will be progressively cut to form the gem.
    /// </summary>
    public static List<CsgPolygon> CreateInitialCube(double size)
    {
        var halfSize = size / 2.0;
        var polygons = new List<CsgPolygon>();

        // All faces wound counter-clockwise when viewed from OUTSIDE (outward-facing normals)

        // Front face (positive Z) - normal points toward +Z
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(-halfSize, -halfSize, halfSize),
            new Vector3D(halfSize, -halfSize, halfSize),
            new Vector3D(halfSize, halfSize, halfSize),
            new Vector3D(-halfSize, halfSize, halfSize)
        }));

        // Back face (negative Z) - normal points toward -Z
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(-halfSize, -halfSize, -halfSize),
            new Vector3D(-halfSize, halfSize, -halfSize),
            new Vector3D(halfSize, halfSize, -halfSize),
            new Vector3D(halfSize, -halfSize, -halfSize)
        }));

        // Top face (positive Y) - normal points toward +Y
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(-halfSize, halfSize, -halfSize),
            new Vector3D(-halfSize, halfSize, halfSize),
            new Vector3D(halfSize, halfSize, halfSize),
            new Vector3D(halfSize, halfSize, -halfSize)
        }));

        // Bottom face (negative Y) - normal points toward -Y
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(-halfSize, -halfSize, -halfSize),
            new Vector3D(halfSize, -halfSize, -halfSize),
            new Vector3D(halfSize, -halfSize, halfSize),
            new Vector3D(-halfSize, -halfSize, halfSize)
        }));

        // Right face (positive X) - normal points toward +X
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(halfSize, -halfSize, -halfSize),
            new Vector3D(halfSize, halfSize, -halfSize),
            new Vector3D(halfSize, halfSize, halfSize),
            new Vector3D(halfSize, -halfSize, halfSize)
        }));

        // Left face (negative X) - normal points toward -X
        polygons.Add(new CsgPolygon(new[]
        {
            new Vector3D(-halfSize, -halfSize, -halfSize),
            new Vector3D(-halfSize, -halfSize, halfSize),
            new Vector3D(-halfSize, halfSize, halfSize),
            new Vector3D(-halfSize, halfSize, -halfSize)
        }));

        return polygons;
    }
}
