using System;
using GemForge.Domain.Models;

namespace GemForge.Domain.Geometry;

/// <summary>
/// Represents a plane in 3D space defined by ax + by + cz = d
/// where (a, b, c) is the normal vector and d is the distance from origin.
/// </summary>
public readonly struct Plane3D
{
    /// <summary>
    /// Normal vector to the plane (should be normalized).
    /// </summary>
    public Vector3D Normal { get; }

    /// <summary>
    /// Distance from the origin along the normal direction.
    /// </summary>
    public double Distance { get; }

    public Plane3D(Vector3D normal, double distance)
    {
        Normal = normal.Normalize();
        Distance = distance;
    }

    /// <summary>
    /// Creates a plane from a facet at a specific index position.
    /// </summary>
    /// <param name="facet">The facet defining the plane</param>
    /// <param name="indexPosition">The angular position on the index gear (0-gear size)</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>A plane in 3D space</returns>
    public static Plane3D FromFacet(Facet facet, int indexPosition, IndexGear gear)
    {
        // GemCad coordinate system:
        // - Angle: The angle of the dop/cutting head from horizontal
        //   - 0° = horizontal cut (table/culet)
        //   - Positive = crown angles (above girdle)
        //   - Negative = pavilion angles (below girdle)
        // - Distance: Radial distance from center axis
        // - Index: Angular position around the stone

        // GemCad uses spherical coordinates to define facet planes
        // Based on reference implementation: https://github.com/sftdevstar/GemCad-Viewer

        var azimuthAngle = IndexCalculator.IndexToAngle(indexPosition, gear);
        var alpha = facet.Angle * Math.PI / 180.0;  // Polar angle (facet angle)
        var beta = azimuthAngle * Math.PI / 180.0;   // Azimuthal angle (index position)
        var radius = facet.Distance;                 // Radial distance

        // Sign handling for below-girdle facets (pavilion)
        var sg = Math.Sign(alpha);
        if (sg == 0) sg = 1;

        // Spherical to Cartesian conversion
        // This point serves BOTH as a point on the plane AND as the normal vector
        var x = radius * Math.Sin(alpha) * Math.Cos(beta);
        var y = radius * Math.Sin(alpha) * Math.Sin(beta);
        var z = sg * radius * Math.Cos(alpha);

        var pointAndNormal = new Vector3D(x, y, z);

        // The plane passes through this point and has this normal
        var normal = pointAndNormal.Normalize();

        // Calculate perpendicular distance from origin: d = n · p
        var distance = normal.Dot(pointAndNormal);

        return new Plane3D(normal, distance);
    }

    /// <summary>
    /// Calculates the signed distance from a point to this plane.
    /// </summary>
    public double DistanceToPoint(Vector3D point)
    {
        return Normal.Dot(point) - Distance;
    }

    /// <summary>
    /// Finds the intersection point of three planes (meetpoint).
    /// Returns null if planes don't intersect at a unique point.
    /// </summary>
    public static Vector3D? Intersect(Plane3D p1, Plane3D p2, Plane3D p3)
    {
        // Solve the system:
        // n1·p = d1
        // n2·p = d2
        // n3·p = d3
        // Using Cramer's rule

        var n1 = p1.Normal;
        var n2 = p2.Normal;
        var n3 = p3.Normal;

        // Calculate determinant (triple product)
        var det = n1.Dot(n2.Cross(n3));

        // If determinant is near zero, planes are parallel or coincident
        if (Math.Abs(det) < 1e-10)
            return null;

        // Calculate intersection point using Cramer's rule
        var point = (p1.Distance * n2.Cross(n3) +
                     p2.Distance * n3.Cross(n1) +
                     p3.Distance * n1.Cross(n2)) / det;

        return point;
    }

    /// <summary>
    /// Checks if a point lies on this plane within tolerance.
    /// </summary>
    public bool Contains(Vector3D point, double tolerance = 1e-6)
    {
        return Math.Abs(DistanceToPoint(point)) < tolerance;
    }

    public override string ToString()
        => $"Plane(N={Normal}, D={Distance:F4})";
}
