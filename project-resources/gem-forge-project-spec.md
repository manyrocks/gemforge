# GemForge Project Specification

## Overview

A modern, cross-platform gem faceting design application. Think "GemCad but modern" with accurate 3D rendering, proper optical simulation, and clean architecture that allows future extensibility.

**Target platforms:** Windows, macOS, Linux (via Avalonia)

---

## Technology Stack

### Core
- **.NET 8** (LTS)
- **C# 12**

### UI
- **Avalonia UI** - Cross-platform WPF-like framework, MIT license
- Uses `OpenGLControlBase` for 3D viewport integration

### 3D/Graphics
- **Silk.NET** - OpenGL/Vulkan bindings for .NET
- **geometry3Sharp** - Pure C# geometry library (meshes, intersections, spatial queries)
- **GeometRi** - Lightweight 3D primitives (planes, lines, spheres)

### Why These Choices
- **Avalonia over Electron:** Native performance, smaller footprint, proper GPU access
- **Silk.NET over Unity:** No game engine overhead, direct GPU control, CAD-appropriate
- **geometry3Sharp:** Boost license (commercial-friendly), battle-tested, no dependencies

---

## Architecture

Clean/onion architecture with fully portable domain layer.

```
Presentation (Avalonia)
       ↓
Application (Commands/Queries/Services)
       ↓
Domain (Models, Geometry Engine, Optics) ← Pure C#, zero dependencies
       ↓
Infrastructure (File I/O, Rendering, Export)
```

### Domain Layer (GemForge.Domain)

**Core Models:**
```csharp
public class GemDesign
{
    public string Name { get; set; }
    public string Author { get; set; }
    public IndexGear IndexGear { get; set; }  // 64, 72, 80, 96, 120
    public Symmetry Symmetry { get; set; }
    public double RefractiveIndex { get; set; }
    public List<Tier> Tiers { get; set; }
    public List<Facet> Facets { get; set; }
}

public class Facet
{
    public double Angle { get; set; }        // Degrees from horizontal (negative = pavilion)
    public double Distance { get; set; }      // Distance from center (normalized)
    public int[] IndexPositions { get; set; } // e.g., [96, 24, 48, 72]
    public string TierName { get; set; }
    public bool IsPavilion => Angle < 0;
}

public class Tier
{
    public string Name { get; set; }
    public List<Facet> Facets { get; set; }
    public int CuttingOrder { get; set; }
}

public enum IndexGear { G64 = 64, G72 = 72, G80 = 80, G96 = 96, G120 = 120 }

public record Symmetry(int Fold, bool HasMirror);  // e.g., (4, true) = 4-fold with mirror
```

**Geometry Engine:**
```csharp
public static class IndexCalculator
{
    public static double DegreesPerIndex(IndexGear gear) => 360.0 / (int)gear;
    
    public static int AngleToIndex(double angle, IndexGear gear)
    {
        var degreesPerIndex = DegreesPerIndex(gear);
        var normalized = ((angle % 360) + 360) % 360;
        return (int)Math.Round(normalized / degreesPerIndex) % (int)gear;
    }
    
    public static double IndexToAngle(int index, IndexGear gear)
    {
        return index * DegreesPerIndex(gear);
    }
    
    public static (int closest, int lower, int upper) GetNearestIndices(double angle, IndexGear gear)
    {
        var degreesPerIndex = DegreesPerIndex(gear);
        var exactIndex = angle / degreesPerIndex;
        return (
            (int)Math.Round(exactIndex) % (int)gear,
            (int)Math.Floor(exactIndex) % (int)gear,
            (int)Math.Ceiling(exactIndex) % (int)gear
        );
    }
}
```

### Application Layer (GemForge.Application)

Commands/queries for use cases:
- `LoadDesignCommand` - Load from file
- `SaveDesignCommand` - Save to file
- `AddFacetCommand` - Add facet with undo support
- `CalculateMeetpointsQuery` - Compute facet intersections
- `CalculateWeightQuery` - Estimate carat weight from mesh + specific gravity
- `ExportDiagramCommand` - Generate PDF/SVG diagram

