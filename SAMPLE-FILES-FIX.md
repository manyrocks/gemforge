# Sample Files Fix - Symmetry Corrections

## Issue
The three sample .asc files had **incorrect symmetry declarations** that didn't match the index positions, making them invalid designs that wouldn't open in Gem Cut Studio.

## Root Cause: Symmetry Mismatch

### What Was Wrong

All three files declared symmetry WITH mirror (`y X y`) but only provided index positions for symmetry WITHOUT mirror.

**Example from simple-test.asc:**
```
y 4 y           # 4-fold WITH mirror = 8 positions needed
a ... 96 n Table 24 48 72 G    # Only 4 indices provided (0, 24, 48, 72)
```

This is invalid because:
- `y 4 y` = 4-fold symmetry WITH mirror
- With 96-index gear: 96 ÷ 4 = 24 index step
- With mirror: Need 8 positions (0, 12, 24, 36, 48, 60, 72, 84)
- But facets only listed 4 positions (0, 24, 48, 72)

## Fixes Applied

### 1. simple-test.asc
**Changed:**
- Symmetry: `y 4 y` → `y 4 n` (removed mirror)
- Index positions: Now correctly 4 positions (0, 24, 48, 72) for 4-fold no-mirror
- Normalized index 96 to 0 (cleaner)

### 2. standard-round-brilliant.asc
**Changed:**
- Symmetry: `y 8 y` → `y 8 n` (removed mirror)
- Index positions: Now correctly 8 positions (0, 12, 24, 36, 48, 60, 72, 84) for 8-fold no-mirror
- Simplified to 6 facets (removed culet)
- Normalized indices to start from 0

### 3. portuguese.asc
**Changed:**
- Symmetry: `y 4 y` → `y 4 n` (removed mirror)
- Index positions: Now correctly 4 positions for 4-fold no-mirror
- Alternating facets use offset patterns (0, 24, 48, 72) vs (12, 36, 60, 84)
- Normalized indices

## Symmetry Rules Reference

For 96-index gear:

| Symmetry | Fold | Mirror | Positions | Indices |
|----------|------|--------|-----------|---------|
| `y 4 n`  | 4    | No     | 4         | 0, 24, 48, 72 |
| `y 4 y`  | 4    | Yes    | 8         | 0, 12, 24, 36, 48, 60, 72, 84 |
| `y 8 n`  | 8    | No     | 8         | 0, 12, 24, 36, 48, 60, 72, 84 |
| `y 8 y`  | 8    | Yes    | 16        | 0, 6, 12, 18, 24, ... |

**Formula:**
- Index step = 96 ÷ Fold
- Positions = Fold × (Mirror ? 2 : 1)
- Mirror offset = Index_step ÷ 2

## Test Status

✅ **All 46 tests passing**

Updated tests to reflect corrected file content:
- `ParseFile_SimpleTest_ParsesCorrectly` - Updated for new name/symmetry
- `Parse_StandardRoundBrilliant_ParsesCorrectly` - Updated for 8-fold no-mirror, 6 facets
- `Parse_Portuguese_ParsesCorrectly` - Updated for new name

## Files Ready for Testing

All sample files now use **valid symmetry configurations** and should open in Gem Cut Studio:

1. ✅ `samples/designs/simple-test.asc` - 4-fold, no mirror, 5 facets
2. ✅ `samples/designs/standard-round-brilliant.asc` - 8-fold, no mirror, 6 facets
3. ✅ `samples/designs/portuguese.asc` - 4-fold, no mirror, 10 facets
4. ✅ `test-output/roundtrip-output.asc` - Reference file roundtrip (known good)

## Note on Real Designs

These sample files are now **geometrically valid** but are simplified test cases. For real-world faceting:

- Download actual designs from [facetdiagrams.org](http://facetdiagrams.org/)
- Filter for "Open designs" to find public domain patterns
- Look for designs by known cutters (e.g., "Standard Round Brilliant" by various authors)

Real designs will have:
- Precise angles optimized for specific materials
- Proper meetpoint calculations
- Cutting sequences that build correctly
- Distance ratios that work in practice
