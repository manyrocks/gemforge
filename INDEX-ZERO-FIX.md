# Index 96 vs Index 0 Fix

## Issue
Sample files rendered as **sliced/partial gems** in Gem Cut Studio - literally showing 1/4 or 1/8 of the gem instead of the full round shape.

### Symptoms from Screenshot
- **3D render**: Wedge-shaped slice instead of full gem
- **Instructions panel**: Weird duplicate indices like "96-96-96-96-48-48-48-48"
- **Missing facets**: Only showing some tiers, not all

## Root Cause: Index 0 vs Index 96

In a 96-index gear system:
- **Mathematically**: Position 0 and position 96 represent the **same angle** (0° and 360° are equivalent)
- **In GemCad format**: The convention is to use **index 96**, not index 0

### What We Were Doing (Wrong)
```
a 0.000000 0.35000000 0 n Table 12 24 36 48 60 72 84 G
                       ^
                   Using index 0
```

### What GemCad Expects (Correct)
```
a 0.000000 0.58802353 96 n T G Meet c2
                       ^^
                   Using index 96
```

### Why This Matters

GCS (Gem Cut Studio) apparently treats index 0 differently than index 96, even though they're the same position. Using 0 caused:
1. Incorrect symmetry application
2. Facet duplication/missing facets
3. Partial gem rendering (slices instead of full gems)

## Evidence from Reference File

Looking at `reference-asc-implemention-srb.asc`:
```
a 19.799999 0.74447873 96 n C3 12 24 36 48 60 72 84 G meet c1
a 0.000000 0.58802353 96 n T G Meet c2
                       ^^
               Always uses 96, never 0
```

The reference file **always uses index 96** when referring to the zero-degree position.

## Fixes Applied

### 1. simple-test.asc (4-fold symmetry)
**Before:**
```
a 0.000000 0.40000000 0 n Table 24 48 72 G
a -90.000000 0.10000000 0 n Culet G
```

**After:**
```
a 0.000000 0.40000000 96 n Table 24 48 72 G
a -90.000000 0.10000000 96 n Culet 24 48 72 G
```

Changes:
- Replaced index 0 with index 96
- Added all 4 symmetric positions to Culet (was missing indices)

### 2. portuguese.asc (8-fold symmetry)
**Before:**
```
a 0.000000 0.35000000 0 n Table 12 24 36 48 60 72 84 G
a -90.000000 0.05000000 0 n Culet G
```

**After:**
```
a 0.000000 0.35000000 96 n Table 12 24 36 48 60 72 84 G
a -90.000000 0.05000000 96 n Culet 12 24 36 48 60 72 84 G
```

Changes:
- Replaced index 0 with index 96
- Added all 8 symmetric positions to Culet (was missing indices)

### 3. standard-round-brilliant.asc
No changes needed - already uses reference file format with index 96.

## Index Numbering Convention

For a 96-index gear:
- **Valid indices**: 1, 2, 3, ..., 95, 96
- **NOT**: 0, 1, 2, ..., 94, 95

| Angle | Index |
|-------|-------|
| 0°    | 96    |
| 3.75° | 1     |
| 7.5°  | 2     |
| ...   | ...   |
| 356.25° | 95  |
| 360°  | 96 (wraps) |

Think of it as **1-indexed, not 0-indexed**.

## Other Index Gears

This same principle applies to all index gears:
- **64-index**: Use 64, not 0
- **72-index**: Use 72, not 0
- **80-index**: Use 80, not 0
- **96-index**: Use 96, not 0
- **120-index**: Use 120, not 0

## Impact on Our Code

### Parser (No Changes Needed)
Our `AscParser.cs` correctly handles both:
- Reads index 96 as position 96
- Reads index 0 as position 0 (even though we shouldn't write it)

The parser is format-agnostic and just reads whatever integers are there.

### Writer (Needs Update - Future)
If we want our `Write()` method to always output GemCad-compliant files, we should:

```csharp
// In AscParser.Write() when writing indices
var firstIndex = facet.IndexPositions[0];
if (firstIndex == 0) firstIndex = (int)design.IndexGear; // Convert 0 to max index
```

But for now, files using index 96 work correctly.

## Test Status
✅ **All 46 tests passing**

## Files Ready for Gem Cut Studio
All sample files now use **index 96 convention**:

1. ✅ `simple-test.asc` - 4-fold, all facets use index 96
2. ✅ `standard-round-brilliant.asc` - 16-fold, reference file (already correct)
3. ✅ `portuguese.asc` - 8-fold, all facets use index 96

These should now render as **complete full gems**, not slices!
