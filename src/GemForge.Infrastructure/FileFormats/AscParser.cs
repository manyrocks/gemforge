using GemForge.Domain.Models;
using System.Globalization;

namespace GemForge.Infrastructure.FileFormats;

/// <summary>
/// Parser for GemCad .asc (ASCII) file format.
/// </summary>
public class AscParser
{
    /// <summary>
    /// Parses a .asc file from the given file path.
    /// </summary>
    /// <param name="filePath">Path to the .asc file</param>
    /// <returns>Parsed GemDesign object</returns>
    public static GemDesign ParseFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    /// <summary>
    /// Parses a .asc file content from a string.
    /// </summary>
    /// <param name="content">Content of the .asc file</param>
    /// <returns>Parsed GemDesign object</returns>
    public static GemDesign Parse(string content)
    {
        var design = new GemDesign();
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var tierMap = new Dictionary<string, Tier>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            // Check if first part is a number (gear line without 'g' prefix)
            if (int.TryParse(parts[0], out var gearValueDirect))
            {
                // <index_gear> <rotation_offset>
                design.IndexGear = ParseIndexGear(gearValueDirect);
                continue;
            }

            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "gemcad":
                    // Version header - just skip
                    break;

                case "g":
                    // g <index_gear> <rotation_offset> (alternate format)
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var gearValue))
                    {
                        design.IndexGear = ParseIndexGear(gearValue);
                    }
                    break;

                case "y":
                case "n":
                    // <y/n> <fold> <y/n>
                    // First y/n is whether symmetry is enabled, second y/n is whether mirror is enabled
                    if (parts.Length >= 3)
                    {
                        var symmetryEnabled = command == "y";
                        if (symmetryEnabled && int.TryParse(parts[1], out var fold))
                        {
                            var hasMirror = parts[2].ToLowerInvariant() == "y";
                            design.Symmetry = new Symmetry(fold, hasMirror);
                        }
                        else
                        {
                            design.Symmetry = new Symmetry(1, false); // No symmetry
                        }
                    }
                    break;

                case "i":
                    // I <refractive_index>
                    if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var ri))
                    {
                        design.RefractiveIndex = ri;
                    }
                    break;

                case "h":
                    // H <header_line>
                    var headerText = string.Join(" ", parts.Skip(1));
                    design.HeaderNotes.Add(headerText);

                    // Try to extract name and author from first header lines
                    if (design.HeaderNotes.Count == 1 && string.IsNullOrEmpty(design.Name))
                    {
                        design.Name = headerText;
                    }
                    else if (design.HeaderNotes.Count == 2 && string.IsNullOrEmpty(design.Author))
                    {
                        design.Author = headerText;
                    }
                    break;

                case "f":
                    // F <footer_line>
                    var footerText = string.Join(" ", parts.Skip(1));
                    design.FooterNotes.Add(footerText);
                    break;

                case "a":
                    // a <angle> <distance> <indices...> n <tier_name>
                    ParseFacetLine(parts, design, tierMap);
                    break;

                case "c":
                    // C <cutting_sequence> - skip for now
                    break;

                default:
                    // Unknown command - skip
                    break;
            }
        }

        // Build tiers list from map and sort by cutting order
        design.Tiers = tierMap.Values.OrderBy(t => t.CuttingOrder).ToList();

        return design;
    }

    /// <summary>
    /// Writes a GemDesign to a .asc file.
    /// </summary>
    /// <param name="design">The gem design to write</param>
    /// <param name="filePath">Output file path</param>
    public static void WriteFile(GemDesign design, string filePath)
    {
        var content = Write(design);
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Writes a GemDesign to a .asc format string.
    /// </summary>
    /// <param name="design">The gem design to write</param>
    /// <returns>The .asc file content as a string</returns>
    public static string Write(GemDesign design)
    {
        var lines = new List<string>();

        // Version header
        lines.Add("GemCad 5.0");

        // Index gear and rotation (no 'g' prefix)
        lines.Add($"{(int)design.IndexGear} 0.0");

        // Symmetry
        var symmetryEnabled = design.Symmetry.Fold > 1 ? "y" : "n";
        var mirrorEnabled = design.Symmetry.HasMirror ? "y" : "n";
        lines.Add($"{symmetryEnabled} {design.Symmetry.Fold} {mirrorEnabled}");

        // Refractive index
        lines.Add($"I {design.RefractiveIndex.ToString("F2", CultureInfo.InvariantCulture)}");

        // Header notes
        foreach (var header in design.HeaderNotes)
        {
            lines.Add($"H {header}");
        }

        // If no header notes, add name and author
        if (design.HeaderNotes.Count == 0)
        {
            if (!string.IsNullOrEmpty(design.Name))
                lines.Add($"H {design.Name}");
            if (!string.IsNullOrEmpty(design.Author))
                lines.Add($"H {design.Author}");
        }

        // Facets
        // Format: a <angle> <distance> <first_index> n <tier_name> [additional_indices] G
        foreach (var facet in design.Facets)
        {
            if (facet.IndexPositions.Length == 0)
                continue;

            var angleStr = facet.Angle.ToString("F6", CultureInfo.InvariantCulture);
            var distanceStr = facet.Distance.ToString("F8", CultureInfo.InvariantCulture);
            var firstIndex = facet.IndexPositions[0];
            var additionalIndices = facet.IndexPositions.Length > 1
                ? " " + string.Join(" ", facet.IndexPositions.Skip(1))
                : "";

            lines.Add($"a {angleStr} {distanceStr} {firstIndex} n {facet.TierName}{additionalIndices} G");
        }

        // Footer notes
        foreach (var footer in design.FooterNotes)
        {
            lines.Add($"F {footer}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void ParseFacetLine(string[] parts, GemDesign design, Dictionary<string, Tier> tierMap)
    {
        // Format: a <angle> <distance> <first_index> n <tier_name> [additional_indices...] G <comment>
        if (parts.Length < 6)
            return;

        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var angle))
            return;

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var distance))
            return;

        // Parse first index (parts[3])
        if (!int.TryParse(parts[3], out var firstIndex))
            return;

        // parts[4] should be 'n'
        if (parts[4].ToLowerInvariant() != "n")
            return;

        // parts[5] is the tier name
        var tierName = parts[5];

        // Find 'G' marker for comment (optional)
        var gIndex = Array.IndexOf(parts, "G", 6);
        if (gIndex == -1)
        {
            gIndex = Array.IndexOf(parts, "g", 6);
        }

        // Parse additional indices between tier name and 'G' (or end of line)
        var indices = new List<int> { firstIndex };
        var endIndex = gIndex != -1 ? gIndex : parts.Length;

        for (int i = 6; i < endIndex; i++)
        {
            if (int.TryParse(parts[i], out var index))
            {
                indices.Add(index);
            }
        }

        // Create facet
        var facet = new Facet
        {
            Angle = angle,
            Distance = distance,
            IndexPositions = indices.ToArray(),
            TierName = tierName
        };

        design.Facets.Add(facet);

        // Add to tier
        if (!tierMap.TryGetValue(tierName, out var tier))
        {
            tier = new Tier
            {
                Name = tierName,
                CuttingOrder = tierMap.Count
            };
            tierMap[tierName] = tier;
        }

        tier.Facets.Add(facet);
    }

    private static IndexGear ParseIndexGear(int value)
    {
        return value switch
        {
            64 => IndexGear.G64,
            72 => IndexGear.G72,
            80 => IndexGear.G80,
            96 => IndexGear.G96,
            120 => IndexGear.G120,
            _ => IndexGear.G96 // Default to 96
        };
    }
}
