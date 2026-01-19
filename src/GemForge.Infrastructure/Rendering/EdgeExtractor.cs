using GemForge.Domain.Geometry;

namespace GemForge.Infrastructure.Rendering;

/// <summary>
/// Extracts visible edges from a mesh for wireframe rendering.
/// </summary>
public static class EdgeExtractor
{
    /// <summary>
    /// Extracts sharp edges from a mesh.
    /// Sharp edges are those shared by non-coplanar faces (facet junctions).
    /// </summary>
    /// <param name="vertices">Mesh vertices</param>
    /// <param name="triangleIndices">Triangle indices (groups of 3)</param>
    /// <param name="angleThreshold">Dihedral angle threshold in degrees (default 10°)</param>
    /// <returns>Array of edge pairs (vertex indices)</returns>
    public static (int, int)[] ExtractEdges(Vector3D[] vertices, int[] triangleIndices, double angleThreshold = 10.0)
    {
        if (vertices.Length == 0 || triangleIndices.Length < 3)
            return Array.Empty<(int, int)>();

        // Build edge-to-faces map
        var edgeToFaces = new Dictionary<Edge, List<int>>();
        var triangleCount = triangleIndices.Length / 3;

        for (int i = 0; i < triangleCount; i++)
        {
            var i0 = triangleIndices[i * 3];
            var i1 = triangleIndices[i * 3 + 1];
            var i2 = triangleIndices[i * 3 + 2];

            // Add the three edges of this triangle
            AddEdge(edgeToFaces, new Edge(i0, i1), i);
            AddEdge(edgeToFaces, new Edge(i1, i2), i);
            AddEdge(edgeToFaces, new Edge(i2, i0), i);
        }

        // Calculate face normals
        var faceNormals = new Vector3D[triangleCount];
        for (int i = 0; i < triangleCount; i++)
        {
            var i0 = triangleIndices[i * 3];
            var i1 = triangleIndices[i * 3 + 1];
            var i2 = triangleIndices[i * 3 + 2];

            var v0 = vertices[i0];
            var v1 = vertices[i1];
            var v2 = vertices[i2];

            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            faceNormals[i] = edge1.Cross(edge2).Normalize();
        }

        // Find sharp edges (edges where adjacent faces have significant angle difference)
        var sharpEdges = new List<(int, int)>();
        var angleThresholdRad = angleThreshold * Math.PI / 180.0;
        // For convex shapes: dihedral angle = 180° - arccos(dot)
        // Sharp edge when dihedral angle < 180° - threshold
        // So: 180° - arccos(dot) < 180° - threshold
        // => arccos(dot) > threshold
        // => dot < cos(threshold)
        var cosThreshold = Math.Cos(angleThresholdRad);

        foreach (var kvp in edgeToFaces)
        {
            var edge = kvp.Key;
            var faces = kvp.Value;

            // Include boundary edges (only one adjacent face)
            if (faces.Count == 1)
            {
                sharpEdges.Add((edge.V0, edge.V1));
                continue;
            }

            // For edges with two adjacent faces, check the dihedral angle
            if (faces.Count == 2)
            {
                var normal1 = faceNormals[faces[0]];
                var normal2 = faceNormals[faces[1]];

                var dot = normal1.Dot(normal2);

                // Sharp edge if normals differ significantly
                // dot < cos(threshold) means the angle between normals > threshold
                if (dot < cosThreshold)
                {
                    sharpEdges.Add((edge.V0, edge.V1));
                }
            }
        }

        return sharpEdges.ToArray();
    }

    private static void AddEdge(Dictionary<Edge, List<int>> edgeToFaces, Edge edge, int faceIndex)
    {
        if (!edgeToFaces.ContainsKey(edge))
        {
            edgeToFaces[edge] = new List<int>();
        }
        edgeToFaces[edge].Add(faceIndex);
    }

    /// <summary>
    /// Represents an undirected edge between two vertices.
    /// </summary>
    private struct Edge : IEquatable<Edge>
    {
        public int V0 { get; }
        public int V1 { get; }

        public Edge(int v0, int v1)
        {
            // Normalize edge direction (smaller index first)
            if (v0 < v1)
            {
                V0 = v0;
                V1 = v1;
            }
            else
            {
                V0 = v1;
                V1 = v0;
            }
        }

        public bool Equals(Edge other)
            => V0 == other.V0 && V1 == other.V1;

        public override bool Equals(object? obj)
            => obj is Edge edge && Equals(edge);

        public override int GetHashCode()
            => HashCode.Combine(V0, V1);
    }
}
