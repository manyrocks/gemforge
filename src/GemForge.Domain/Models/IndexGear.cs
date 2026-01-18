namespace GemForge.Domain.Models;

/// <summary>
/// Index gear settings for faceting machines.
/// The value represents the number of index positions in a full 360° rotation.
/// </summary>
public enum IndexGear
{
    /// <summary>64-index gear (5.625° per index)</summary>
    G64 = 64,

    /// <summary>72-index gear (5.0° per index)</summary>
    G72 = 72,

    /// <summary>80-index gear (4.5° per index)</summary>
    G80 = 80,

    /// <summary>96-index gear (3.75° per index)</summary>
    G96 = 96,

    /// <summary>120-index gear (3.0° per index)</summary>
    G120 = 120
}
