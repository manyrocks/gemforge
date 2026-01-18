using GemForge.Domain.Geometry;
using GemForge.Domain.Models;
using Xunit;

namespace GemForge.Domain.Tests;

public class IndexCalculatorTests
{
    [Theory]
    [InlineData(IndexGear.G64, 5.625)]
    [InlineData(IndexGear.G72, 5.0)]
    [InlineData(IndexGear.G80, 4.5)]
    [InlineData(IndexGear.G96, 3.75)]
    [InlineData(IndexGear.G120, 3.0)]
    public void DegreesPerIndex_ReturnsCorrectValue_ForEachGear(IndexGear gear, double expectedDegrees)
    {
        // Act
        var result = IndexCalculator.DegreesPerIndex(gear);

        // Assert
        Assert.Equal(expectedDegrees, result, precision: 6);
    }

    [Theory]
    [InlineData(0, IndexGear.G96, 0)]
    [InlineData(3.75, IndexGear.G96, 1)]
    [InlineData(7.5, IndexGear.G96, 2)]
    [InlineData(90, IndexGear.G96, 24)]
    [InlineData(180, IndexGear.G96, 48)]
    [InlineData(270, IndexGear.G96, 72)]
    [InlineData(360, IndexGear.G96, 0)]
    public void AngleToIndex_ConvertsCorrectly_For96Gear(double angle, IndexGear gear, int expectedIndex)
    {
        // Act
        var result = IndexCalculator.AngleToIndex(angle, gear);

        // Assert
        Assert.Equal(expectedIndex, result);
    }

    [Theory]
    [InlineData(0, IndexGear.G72, 0)]
    [InlineData(5.0, IndexGear.G72, 1)]
    [InlineData(90, IndexGear.G72, 18)]
    [InlineData(180, IndexGear.G72, 36)]
    public void AngleToIndex_ConvertsCorrectly_For72Gear(double angle, IndexGear gear, int expectedIndex)
    {
        // Act
        var result = IndexCalculator.AngleToIndex(angle, gear);

        // Assert
        Assert.Equal(expectedIndex, result);
    }

    [Fact]
    public void AngleToIndex_HandlesNegativeAngles()
    {
        // Arrange - negative angles should wrap around
        var angle = -3.75; // Should be equivalent to 356.25° on G96
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.AngleToIndex(angle, gear);

        // Assert
        Assert.Equal(95, result); // 356.25° / 3.75° = 95
    }

    [Fact]
    public void AngleToIndex_HandlesAnglesOver360()
    {
        // Arrange
        var angle = 363.75; // Should wrap to 3.75°
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.AngleToIndex(angle, gear);

        // Assert
        Assert.Equal(1, result); // 3.75° / 3.75° = 1
    }

    [Theory]
    [InlineData(0, IndexGear.G96, 0)]
    [InlineData(1, IndexGear.G96, 3.75)]
    [InlineData(24, IndexGear.G96, 90)]
    [InlineData(48, IndexGear.G96, 180)]
    [InlineData(72, IndexGear.G96, 270)]
    [InlineData(96, IndexGear.G96, 360)]
    public void IndexToAngle_ConvertsCorrectly_For96Gear(int index, IndexGear gear, double expectedAngle)
    {
        // Act
        var result = IndexCalculator.IndexToAngle(index, gear);

        // Assert
        Assert.Equal(expectedAngle, result, precision: 6);
    }

    [Fact]
    public void GetNearestIndices_ReturnsCorrectValues_ForExactMatch()
    {
        // Arrange - exact angle match
        var angle = 90.0;
        var gear = IndexGear.G96;

        // Act
        var (closest, lower, upper) = IndexCalculator.GetNearestIndices(angle, gear);

        // Assert
        Assert.Equal(24, closest);
        Assert.Equal(24, lower);
        Assert.Equal(24, upper);
    }

    [Fact]
    public void GetNearestIndices_ReturnsCorrectValues_BetweenIndices()
    {
        // Arrange - angle between index positions
        var angle = 5.0; // Between index 1 (3.75°) and index 2 (7.5°)
        var gear = IndexGear.G96;

        // Act
        var (closest, lower, upper) = IndexCalculator.GetNearestIndices(angle, gear);

        // Assert
        Assert.Equal(1, closest); // 5.0 is closer to 3.75 (index 1) than 7.5 (index 2)
        Assert.Equal(1, lower);
        Assert.Equal(2, upper);
    }

