using GemForge.Domain.Models;

namespace GemForge.Domain.Geometry;

/// <summary>
/// Provides calculations for converting between angles and index positions
/// on faceting machine index gears.
/// </summary>
public static class IndexCalculator
{
    /// <summary>
    /// Calculates the degrees per index position for a given gear.
    /// </summary>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Degrees per index position (e.g., 3.75° for 96-index gear)</returns>
    public static double DegreesPerIndex(IndexGear gear)
    {
        return 360.0 / (int)gear;
    }

    /// <summary>
    /// Converts an angle in degrees to the nearest index position on the gear.
    /// </summary>
    /// <param name="angle">Angle in degrees (0-360)</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Index position (0 to gear-1)</returns>
    public static int AngleToIndex(double angle, IndexGear gear)
    {
        var degreesPerIndex = DegreesPerIndex(gear);
        var normalized = ((angle % 360) + 360) % 360;
        return (int)Math.Round(normalized / degreesPerIndex) % (int)gear;
    }

    /// <summary>
    /// Converts an index position to the exact angle in degrees.
    /// </summary>
    /// <param name="index">Index position (0 to gear-1)</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Angle in degrees</returns>
    public static double IndexToAngle(int index, IndexGear gear)
    {
        return index * DegreesPerIndex(gear);
    }

    /// <summary>
    /// Finds the nearest index positions for a given angle.
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Tuple of (closest index, lower bound index, upper bound index)</returns>
    public static (int closest, int lower, int upper) GetNearestIndices(double angle, IndexGear gear)
    {
        var degreesPerIndex = DegreesPerIndex(gear);
        var normalized = ((angle % 360) + 360) % 360;
        var exactIndex = normalized / degreesPerIndex;
        var gearSize = (int)gear;

        return (
            (int)Math.Round(exactIndex) % gearSize,
            (int)Math.Floor(exactIndex) % gearSize,
            (int)Math.Ceiling(exactIndex) % gearSize
        );
    }

    /// <summary>
    /// Calculates the angular difference between two index positions.
    /// </summary>
    /// <param name="index1">First index position</param>
    /// <param name="index2">Second index position</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Angular difference in degrees</returns>
    public static double IndexDifference(int index1, int index2, IndexGear gear)
    {
        var angle1 = IndexToAngle(index1, gear);
        var angle2 = IndexToAngle(index2, gear);
        var diff = Math.Abs(angle2 - angle1);
        return Math.Min(diff, 360 - diff); // Return shortest angular distance
    }

    /// <summary>
    /// Generates all symmetric index positions for a given base index and symmetry.
    /// </summary>
    /// <param name="baseIndex">Starting index position</param>
    /// <param name="symmetry">Symmetry configuration</param>
    /// <param name="gear">The index gear configuration</param>
    /// <returns>Array of all symmetric index positions</returns>
    public static int[] GetSymmetricIndices(int baseIndex, Symmetry symmetry, IndexGear gear)
    {
        var gearSize = (int)gear;
        var indexStep = gearSize / symmetry.Fold;
        var indices = new List<int>();

        for (int i = 0; i < symmetry.Fold; i++)
        {
            var index = (baseIndex + i * indexStep) % gearSize;
            indices.Add(index);

            if (symmetry.HasMirror)
            {
                var mirrorOffset = indexStep / 2;
                var mirrorIndex = (baseIndex + i * indexStep + mirrorOffset) % gearSize;
                if (!indices.Contains(mirrorIndex))
                {
                    indices.Add(mirrorIndex);
                }
            }
        }

        return indices.OrderBy(i => i).ToArray();
    }
}
