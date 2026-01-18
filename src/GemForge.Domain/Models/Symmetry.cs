namespace GemForge.Domain.Models;

/// <summary>
/// Defines the rotational symmetry of a gem design.
/// </summary>
/// <param name="Fold">Number of rotational symmetry sections (e.g., 4 for 4-fold symmetry)</param>
/// <param name="HasMirror">Whether the design has mirror symmetry within each fold</param>
public record Symmetry(int Fold, bool HasMirror)
{
    /// <summary>
    /// Gets the angle of each symmetry section in degrees.
    /// </summary>
    public double SectionAngle => 360.0 / Fold;

    /// <summary>
    /// Gets the total number of symmetric positions.
    /// </summary>
    public int SymmetricPositions => HasMirror ? Fold * 2 : Fold;
}
