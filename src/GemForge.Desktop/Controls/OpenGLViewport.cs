using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GemForge.Desktop.Rendering;
using GemForge.Infrastructure.Rendering;
using Silk.NET.OpenGL;
using SilkWindow = Silk.NET.Windowing.Window;

namespace GemForge.Desktop.Controls;

/// <summary>
/// Avalonia control that hosts an OpenGL viewport for rendering 3D gems.
/// </summary>
public class OpenGLViewport : UserControl
{
    private Silk.NET.Windowing.IWindow? _window;
    private GL? _gl;
    private GemRenderer? _renderer;
    private Camera? _camera;
    private TrackballController? _controller;
    private DispatcherTimer? _renderTimer;
    private Mesh? _currentMesh;

    public static readonly StyledProperty<Mesh?> MeshProperty =
        AvaloniaProperty.Register<OpenGLViewport, Mesh?>(nameof(Mesh));

    public Mesh? Mesh
    {
        get => GetValue(MeshProperty);
        set => SetValue(MeshProperty, value);
    }

    public OpenGLViewport()
    {
        Background = Brushes.Black;
        Focusable = true;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    static OpenGLViewport()
    {
        MeshProperty.Changed.AddClassHandler<OpenGLViewport>((control, args) =>
        {
            if (args.NewValue is Mesh mesh)
            {
                control.OnMeshChanged(mesh);
            }
        });
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InitializeOpenGL();
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CleanupOpenGL();
    }

    private void InitializeOpenGL()
    {
        try
        {
            // Create Silk.NET window (invisible, just for OpenGL context)
            var options = Silk.NET.Windowing.WindowOptions.Default;
            options.IsVisible = false;
            options.Size = new Silk.NET.Maths.Vector2D<int>(800, 600);
            options.Title = "GemForge OpenGL Context";

            _window = SilkWindow.Create(options);
            _window.Load += OnWindowLoad;
            _window.Render += OnWindowRender;
            _window.FramebufferResize += OnWindowResize;

            // Initialize components
            _camera = new Camera
            {
                Position = new Vector3(0, 0, 3),
                Target = Vector3.Zero,
                AspectRatio = (float)Bounds.Width / Math.Max(1, (float)Bounds.Height)
            };

            _controller = new TrackballController();

            // Start render loop
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _renderTimer.Tick += (s, e) => InvalidateVisual();
            _renderTimer.Start();
        }
        catch (Exception ex)
        {
            // OpenGL initialization failed - this is expected in some environments
            // For now, just show a placeholder
            System.Diagnostics.Debug.WriteLine($"OpenGL initialization failed: {ex.Message}");
        }
    }

    private void OnWindowLoad()
    {
        _gl = GL.GetApi(_window);
        _renderer = new GemRenderer(_gl);
        _renderer.Initialize();

        if (_currentMesh != null)
        {
            _renderer.UpdateMesh(_currentMesh);
        }

        _gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
    }

    private void OnWindowRender(double deltaTime)
    {
        if (_gl == null || _renderer == null || _camera == null || _controller == null)
            return;

        _controller.UpdateCamera(_camera);
        var viewProjection = _camera.GetViewProjectionMatrix();
        _renderer.Render(viewProjection);
    }

    private void OnWindowResize(Silk.NET.Maths.Vector2D<int> size)
    {
        if (_renderer != null && _camera != null)
        {
            _renderer.SetViewport(size.X, size.Y);
            _camera.AspectRatio = (float)size.X / Math.Max(1, size.Y);
        }
    }

    private void OnMeshChanged(Mesh mesh)
    {
        _currentMesh = mesh;
        _renderer?.UpdateMesh(mesh);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty && _camera != null)
        {
            _camera.AspectRatio = (float)Bounds.Width / Math.Max(1, (float)Bounds.Height);
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

    private void CleanupOpenGL()
    {
        _renderTimer?.Stop();
        _renderTimer = null;

        _renderer?.Dispose();
        _renderer = null;

        _window?.Dispose();
        _window = null;

        _gl?.Dispose();
        _gl = null;
    }
}
