# GemForge References

## Gem Design Software (Existing)

| Software | URL | Notes |
|----------|-----|-------|
| GemCad | https://www.gemcad.com/ | Original, free, Windows only, closed source |
| Gem Cut Studio | https://gemcutstudio.com/ | Modern commercial ($150), good reference for features |
| Hackagem | https://hackagem.com/ | Web-based, open source JS, has .asc import code |
| facetdiagrams.org | http://facetdiagrams.org/ | Huge database of .asc designs for testing |

## .ASC File Format

**Hackagem's import documentation (shows exact format):**
https://hackagem.com/doc/posts/gemcad-import/

**Example .asc file structure:**
```
GemCad 5.0                           # Version
g 96 0.0                             # Index gear, rotation offset
y 4 y                                # Symmetry (yes, 4-fold, mirror)
I 1.54                               # Refractive index
H Design Name                        # Header lines
H Author, Date
a 90.000000 1.09380454 96 24 48 72 n g1   # Facet: angle, distance, indices, tier name
a -38.000000 0.87556750 96 72 48 24 n 1   # Negative = pavilion
F Footer note                        # Footer
```

**Line codes:**
- `g` - Index gear and rotation
- `y/n` - Symmetry settings  
- `I` - Refractive index
- `H` - Header/comments
- `a` - Facet (angle, distance, index positions, name)
- `F` - Footer
- `C` - Cutting sequence

## C# Libraries

### UI Framework
| Library | NuGet | License | URL |
|---------|-------|---------|-----|
| Avalonia | `Avalonia` | MIT | https://avaloniaui.net/ |
| Avalonia Docs | - | - | https://docs.avaloniaui.net/ |

### 3D/Graphics
| Library | NuGet | License | URL |
|---------|-------|---------|-----|
| Silk.NET | `Silk.NET.OpenGL` | MIT | https://github.com/dotnet/Silk.NET |
| geometry3Sharp | `geometry3Sharp` | Boost | https://github.com/gradientspace/geometry3Sharp |
| GeometRi | `GeometRi` | MIT | https://github.com/RiSearcher/GeometRi |

### Utilities
| Library | NuGet | License | URL |
|---------|-------|---------|-----|
| SkiaSharp | `SkiaSharp` | MIT | 2D rendering |
| PDFsharp | `PDFsharp` | MIT | PDF export |

## Geometry3Sharp Features (Most Relevant)

- `Vector3d`, `Vector2d` - Vector math
- `Plane3d` - Plane definitions and intersections
- `DMesh3` - Dynamic triangle mesh
- `MeshPlaneCut` - Cut mesh with plane
- `ConvexHull2` - 2D convex hull
- `Polygon2d` - 2D polygon operations

GitHub: https://github.com/gradientspace/geometry3Sharp
Tutorials: http://www.gradientspace.com/tutorials

## Avalonia + OpenGL

**OpenGLControlBase** - Built-in control for custom OpenGL rendering:
- Discussion: https://github.com/AvaloniaUI/Avalonia/discussions/6842
- Example: Avalonia ControlCatalog has a "disco teapot" OpenGL demo

**GritWorld case study** (3D engine built on Avalonia):
https://blog.jetbrains.com/dotnet/2021/05/10/case-study-how-gritworld-uses-rider-and-avalonia-to-build-a-powerful-3d-engine/

## Optical/Physics References

**Snell's Law:** n₁ sin(θ₁) = n₂ sin(θ₂)

**Critical angle:** θc = arcsin(n₂/n₁)

**Common refractive indices:**
| Material | RI |
|----------|-----|
| Quartz | 1.544-1.553 |
| Topaz | 1.619-1.627 |
| Tourmaline | 1.624-1.644 |
| Sapphire | 1.762-1.770 |
| Diamond | 2.417 |

**Specific gravity (for weight calc):**
| Material | SG |
|----------|-----|
| Quartz | 2.65 |
| Topaz | 3.53 |
| Tourmaline | 3.06 |
| Sapphire | 4.00 |
| Diamond | 3.52 |

**Weight formula:** Volume (mm³) × SG ÷ 200 = carats

## Community Resources

- GemologyOnline Forum: https://www.gemologyonline.com/Forum/
- International Gem Society: https://www.gemsociety.org/
- United States Faceters Guild: https://usfacetersguild.org/

## Sample Designs for Testing

Download .asc files from:
- http://facetdiagrams.org/ (tick "Only show Open designs")
- Search for "Standard Round Brilliant" for a classic test case
- Search for "Barion" designs for more complex meetpoint testing
