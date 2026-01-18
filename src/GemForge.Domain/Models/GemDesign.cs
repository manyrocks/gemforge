namespace GemForge.Domain.Models;

/// <summary>
/// Represents a complete gem faceting design.
/// </summary>
public class GemDesign
{
    /// <summary>
    /// Name of the design (e.g., "Standard Round Brilliant").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Author or designer name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Index gear used for this design.
    /// </summary>
    public IndexGear IndexGear { get; set; } = IndexGear.G96;

    /// <summary>
    /// Rotational symmetry configuration.
    /// </summary>
    public Symmetry Symmetry { get; set; } = new(4, true);

    /// <summary>
    /// Refractive index of the material (e.g., 1.54 for quartz, 2.417 for diamond).
    /// </summary>
    public double RefractiveIndex { get; set; } = 1.54;

    /// <summary>
    /// Tiers (groups of facets) in cutting order.
    /// </summary>
    public List<Tier> Tiers { get; set; } = new();

    /// <summary>
    /// All facets in the design (flattened from all tiers).
    /// </summary>
    public List<Facet> Facets { get; set; } = new();

    /// <summary>
    /// Header notes/comments from the design file.
    /// </summary>
    public List<string> HeaderNotes { get; set; } = new();

    /// <summary>
    /// Footer notes/comments from the design file.
    /// </summary>
    public List<string> FooterNotes { get; set; } = new();

    /// <summary>
    /// Gets all crown facets.
    /// </summary>
    public IEnumerable<Facet> CrownFacets => Facets.Where(f => f.IsCrown);

    /// <summary>
    /// Gets all pavilion facets.
    /// </summary>
    public IEnumerable<Facet> PavilionFacets => Facets.Where(f => f.IsPavilion);
}
