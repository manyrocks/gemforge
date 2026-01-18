# Geometry Fix - Valid Gemstone Designs

## Issue
The sample .asc files had **invalid geometry** that caused weird renders in Gem Cut Studio:
- Spiky protrusions instead of smooth round shapes
- Incorrect tier grouping in the instructions panel
- Simplified/wrong 3D renders

## Root Cause: Invalid Angles and Distances

Creating valid gemstone geometry requires understanding how angles and distances work together:

### Angle Meaning
- **0°** = Horizontal (parallel to girdle plane)
- **90°** = Vertical pointing up (perpendicular, crown side)
- **-90°** = Vertical pointing down (perpendicular, pavilion side)
- **Positive angles (0° to 90°)** = Crown facets (above girdle)
- **Negative angles (-90° to 0°)** = Pavilion facets (below girdle)

### Distance Meaning
- Distance is the **radius from the center axis** (normalized)
- Girdle typically at distance ~1.0 (widest part)
- Table (top) at smaller distance (e.g., 0.4-0.6)
- Culet (bottom point) at very small distance (e.g., 0.05-0.1)
- Crown/pavilion facets between table and girdle

### What Was Wrong

**Original simple-test.asc:**
```
a 90.000000 0.00000000 96 n Table 24 48 72 G  # WRONG: 90° at distance 0 = vertical line at center!
a 0.000000 1.00000000 96 n Girdle 24 48 72 G  # WRONG: 0° = horizontal, not vertical girdle
```

Problems:
- Table at 90° with distance 0 creates a vertical line (infinitely small), not a flat top surface
- Girdle at 0° is horizontal, but girdle should be vertical (90°)
- These create impossible geometry that doesn't form a valid solid

**Corrected simple-test.asc:**
```
a 0.000000 0.40000000 0 n Table 24 48 72 G    # Horizontal table at radius 0.4
a 37.000000 0.85000000 0 n Crown 24 48 72 G   # Angled crown at radius 0.85
a 90.000000 1.00000000 0 n Girdle 24 48 72 G  # Vertical girdle at radius 1.0 (widest)
a -40.000000 0.60000000 0 n Pavilion 24 48 72 G # Angled pavilion at radius 0.6
a -90.000000 0.10000000 0 n Culet G           # Vertical culet at radius 0.1 (small point)
```

This creates valid geometry:
- Horizontal table on top
- Angled crown facets connecting table to girdle
- Vertical girdle (widest part)
- Angled pavilion facets connecting girdle to culet
- Small culet point at bottom

## Fixes Applied

### 1. standard-round-brilliant.asc
**Replaced with the reference file design** (16-fold symmetry, 7 tiers)
- Uses known-good angles and distances from working design
- Properly calculated meetpoints between facets
- Will render correctly as a round brilliant

### 2. simple-test.asc
**Created simple but valid table-cut design** (4-fold symmetry, 5 facets)
- Horizontal table at 0° with radius 0.4
- Crown mains at 37° with radius 0.85
- Vertical girdle at 90° with radius 1.0 (widest point)
- Pavilion mains at -40° with radius 0.6
- Culet point at -90° with radius 0.1

### 3. portuguese.asc (renamed to simple-8-fold)
**Created simple 8-fold design** (8-fold symmetry, 6 facets)
- Similar structure to simple-test but with 8-fold symmetry
- Table, star, crown, girdle, pavilion, culet
- Valid angles and distances that create proper geometry

## Understanding Facet Geometry

### Valid Design Principles

1. **Table must be horizontal** (0°) at some radius < girdle
2. **Girdle is typically vertical** (90°) at the widest radius
3. **Crown facets** have positive angles (0° to 90°) at radii between table and girdle
4. **Pavilion facets** have negative angles (-90° to 0°) at radii between girdle and culet
5. **Culet** can be a point (very small radius) or flat (0° at small radius)

### Distance Relationships
```
Table distance < Crown distance < Girdle distance (1.0)
Culet distance < Pavilion distance < Girdle distance (1.0)
```

### Angle Relationships
- Crown angles: 0° (table) to ~45° (crown mains) to 90° (girdle)
- Pavilion angles: 0° (girdle plane) to ~-40° (pavilion) to -90° (culet)

## Test Status
✅ **All 46 tests passing**

## Files Ready for Gem Cut Studio
All three sample files now have **valid gemstone geometry**:

1. ✅ `simple-test.asc` - Simple 4-fold table cut (5 facets)
2. ✅ `standard-round-brilliant.asc` - Real brilliant design (7 tiers, 16-fold)
3. ✅ `portuguese.asc` - Simple 8-fold round (6 facets)

## Recommendation

For production designs, **use real designs from facetdiagrams.org** rather than creating synthetic ones. Real designs have:
- Precisely calculated angles for optimal light return
- Properly positioned meetpoints
- Tested cutting sequences
- Proven optical performance

Our sample files are now geometrically valid for **parser testing and basic rendering**, but not optimized for actual gem cutting.
