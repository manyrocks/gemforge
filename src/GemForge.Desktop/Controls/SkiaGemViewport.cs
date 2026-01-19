using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using GemForge.Desktop.Rendering;
using GemForge.Infrastructure.Rendering;
using SkiaSharp;

namespace GemForge.Desktop.Controls;

/// <summary>
/// Avalonia control that renders 3D gems using SkiaSharp.
/// </summary>
public class SkiaGemViewport : Control
{
    private Camera? _camera;
    private TrackballController? _controller;
    private Mesh? _currentMesh;
    private DispatcherTimer? _renderTimer;

    public static readonly StyledProperty<Mesh?> MeshProperty =
        AvaloniaProperty.Register<SkiaGemViewport, Mesh?>(nameof(Mesh));

    public Mesh? Mesh
    {
        get => GetValue(MeshProperty);
        set => SetValue(MeshProperty, value);
    }

    static SkiaGemViewport()
    {
        AffectsRender<SkiaGemViewport>(MeshProperty);

        MeshProperty.Changed.AddClassHandler<SkiaGemViewport>((control, args) =>
        {
            if (args.NewValue is Mesh mesh)
            {
                control._currentMesh = mesh;
                control.InvalidateVisual();
            }
        });
    }

    public SkiaGemViewport()
    {
        Focusable = true;
        ClipToBounds = true;

        _camera = new Camera
        {
            Position = new Vector3(0, 0, 3),
            Target = Vector3.Zero,
            AspectRatio = 1.0f
        };

        _controller = new TrackballController();

        // Setup render timer for smooth animation
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _renderTimer.Tick += (s, e) =>
        {
            _controller?.UpdateCamera(_camera);
            InvalidateVisual();
        };
        _renderTimer.Start();

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_currentMesh == null || _camera == null)
            return;

        // Use custom SkiaSharp drawing
        context.Custom(new GemRenderOperation(_currentMesh, _camera, Bounds));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty && _camera != null)
        {
            _camera.AspectRatio = (float)Bounds.Width / Math.Max(1f, (float)Bounds.Height);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _controller?.OnMouseDown(e);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        _controller?.OnMouseMove(e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _controller?.OnMouseUp(e);
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _controller?.OnScroll(e);
    }

    /// <summary>
    /// Custom render operation for SkiaSharp 3D rendering.
    /// </summary>
    private class GemRenderOperation : ICustomDrawOperation
    {
        private readonly Mesh _mesh;
        private readonly Camera _camera;
        private readonly Rect _bounds;

        public GemRenderOperation(Mesh mesh, Camera camera, Rect bounds)
        {
            _mesh = mesh;
            _camera = camera;
            _bounds = bounds;
        }

        public void Dispose() { }

        public Rect Bounds => _bounds;

        public bool HitTest(Point p) => _bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature));
            if (leaseFeature == null)
                return;

            var skiaFeature = leaseFeature as ISkiaSharpApiLeaseFeature;
            if (skiaFeature == null)
                return;

            using var lease = skiaFeature.Lease();
            var canvas = lease?.SkCanvas;
            if (canvas == null)
                return;

            canvas.Clear(new SKColor(51, 51, 51)); // Dark gray background

            // Get view-projection matrix
            var viewProjection = _camera.GetViewProjectionMatrix();

            // Project and render triangles
            RenderMesh(canvas, viewProjection);
        }

        private void RenderMesh(SKCanvas canvas, Matrix4x4 mvp)
        {
            if (_mesh.Vertices.Length == 0)
                return;

            var width = (float)_bounds.Width;
            var height = (float)_bounds.Height;

            // Project all vertices to screen space
            var screenPoints = new SKPoint[_mesh.Vertices.Length];
            var depths = new float[_mesh.Vertices.Length];

            for (int i = 0; i < _mesh.Vertices.Length; i++)
            {
                var v = _mesh.Vertices[i];
                var pos = new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1.0f);
                var projected = Vector4.Transform(pos, mvp);

                if (Math.Abs(projected.W) > 0.0001f)
                {
                    projected /= projected.W;
                }

                // Convert NDC (-1 to 1) to screen coordinates
                screenPoints[i] = new SKPoint(
                    (projected.X + 1) * width / 2,
                    (-projected.Y + 1) * height / 2
                );
                depths[i] = projected.Z;
            }

            // Sort triangles by depth (painter's algorithm - back to front)
            // Back triangles drawn first, front triangles paint over them
            var triangleOrder = new List<(int index, float depth)>();
            for (int i = 0; i < _mesh.TriangleCount; i++)
            {
                var i0 = _mesh.TriangleIndices[i * 3];
                var i1 = _mesh.TriangleIndices[i * 3 + 1];
                var i2 = _mesh.TriangleIndices[i * 3 + 2];
                var avgDepth = (depths[i0] + depths[i1] + depths[i2]) / 3.0f;
                triangleOrder.Add((i, avgDepth));
            }
            triangleOrder.Sort((a, b) => b.depth.CompareTo(a.depth)); // Back to front

            // Draw filled triangles (solid gray) with strict backface culling
            using var facePaint = new SKPaint
            {
                Color = new SKColor(160, 160, 160, 255), // Fully opaque gray
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                BlendMode = SKBlendMode.Src // No alpha blending - completely replace pixels
            };

            // First pass: draw all triangles back to front
            // Opaque rendering means front triangles paint over back ones
            foreach (var item in triangleOrder)
            {
                var triIndex = item.index;
                var i0 = _mesh.TriangleIndices[triIndex * 3];
                var i1 = _mesh.TriangleIndices[triIndex * 3 + 1];
                var i2 = _mesh.TriangleIndices[triIndex * 3 + 2];

                var p0 = screenPoints[i0];
                var p1 = screenPoints[i1];
                var p2 = screenPoints[i2];

                // Skip degenerate triangles
                var area = Math.Abs((p1.X - p0.X) * (p2.Y - p0.Y) - (p1.Y - p0.Y) * (p2.X - p0.X));
                if (area < 0.1f) continue;

                using var path = new SKPath();
                path.MoveTo(p0);
                path.LineTo(p1);
                path.LineTo(p2);
                path.Close();

                canvas.DrawPath(path, facePaint);
            }

            // Second pass: draw black edges only on visible facet boundaries
            using var edgePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            // Draw all facet boundary edges
            foreach (var edge in _mesh.Edges)
            {
                var p1 = screenPoints[edge.Item1];
                var p2 = screenPoints[edge.Item2];
                canvas.DrawLine(p1, p2, edgePaint);
            }
        }

        private static void AddEdge(HashSet<(int, int)> edges, int v1, int v2)
        {
            var edge = v1 < v2 ? (v1, v2) : (v2, v1);
            edges.Add(edge);
        }
    }
}
