using GemForge.Domain.Models;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Generates 3D meshes from gem designs.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Generates a triangulated mesh from a gem design.
    /// Uses CSG (Constructive Solid Geometry) to accurately represent facet planes.
    /// </summary>
    /// <param name="design">The gem design to convert to mesh</param>
    /// <returns>A triangulated mesh with vertices, triangles, and edges</returns>
    public static Mesh GenerateMesh(GemDesign design)
    {
        // Use CSG (Constructive Solid Geometry) approach for accurate facet representation
        return CsgMeshGenerator.GenerateMesh(design);
    }
}