### Infrastructure Layer (GemForge.Infrastructure)

**File Parsers:**
- `AscParser` - GemCad .asc text format
- `GemParser` - GemCad .gem binary format (if needed)
- `GcsParser` - Gem Cut Studio format (if needed)

**Rendering:**
- `OpenGLRenderer` - 3D viewport rendering
- `MeshGenerator` - Convert facet planes to triangle mesh
- `DiagramRenderer` - 2D plan/profile views

**Export:**
- `PdfExporter` - Faceting diagrams
- `SvgExporter` - Vector diagrams
- `StlExporter` - 3D printing (future)

---

## .ASC File Format

Text-based, line-by-line:

```
GemCad 5.0                                    # Version header
g 96 0.0                                      # g <index_gear> <rotation_offset>
y 4 y                                         # <symmetry_enabled> <fold> <mirror_enabled>
I 1.54                                        # I <refractive_index>
H Design Name Here                            # H <header_line>
H Author Name, Date
H License/notes
a 90.000000 1.09380454 96 24 48 72 n g1       # a <angle> <distance> <indices...> n <tier_name>
a -38.000000 0.87556750 96 72 48 24 n 1       # Negative angle = pavilion
a 20.000000 0.23666377 96 24 48 72 n a
F Footer note                                 # F <footer_line>
```

**Parsing notes:**
- Lines starting with `a` are facets
- Angle is in degrees; negative = pavilion (below girdle)
- Distance is normalized (girdle typically ~1.0)
- Index positions list all symmetric positions
- Tier name follows `n` marker

---

## Development Phases

### Phase 1: Foundation (4-6 weeks)
- [ ] Solution structure
- [ ] Domain models (GemDesign, Facet, Tier, IndexGear, Symmetry)
- [ ] IndexCalculator
- [ ] .asc parser (read and write)
- [ ] Basic console app to test loading files
- [ ] Unit tests for geometry calculations

### Phase 2: 2D Visualization (4-6 weeks)
- [ ] Avalonia desktop app shell
- [ ] Plan view (top-down 2D)
- [ ] Profile view (side 2D)
- [ ] Facet diagram with angles/indices
- [ ] Basic editing (select, modify, add/delete facets)
- [ ] Undo/redo

### Phase 3: 3D Preview (6-8 weeks)
- [ ] Mesh generation from facet planes
- [ ] OpenGL viewport (Silk.NET + Avalonia OpenGLControlBase)
- [ ] Wireframe, solid, transparent modes
- [ ] Interactive orbit/pan/zoom
- [ ] Material colors

