namespace GemForge.Domain.Models;

/// <summary>
/// Represents a single facet or group of symmetric facets in a gem design.
/// </summary>
public class Facet
{
    /// <summary>
    /// Cutting angle in degrees from horizontal plane.
    /// Positive = crown (above girdle), Negative = pavilion (below girdle).
    /// </summary>
    public double Angle { get; set; }

    /// <summary>
    /// Distance from center of stone (normalized, typically 0.0 to 1.0+).
    /// The girdle is typically around 1.0.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Index positions where this facet appears (accounting for symmetry).
    /// For example, [0, 24, 48, 72] on a 96-index gear with 4-fold symmetry.
    /// </summary>
    public int[] IndexPositions { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Name of the tier this facet belongs to (e.g., "Table", "Crown Main", "Pavilion").
    /// </summary>
    public string TierName { get; set; } = string.Empty;

    /// <summary>
    /// Gets whether this facet is on the pavilion (below the girdle).
    /// </summary>
    public bool IsPavilion => Angle < 0;

    /// <summary>
    /// Gets whether this facet is on the crown (above the girdle).
    /// </summary>
    public bool IsCrown => Angle > 0;

    /// <summary>
    /// Gets whether this facet is horizontal (table or culet).
    /// </summary>
    public bool IsHorizontal => Math.Abs(Angle - 90.0) < 0.001 || Math.Abs(Angle + 90.0) < 0.001;
}
