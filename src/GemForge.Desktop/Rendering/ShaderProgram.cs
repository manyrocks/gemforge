using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GemForge.Desktop.Rendering;

/// <summary>
/// Manages OpenGL shader programs.
/// </summary>
public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private uint _programId;
    private readonly Dictionary<string, int> _uniformLocations = new();

    public ShaderProgram(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;
        _programId = CreateProgram(vertexSource, fragmentSource);
    }

    private uint CreateProgram(string vertexSource, string fragmentSource)
    {
        // Compile vertex shader
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);

        // Compile fragment shader
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

        // Link program
        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        // Check for linking errors
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetProgramInfoLog(program);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }

        // Clean up shaders (they're now linked into the program)
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        // Check for compilation errors
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation failed ({type}): {infoLog}");
        }

        return shader;
    }

    /// <summary>
    /// Activates this shader program for use.
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(_programId);
    }

    /// <summary>
    /// Gets the location of a uniform variable.
    /// </summary>
    private int GetUniformLocation(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int location))
        {
            location = _gl.GetUniformLocation(_programId, name);
            _uniformLocations[name] = location;
        }
        return location;
    }

    /// <summary>
    /// Sets a mat4 uniform.
    /// </summary>
    public unsafe void SetUniformMatrix4(string name, Matrix4x4 matrix)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            // OpenGL expects column-major matrices, which Matrix4x4 is by default
            _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
    }

    /// <summary>
    /// Sets a vec3 uniform.
    /// </summary>
    public void SetUniformVector3(string name, Vector3 vector)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform3(location, vector.X, vector.Y, vector.Z);
        }
    }

    /// <summary>
    /// Sets a float uniform.
    /// </summary>
    public void SetUniformFloat(string name, float value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    /// <summary>
    /// Sets an int uniform.
    /// </summary>
    public void SetUniformInt(string name, int value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    public void Dispose()
    {
        if (_programId != 0)
        {
            _gl.DeleteProgram(_programId);
            _programId = 0;
        }
    }
}
