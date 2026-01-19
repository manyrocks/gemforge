using System;
using System.Numerics;
using Avalonia.Input;

namespace GemForge.Desktop.Rendering;

/// <summary>
/// Controls camera movement using trackball/arcball rotation.
/// </summary>
public class TrackballController
{
    private Vector2 _previousMousePos;
    private bool _isRotating;
    private bool _isPanning;

    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _panOffset = Vector3.Zero;
    private float _zoomDistance = 3.0f;

    /// <summary>
    /// Rotation sensitivity in radians per pixel.
    /// </summary>
    public float RotationSensitivity { get; set; } = 0.005f;

    /// <summary>
    /// Pan sensitivity (world units per pixel).
    /// </summary>
    public float PanSensitivity { get; set; } = 0.003f;

    /// <summary>
    /// Zoom sensitivity.
    /// </summary>
    public float ZoomSensitivity { get; set; } = 0.1f;

    /// <summary>
    /// Minimum zoom distance.
    /// </summary>
    public float MinZoomDistance { get; set; } = 0.5f;

    /// <summary>
    /// Maximum zoom distance.
    /// </summary>
    public float MaxZoomDistance { get; set; } = 20.0f;

    /// <summary>
    /// Handles mouse button press.
    /// </summary>
    public void OnMouseDown(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(null);
        _previousMousePos = new Vector2((float)point.Position.X, (float)point.Position.Y);

        if (point.Properties.IsLeftButtonPressed && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            _isRotating = true;
        }
        else if (point.Properties.IsMiddleButtonPressed ||
                (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
            _isPanning = true;
        }
    }

    /// <summary>
    /// Handles mouse movement.
    /// </summary>
    public void OnMouseMove(PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(null);
        var currentPos = new Vector2((float)point.Position.X, (float)point.Position.Y);
        var delta = currentPos - _previousMousePos;

        if (_isRotating)
        {
            // Trackball rotation
            var deltaX = delta.X * RotationSensitivity;
            var deltaY = delta.Y * RotationSensitivity;

            // Create rotation quaternions for X and Y
            var rotY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -deltaX);
            var rotX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -deltaY);

            // Combine rotations
            _rotation = rotY * _rotation * rotX;
            _rotation = Quaternion.Normalize(_rotation);
        }
        else if (_isPanning)
        {
            // Pan in screen space
            var deltaX = delta.X * PanSensitivity;
            var deltaY = -delta.Y * PanSensitivity; // Invert Y for intuitive panning

            _panOffset += new Vector3(deltaX, deltaY, 0);
        }

        _previousMousePos = currentPos;
    }

    /// <summary>
    /// Handles mouse button release.
    /// </summary>
    public void OnMouseUp(PointerReleasedEventArgs e)
    {
        _isRotating = false;
        _isPanning = false;
    }

    /// <summary>
    /// Handles mouse scroll wheel.
    /// </summary>
    public void OnScroll(PointerWheelEventArgs e)
    {
        var delta = (float)e.Delta.Y;
        _zoomDistance -= delta * ZoomSensitivity;
        _zoomDistance = Math.Clamp(_zoomDistance, MinZoomDistance, MaxZoomDistance);
    }

    /// <summary>
    /// Updates the camera based on current controller state.
    /// </summary>
    public void UpdateCamera(Camera camera)
    {
        // Apply rotation to base position
        var rotatedPosition = Vector3.Transform(new Vector3(0, 0, _zoomDistance), _rotation);

        // Apply pan offset
        camera.Position = rotatedPosition + _panOffset;
        camera.Target = _panOffset;

        // Update up vector based on rotation
        camera.Up = Vector3.Transform(Vector3.UnitY, _rotation);
    }

    /// <summary>
    /// Resets the controller to default state.
    /// </summary>
    public void Reset()
    {
        _rotation = Quaternion.Identity;
        _panOffset = Vector3.Zero;
        _zoomDistance = 3.0f;
    }
}
