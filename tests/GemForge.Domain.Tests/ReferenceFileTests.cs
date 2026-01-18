using GemForge.Infrastructure.FileFormats;
using GemForge.Domain.Models;
using Xunit;

namespace GemForge.Domain.Tests;

public class ReferenceFileTests
{
    [Fact]
    public void ParseFile_ReferenceAscFile_ParsesCorrectly()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "reference-asc-implemention-srb.asc");
        var fullPath = Path.GetFullPath(samplePath);

        // Act
        var design = AscParser.ParseFile(fullPath);

        // Assert - basic properties
        Assert.Equal(IndexGear.G96, design.IndexGear);
        Assert.Equal(16, design.Symmetry.Fold);
        Assert.False(design.Symmetry.HasMirror); // "y 16 n" means mirror = no
        Assert.Equal(1.54, design.RefractiveIndex);
        Assert.Equal("Standard Brilliant", design.Name.Trim());

        // Assert - facets
        Assert.Equal(7, design.Facets.Count);

        // Check first facet (G1 - girdle)
        var g1 = design.Facets[0];
        Assert.Equal(-90.0, g1.Angle, precision: 5);
        Assert.Equal("G1", g1.TierName);
        Assert.Equal(16, g1.IndexPositions.Length); // 93, 87, 81, 75, 69, 63, 57, 51, 45, 39, 33, 27, 21, 15, 9, 3
        Assert.Contains(93, g1.IndexPositions);
        Assert.Contains(3, g1.IndexPositions);

        // Check table facet (last one)
        var table = design.Facets[6];
        Assert.Equal(0.0, table.Angle);
        Assert.Equal("T", table.TierName);
        Assert.Single(table.IndexPositions);
        Assert.Equal(96, table.IndexPositions[0]);
    }

    [Fact]
    public void WriteAndParse_ReferenceFile_PreservesIndexPositions()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "reference-asc-implemention-srb.asc");
        var fullPath = Path.GetFullPath(samplePath);
        var original = AscParser.ParseFile(fullPath);

        // Act - write and parse back
        var ascContent = AscParser.Write(original);
        var parsed = AscParser.Parse(ascContent);

        // Assert - facet count matches
        Assert.Equal(original.Facets.Count, parsed.Facets.Count);

        // Assert - all index positions preserved
        for (int i = 0; i < original.Facets.Count; i++)
        {
            var originalFacet = original.Facets[i];
            var parsedFacet = parsed.Facets[i];

            Assert.Equal(originalFacet.Angle, parsedFacet.Angle, precision: 5);
            Assert.Equal(originalFacet.Distance, parsedFacet.Distance, precision: 7);
            Assert.Equal(originalFacet.TierName, parsedFacet.TierName);
            Assert.Equal(originalFacet.IndexPositions.Length, parsedFacet.IndexPositions.Length);

            // Verify all indices match
            for (int j = 0; j < originalFacet.IndexPositions.Length; j++)
            {
                Assert.Equal(originalFacet.IndexPositions[j], parsedFacet.IndexPositions[j]);
            }
        }
    }
}
