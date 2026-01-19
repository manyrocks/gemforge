using System;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using GemForge.Desktop.Rendering;
using GemForge.Infrastructure.Rendering;
using GemForge.Infrastructure.FileFormats;

namespace GemForge.Desktop;

/// <summary>
/// Standalone OpenGL window for testing (separate from Avalonia).
/// </summary>
public class StandaloneOpenGLWindow
{
    private IWindow? _window;
    private GL? _gl;
    private GemRenderer? _renderer;
    private Camera? _camera;
    private TrackballController? _controller;
    private Mesh? _mesh;

    public static void Run(string ascFilePath)
    {
        var window = new StandaloneOpenGLWindow();
        window.LoadGem(ascFilePath);
        window.Start();
    }

    private void LoadGem(string ascFilePath)
    {
        var design = AscParser.ParseFile(ascFilePath);
        _mesh = MeshGenerator.GenerateMesh(design);
    }

    private void Start()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1024, 768);
        options.Title = "GemForge - Standalone 3D Viewer";

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClosing;

        _window.Run();
    }

    private void OnLoad()
    {
        if (_window == null) return;

        _gl = GL.GetApi(_window);
        _renderer = new GemRenderer(_gl);
        _renderer.Initialize();

        if (_mesh != null)
        {
            _renderer.UpdateMesh(_mesh);
        }

        _camera = new Camera
        {
            Position = new Vector3(0, 0, 3),
            Target = Vector3.Zero,
            AspectRatio = (float)_window.Size.X / _window.Size.Y
        };

        _controller = new TrackballController();

        _gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
    }

    private void OnRender(double deltaTime)
    {
        if (_gl == null || _renderer == null || _camera == null || _controller == null)
            return;

        _controller.UpdateCamera(_camera);
        var viewProjection = _camera.GetViewProjectionMatrix();
        _renderer.Render(viewProjection);
    }

    private void OnResize(Vector2D<int> size)
    {
        if (_renderer != null && _camera != null)
        {
            _renderer.SetViewport(size.X, size.Y);
            _camera.AspectRatio = (float)size.X / Math.Max(1, size.Y);
        }
    }

    private void OnClosing()
    {
        _renderer?.Dispose();
        _gl?.Dispose();
    }
}
