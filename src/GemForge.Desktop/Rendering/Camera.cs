using System;
using System.Numerics;

namespace GemForge.Desktop.Rendering;

/// <summary>
/// Represents a 3D perspective camera.
/// </summary>
public class Camera
{
    /// <summary>
    /// Camera position in world space.
    /// </summary>
    public Vector3 Position { get; set; } = new Vector3(0, 0, 3);

    /// <summary>
    /// Point the camera is looking at.
    /// </summary>
    public Vector3 Target { get; set; } = Vector3.Zero;

    /// <summary>
    /// Up vector (typically (0, 1, 0)).
    /// </summary>
    public Vector3 Up { get; set; } = Vector3.UnitY;

    /// <summary>
    /// Field of view in degrees.
    /// </summary>
    public float FieldOfView { get; set; } = 45.0f;

    /// <summary>
    /// Aspect ratio (width / height).
    /// </summary>
    public float AspectRatio { get; set; } = 1.0f;

    /// <summary>
    /// Near clipping plane distance.
    /// </summary>
    public float NearPlane { get; set; } = 0.1f;

    /// <summary>
    /// Far clipping plane distance.
    /// </summary>
    public float FarPlane { get; set; } = 100.0f;

    /// <summary>
    /// Calculates the view matrix (transforms world space to camera space).
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Target, Up);
    }

    /// <summary>
    /// Calculates the projection matrix (transforms camera space to clip space).
    /// </summary>
    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView * MathF.PI / 180.0f,
            AspectRatio,
            NearPlane,
            FarPlane
        );
    }

    /// <summary>
    /// Calculates the combined view-projection matrix.
    /// </summary>
    public Matrix4x4 GetViewProjectionMatrix()
    {
        return GetViewMatrix() * GetProjectionMatrix();
    }

    /// <summary>
    /// Gets the forward direction vector (from camera to target).
    /// </summary>
    public Vector3 Forward => Vector3.Normalize(Target - Position);

    /// <summary>
    /// Gets the right direction vector.
    /// </summary>
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Up));

    /// <summary>
    /// Gets the actual up direction vector (perpendicular to forward and right).
    /// </summary>
    public Vector3 ActualUp => Vector3.Cross(Right, Forward);

    /// <summary>
    /// Gets the distance from camera to target.
    /// </summary>
    public float Distance => (Position - Target).Length();
}
