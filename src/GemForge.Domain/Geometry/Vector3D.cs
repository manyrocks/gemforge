namespace GemForge.Domain.Geometry;

/// <summary>
/// Represents a 3D vector with X, Y, Z components.
/// </summary>
public readonly struct Vector3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Calculates the length (magnitude) of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Calculates the squared length of the vector (faster than Length).
    /// </summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Returns a normalized (unit length) version of this vector.
    /// </summary>
    public Vector3D Normalize()
    {
        var length = Length;
        if (length < double.Epsilon)
            return new Vector3D(0, 0, 0);
        return new Vector3D(X / length, Y / length, Z / length);
    }

    /// <summary>
    /// Calculates the dot product with another vector.
    /// </summary>
    public double Dot(Vector3D other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    /// <summary>
    /// Calculates the cross product with another vector.
    /// </summary>
    public Vector3D Cross(Vector3D other)
    {
        return new Vector3D(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }

    /// <summary>
    /// Calculates the distance to another vector.
    /// </summary>
    public double DistanceTo(Vector3D other)
    {
        return (this - other).Length;
    }

    public static Vector3D operator +(Vector3D a, Vector3D b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3D operator -(Vector3D a, Vector3D b)
        => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3D operator *(Vector3D v, double scalar)
        => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator *(double scalar, Vector3D v)
        => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator /(Vector3D v, double scalar)
        => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static Vector3D operator -(Vector3D v)
        => new(-v.X, -v.Y, -v.Z);

    /// <summary>
    /// Checks equality with tolerance for floating-point comparison.
    /// </summary>
    public bool Equals(Vector3D other, double tolerance = 1e-10)
    {
        return Math.Abs(X - other.X) < tolerance &&
               Math.Abs(Y - other.Y) < tolerance &&
               Math.Abs(Z - other.Z) < tolerance;
    }

    public override bool Equals(object? obj)
        => obj is Vector3D other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    public override string ToString()
        => $"({X:F4}, {Y:F4}, {Z:F4})";

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);
}