### Phase 4: Optical Simulation (8-12 weeks)
- [ ] Ray tracer with refraction (Snell's law)
- [ ] Total internal reflection
- [ ] Critical angle visualization
- [ ] Brilliance/light return metrics
- [ ] Material library (RI, dispersion, SG)

### Phase 5: Advanced Features (ongoing)
- [ ] Weight/yield calculator
- [ ] Design comparison tools
- [ ] Cutting sequence optimization
- [ ] PDF diagram export
- [ ] Improved lighting models (environment maps, spectral dispersion)
- [ ] Rough stone fitting (3D scanner integration?)

### Future Consideration: Concave Faceting (OMF)

**Not in scope for v1-4, but keep architecture flexible.**

Concave faceting uses cylindrical or spherical laps to create curved facets (grooves, scallops, lenses). This is significantly more complex:

- Surfaces are curves, not planes
- Intersections become conic sections
- Mesh generation requires tessellation
- Ray tracing through curved internal surfaces is different math

**Architectural note:** The current `Facet` model assumes flat planes. If concave support is ever added, consider abstracting to an `ISurface` interface:

```csharp
public interface ISurface
{
    Mesh3 Tessellate(int resolution);
    Vector3? IntersectRay(Ray3 ray);
}

public class PlaneSurface : ISurface { }       // Traditional flat facet
public class CylindricalSurface : ISurface { } // Concave (future)
public class SphericalSurface : ISurface { }   // Concave (future)
```

For now, build with flat facets but keep rendering/optical code decoupled from surface geometry so this could be added later without a full rewrite.

---

## Key Formulas

### Index Calculation
```
DegreesPerIndex = 360 / IndexGear
Index = Angle / DegreesPerIndex (rounded)
Angle = Index × DegreesPerIndex
```

### Common Index Gears
| Gear | Degrees/Index |
|------|---------------|
| 64   | 5.625°        |
| 72   | 5.0°          |
| 80   | 4.5°          |
| 96   | 3.75°         |
| 120  | 3.0°          |

### Optics
```
Snell's Law: n₁ sin(θ₁) = n₂ sin(θ₂)
Critical Angle: θc = arcsin(n₂/n₁)
```

### Weight Calculation
```
Volume (mm³) × Specific Gravity / 200 = Carats
```

---

## Project Structure

```
GemForge/
├── src/
│   ├── GemForge.Domain/
│   │   ├── Models/
│   │   │   ├── GemDesign.cs
│   │   │   ├── Facet.cs
│   │   │   ├── Tier.cs
│   │   │   ├── IndexGear.cs
│   │   │   └── Symmetry.cs
│   │   ├── Geometry/
│   │   │   ├── IndexCalculator.cs
│   │   │   ├── MeetpointSolver.cs
│   │   │   ├── PlaneIntersection.cs
│   │   │   └── SymmetryEngine.cs
│   │   └── Optics/
│   │       ├── RayTracer.cs
│   │       ├── Material.cs
│   │       └── MaterialLibrary.cs
│   │
│   ├── GemForge.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Services/
│   │
│   ├── GemForge.Infrastructure/
│   │   ├── FileFormats/
│   │   │   └── AscParser.cs
│   │   ├── Rendering/
│   │   │   ├── OpenGLRenderer.cs
│   │   │   └── MeshGenerator.cs
│   │   └── Export/
│   │       └── PdfExporter.cs
│   │
│   └── GemForge.Desktop/
│       ├── App.axaml
│       ├── Views/
│       │   └── MainWindow.axaml
│       ├── ViewModels/
│       │   └── MainWindowViewModel.cs
│       └── Controls/
│           └── GemViewport.cs
│
├── tests/
│   ├── GemForge.Domain.Tests/
│   │   ├── IndexCalculatorTests.cs
│   │   └── MeetpointSolverTests.cs
│   └── GemForge.Infrastructure.Tests/
│       └── AscParserTests.cs
│
└── samples/
    └── designs/
        └── (download .asc files from facetdiagrams.org)
```

---

## Getting Started

```bash
# Create solution
dotnet new sln -n GemForge

# Create projects
dotnet new classlib -n GemForge.Domain -o src/GemForge.Domain
dotnet new classlib -n GemForge.Application -o src/GemForge.Application
dotnet new classlib -n GemForge.Infrastructure -o src/GemForge.Infrastructure
dotnet new avalonia.app -n GemForge.Desktop -o src/GemForge.Desktop

# Add to solution
dotnet sln add src/GemForge.Domain
dotnet sln add src/GemForge.Application
dotnet sln add src/GemForge.Infrastructure
dotnet sln add src/GemForge.Desktop

# Add test projects
dotnet new xunit -n GemForge.Domain.Tests -o tests/GemForge.Domain.Tests
dotnet sln add tests/GemForge.Domain.Tests

# Add NuGet packages (Infrastructure)
cd src/GemForge.Infrastructure
dotnet add package geometry3Sharp
dotnet add package GeometRi
dotnet add package Silk.NET.OpenGL
```

---

## Notes

- Download sample .asc files from facetdiagrams.org for testing
- Start with simple designs (Standard Round Brilliant) before complex ones
- The domain layer should have ZERO NuGet dependencies—keep it pure C#
- geometry3Sharp goes in Infrastructure, not Domain