    [Theory]
    [InlineData(0, 24, IndexGear.G96, 90.0)]
    [InlineData(0, 48, IndexGear.G96, 180.0)]
    [InlineData(24, 48, IndexGear.G96, 90.0)]
    [InlineData(0, 72, IndexGear.G96, 90.0)] // Should take shortest path: 270° vs 90°
    public void IndexDifference_ReturnsShortestAngularDistance(int index1, int index2, IndexGear gear, double expectedDifference)
    {
        // Act
        var result = IndexCalculator.IndexDifference(index1, index2, gear);

        // Assert
        Assert.Equal(expectedDifference, result, precision: 6);
    }

    [Fact]
    public void GetSymmetricIndices_4FoldWithMirror_ReturnsCorrectPositions()
    {
        // Arrange - 4-fold symmetry with mirror (8 positions total)
        var baseIndex = 0;
        var symmetry = new Symmetry(4, true);
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.GetSymmetricIndices(baseIndex, symmetry, gear);

        // Assert
        // 96 / 4 = 24 index step, mirror adds +12 offset
        // Expected: [0, 12, 24, 36, 48, 60, 72, 84]
        Assert.Equal(8, result.Length);
        Assert.Contains(0, result);
        Assert.Contains(12, result);
        Assert.Contains(24, result);
        Assert.Contains(36, result);
        Assert.Contains(48, result);
        Assert.Contains(60, result);
        Assert.Contains(72, result);
        Assert.Contains(84, result);
    }

    [Fact]
    public void GetSymmetricIndices_4FoldNoMirror_ReturnsCorrectPositions()
    {
        // Arrange - 4-fold symmetry without mirror (4 positions)
        var baseIndex = 0;
        var symmetry = new Symmetry(4, false);
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.GetSymmetricIndices(baseIndex, symmetry, gear);

        // Assert
        // Expected: [0, 24, 48, 72]
        Assert.Equal(4, result.Length);
        Assert.Contains(0, result);
        Assert.Contains(24, result);
        Assert.Contains(48, result);
        Assert.Contains(72, result);
    }

    [Fact]
    public void GetSymmetricIndices_6FoldWithMirror_ReturnsCorrectPositions()
    {
        // Arrange - 6-fold symmetry with mirror
        var baseIndex = 0;
        var symmetry = new Symmetry(6, true);
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.GetSymmetricIndices(baseIndex, symmetry, gear);

        // Assert
        // 96 / 6 = 16 index step, mirror adds +8 offset
        // Expected 12 positions
        Assert.Equal(12, result.Length);
    }

    [Fact]
    public void GetSymmetricIndices_ReturnsOrderedIndices()
    {
        // Arrange
        var baseIndex = 5;
        var symmetry = new Symmetry(4, true);
        var gear = IndexGear.G96;

        // Act
        var result = IndexCalculator.GetSymmetricIndices(baseIndex, symmetry, gear);

        // Assert - indices should be in ascending order
        for (int i = 0; i < result.Length - 1; i++)
        {
            Assert.True(result[i] < result[i + 1], "Indices should be in ascending order");
        }
    }

    [Fact]
    public void Symmetry_SectionAngle_CalculatesCorrectly()
    {
        // Arrange
        var symmetry = new Symmetry(4, true);

        // Act
        var sectionAngle = symmetry.SectionAngle;

        // Assert
        Assert.Equal(90.0, sectionAngle);
    }

    [Theory]
    [InlineData(4, true, 8)]
    [InlineData(4, false, 4)]
    [InlineData(6, true, 12)]
    [InlineData(6, false, 6)]
    public void Symmetry_SymmetricPositions_CalculatesCorrectly(int fold, bool hasMirror, int expectedPositions)
    {
        // Arrange
        var symmetry = new Symmetry(fold, hasMirror);

        // Act
        var positions = symmetry.SymmetricPositions;

        // Assert
        Assert.Equal(expectedPositions, positions);
    }
}
