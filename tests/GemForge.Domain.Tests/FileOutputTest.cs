using GemForge.Infrastructure.FileFormats;
using Xunit;
using Xunit.Abstractions;

namespace GemForge.Domain.Tests;

public class FileOutputTest
{
    private readonly ITestOutputHelper _output;

    public FileOutputTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OutputReferenceFileRoundTrip()
    {
        // Arrange
        var samplePath = Path.Combine("..", "..", "..", "..", "..", "samples", "designs", "reference-asc-implemention-srb.asc");
        var fullPath = Path.GetFullPath(samplePath);
        var design = AscParser.ParseFile(fullPath);

        // Act - write back
        var ascContent = AscParser.Write(design);

        // Output for manual inspection
        _output.WriteLine("=== Round-trip output ===");
        _output.WriteLine(ascContent);
        _output.WriteLine("=== End output ===");

        // Also write to a file for manual testing
        var outputPath = Path.Combine("..", "..", "..", "..", "..", "test-output", "roundtrip-output.asc");
        var outputFullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputFullPath)!);
        File.WriteAllText(outputFullPath, ascContent);

        _output.WriteLine($"Written to: {outputFullPath}");

        // Simple assertion
        Assert.NotEmpty(ascContent);
    }
}
