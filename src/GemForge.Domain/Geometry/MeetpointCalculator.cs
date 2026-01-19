using GemForge.Domain.Models;

namespace GemForge.Domain.Geometry;

/// <summary>
/// Calculates meetpoints (vertices) where three facet planes intersect.
/// </summary>
public static class MeetpointCalculator
{
    /// <summary>
    /// Finds all valid meetpoints for a gem design.
    /// A meetpoint is where three facet planes intersect to form a vertex.
    /// </summary>
    /// <param name="facets">All facets in the gem design</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>List of unique 3D vertices</returns>
    public static List<Vector3D> FindAllMeetpoints(List<Facet> facets, IndexGear gear)
    {
        var planes = new List<(Plane3D plane, Facet facet, int index)>();

        // Generate all planes from facets at their index positions
        foreach (var facet in facets)
        {
            foreach (var indexPos in facet.IndexPositions)
            {
                var plane = Plane3D.FromFacet(facet, indexPos, gear);
                planes.Add((plane, facet, indexPos));
            }
        }

        var meetpoints = new List<Vector3D>();
        var tolerance = 1e-6;

        // Try all combinations of 3 planes
        for (int i = 0; i < planes.Count; i++)
        {
            for (int j = i + 1; j < planes.Count; j++)
            {
                for (int k = j + 1; k < planes.Count; k++)
                {
                    var intersection = Plane3D.Intersect(
                        planes[i].plane,
                        planes[j].plane,
                        planes[k].plane
                    );

                    if (intersection.HasValue)
                    {
                        var point = intersection.Value;

                        // Validate the meetpoint
                        if (IsValidMeetpoint(point, planes.Select(p => p.plane).ToList()))
                        {
                            // Check if this is a duplicate
                            if (!meetpoints.Any(mp => mp.Equals(point, tolerance)))
                            {
                                meetpoints.Add(point);
                            }
                        }
                    }
                }
            }
        }

        return meetpoints;
    }

    /// <summary>
    /// Validates if a meetpoint is geometrically valid for the gem.
    /// Filters out points that are clearly outside the gem bounds.
    /// </summary>
    private static bool IsValidMeetpoint(Vector3D point, List<Plane3D> allPlanes)
    {
        // Basic sanity checks
        // 1. Not too far from origin (gems are roughly unit scale)
        if (point.Length > 10.0)
            return false;

        // 2. Not at origin (degenerate case)
        if (point.Length < 1e-6)
            return false;

        // 3. Check if point is on the "correct" side of most planes
        // For a convex gem, valid vertices should be on or inside most facet planes
        var onCorrectSide = 0;
        var tolerance = 1e-4;

        foreach (var plane in allPlanes)
        {
            var distance = plane.DistanceToPoint(point);
            // Point should be on or inside the plane (negative distance = inside for outward normals)
            if (distance <= tolerance)
                onCorrectSide++;
        }

        // Valid meetpoint should be inside or on most planes
        // Use a threshold to allow for some tolerance
        return onCorrectSide >= allPlanes.Count * 0.5;
    }

    /// <summary>
    /// Removes duplicate meetpoints within tolerance.
    /// </summary>
    public static List<Vector3D> RemoveDuplicates(List<Vector3D> points, double tolerance = 1e-6)
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

    /// <summary>
    /// Calculates the bounding box of a set of points.
    /// </summary>
    public static (Vector3D min, Vector3D max) GetBounds(List<Vector3D> points)
    {
        if (points.Count == 0)
            return (Vector3D.Zero, Vector3D.Zero);

        var minX = points.Min(p => p.X);
        var minY = points.Min(p => p.Y);
        var minZ = points.Min(p => p.Z);
        var maxX = points.Max(p => p.X);
        var maxY = points.Max(p => p.Y);
        var maxZ = points.Max(p => p.Z);

        return (new Vector3D(minX, minY, minZ), new Vector3D(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Centers a set of points around the origin and returns the translation.
    /// </summary>
    public static (List<Vector3D> centered, Vector3D offset) CenterPoints(List<Vector3D> points)
    {
        if (points.Count == 0)
            return (points, Vector3D.Zero);

        var center = new Vector3D(
            points.Average(p => p.X),
            points.Average(p => p.Y),
            points.Average(p => p.Z)
        );

        var centered = points.Select(p => p - center).ToList();
        return (centered, center);
    }
}
