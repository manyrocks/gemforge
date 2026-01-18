namespace GemForge.Domain.Models;

/// <summary>
/// Represents a tier (group) of facets that are cut together in sequence.
/// </summary>
public class Tier
{
    /// <summary>
    /// Name of this tier (e.g., "Table", "Crown Main", "Pavilion").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of facets in this tier.
    /// </summary>
    public List<Facet> Facets { get; set; } = new();

    /// <summary>
    /// Cutting order/sequence number (lower numbers are cut first).
    /// </summary>
    public int CuttingOrder { get; set; }

    /// <summary>
    /// Gets whether this tier contains pavilion facets.
    /// </summary>
    public bool IsPavilion => Facets.Any(f => f.IsPavilion);

    /// <summary>
    /// Gets whether this tier contains crown facets.
    /// </summary>
    public bool IsCrown => Facets.Any(f => f.IsCrown);
}
