using GemForge.Infrastructure.FileFormats;
using GemForge.Domain.Models;
using Xunit;

namespace GemForge.Domain.Tests;

public class AscParserIntegrationTests
{
    [Fact]
    public void ParseFile_SimpleTest_ParsesCorrectly()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "simple-test.asc");
        var fullPath = Path.GetFullPath(samplePath);

        // Act
        var design = AscParser.ParseFile(fullPath);

        // Assert
        Assert.Equal("Simple Table Cut", design.Name);
        Assert.Equal("Basic 4-fold test design", design.Author);
        Assert.Equal(IndexGear.G96, design.IndexGear);
        Assert.Equal(4, design.Symmetry.Fold);
        Assert.False(design.Symmetry.HasMirror);
        Assert.Equal(1.54, design.RefractiveIndex);
        Assert.Equal(4, design.Facets.Count); // G1, P1, C1, T
        Assert.Single(design.FooterNotes);
    }

    [Fact]
    public void WriteAndParse_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new GemDesign
        {
            Name = "Test Design",
            Author = "Test Author",
            IndexGear = IndexGear.G96,
            Symmetry = new Symmetry(4, true),
            RefractiveIndex = 1.76,
            Facets = new List<Facet>
            {
                new Facet
                {
                    Angle = 90.0,
                    Distance = 0.0,
                    IndexPositions = new[] { 96 },
                    TierName = "Table"
                },
                new Facet
                {
                    Angle = 40.0,
                    Distance = 0.75,
                    IndexPositions = new[] { 0, 24, 48, 72 },
                    TierName = "Crown"
                }
            }
        };

        original.HeaderNotes.Add(original.Name);
        original.HeaderNotes.Add(original.Author);
        original.FooterNotes.Add("Test footer");

        // Act - write and parse back
        var ascContent = AscParser.Write(original);
        var parsed = AscParser.Parse(ascContent);

        // Assert
        Assert.Equal(original.Name, parsed.Name);
        Assert.Equal(original.Author, parsed.Author);
        Assert.Equal(original.IndexGear, parsed.IndexGear);
        Assert.Equal(original.Symmetry.Fold, parsed.Symmetry.Fold);
        Assert.Equal(original.Symmetry.HasMirror, parsed.Symmetry.HasMirror);
        Assert.Equal(original.RefractiveIndex, parsed.RefractiveIndex);
        Assert.Equal(original.Facets.Count, parsed.Facets.Count);

        // Verify first facet
        Assert.Equal(90.0, parsed.Facets[0].Angle);
        Assert.Equal("Table", parsed.Facets[0].TierName);

        // Verify second facet
        Assert.Equal(40.0, parsed.Facets[1].Angle);
        Assert.Equal("Crown", parsed.Facets[1].TierName);
        Assert.Equal(4, parsed.Facets[1].IndexPositions.Length);
    }

    [Fact]
    public void Parse_StandardRoundBrilliant_ParsesCorrectly()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "standard-round-brilliant.asc");
        var fullPath = Path.GetFullPath(samplePath);

        // Act
        var design = AscParser.ParseFile(fullPath);

        // Assert
        Assert.Equal("Standard Brilliant", design.Name.Trim());
        Assert.Equal(IndexGear.G96, design.IndexGear);
        Assert.Equal(16, design.Symmetry.Fold);
        Assert.False(design.Symmetry.HasMirror);
        Assert.Equal(7, design.Facets.Count); // 7 facet tiers
        Assert.Contains(design.Facets, f => f.TierName == "T"); // Table
        Assert.Contains(design.Facets, f => f.TierName == "G1"); // Girdle
        Assert.Contains(design.Facets, f => f.TierName == "P1"); // Pavilion
    }

    [Fact]
    public void Parse_Portuguese_ParsesCorrectly()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "portuguese.asc");
        var fullPath = Path.GetFullPath(samplePath);

        // Act
        var design = AscParser.ParseFile(fullPath);

        // Assert
        Assert.Equal("Simple 8-Fold Round", design.Name);
        Assert.Equal(IndexGear.G96, design.IndexGear);
        Assert.Equal(8, design.Symmetry.Fold);
        Assert.False(design.Symmetry.HasMirror);
        Assert.Equal(1.76, design.RefractiveIndex); // Sapphire
        Assert.Equal(5, design.Facets.Count); // G1, P1, C1, C2, T

        // Verify pavilion facets
        var pavilionFacets = design.PavilionFacets.ToList();
        Assert.True(pavilionFacets.Count > 0);
        Assert.All(pavilionFacets, f => Assert.True(f.Angle < 0));
    }
}
