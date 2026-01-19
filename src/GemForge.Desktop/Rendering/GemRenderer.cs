using Silk.NET.OpenGL;
using System;
using System.IO;
using System.Numerics;
using GemForge.Infrastructure.Rendering;
using GemForge.Domain.Geometry;

namespace GemForge.Desktop.Rendering;

/// <summary>
/// Renders gem meshes using OpenGL.
/// </summary>
public class GemRenderer : IDisposable
{
    private readonly GL _gl;
    private ShaderProgram? _shader;
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private uint _edgeVbo;
    private uint _edgeEbo;
    private int _triangleCount;
    private int _edgeCount;
    private bool _hasMesh;

    public GemRenderer(GL gl)
    {
        _gl = gl;
    }

    /// <summary>
    /// Initializes the renderer (loads shaders, sets up OpenGL state).
    /// </summary>
    public void Initialize()
    {
        // Load shaders from embedded resources
        var vertexShader = LoadEmbeddedResource("basic.vert");
        var fragmentShader = LoadEmbeddedResource("basic.frag");

        _shader = new ShaderProgram(_gl, vertexShader, fragmentShader);

        // Create vertex array object
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Create buffers
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();
        _edgeVbo = _gl.GenBuffer();
        _edgeEbo = _gl.GenBuffer();

        // Set up OpenGL state
        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Less);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.FrontFace(FrontFaceDirection.Ccw);

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// Updates the mesh to render.
    /// </summary>
    public void UpdateMesh(Mesh mesh)
    {
        if (mesh == null || mesh.Vertices.Length == 0)
        {
            _hasMesh = false;
            return;
        }

        _gl.BindVertexArray(_vao);

        // Convert Vector3D to float array
        var vertices = new float[mesh.Vertices.Length * 3];
        for (int i = 0; i < mesh.Vertices.Length; i++)
        {
            vertices[i * 3] = (float)mesh.Vertices[i].X;
            vertices[i * 3 + 1] = (float)mesh.Vertices[i].Y;
            vertices[i * 3 + 2] = (float)mesh.Vertices[i].Z;
        }

        // Upload vertex data
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* ptr = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)),
                    ptr, BufferUsageARB.StaticDraw);
            }
        }

        // Set up vertex attributes
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);

        // Upload triangle indices
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (int* ptr = mesh.TriangleIndices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(mesh.TriangleIndices.Length * sizeof(int)),
                    ptr, BufferUsageARB.StaticDraw);
            }
        }

        _triangleCount = mesh.TriangleCount;

        // Upload edge indices
        var edgeIndices = new int[mesh.Edges.Length * 2];
        for (int i = 0; i < mesh.Edges.Length; i++)
        {
            edgeIndices[i * 2] = mesh.Edges[i].Item1;
            edgeIndices[i * 2 + 1] = mesh.Edges[i].Item2;
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _edgeEbo);
        unsafe
        {
            fixed (int* ptr = edgeIndices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(edgeIndices.Length * sizeof(int)),
                    ptr, BufferUsageARB.StaticDraw);
            }
        }

        _edgeCount = mesh.Edges.Length;
        _hasMesh = true;

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// Renders the gem mesh.
    /// </summary>
    public void Render(Matrix4x4 viewProjection)
    {
        if (!_hasMesh || _shader == null)
            return;

        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _gl.BindVertexArray(_vao);

        // Pass 1: Draw filled faces (gray)
        _shader.SetUniformMatrix4("uMVP", viewProjection);
        _shader.SetUniformVector3("uColor", new Vector3(0.7f, 0.7f, 0.7f)); // Gray

        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)(_triangleCount * 3),
                DrawElementsType.UnsignedInt, null);
        }

        // Pass 2: Draw edges (black)
        _shader.SetUniformVector3("uColor", new Vector3(0.0f, 0.0f, 0.0f)); // Black

        _gl.LineWidth(1.5f);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _edgeEbo);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Lines, (uint)(_edgeCount * 2),
                DrawElementsType.UnsignedInt, null);
        }

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// Sets the viewport size.
    /// </summary>
    public void SetViewport(int width, int height)
    {
        _gl.Viewport(0, 0, (uint)width, (uint)height);
    }

    /// <summary>
    /// Loads an embedded resource as a string.
    /// </summary>
    private string LoadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(GemRenderer).Assembly;
        var fullName = $"GemForge.Desktop.Rendering.Shaders.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource not found: {fullName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        _shader?.Dispose();

        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_ebo != 0) _gl.DeleteBuffer(_ebo);
        if (_edgeVbo != 0) _gl.DeleteBuffer(_edgeVbo);
        if (_edgeEbo != 0) _gl.DeleteBuffer(_edgeEbo);
    }
}
