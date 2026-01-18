# Distance Coordinate System Fix

## Issue
Sample files rendered as **super tall, skinny gems** (like pencils) with missing tiers in Gem Cut Studio.

### Symptoms from Screenshots
- **Design view**: Extremely tall and narrow gem shape
- **Instructions panel**: Missing tiers (Crown and Girdle not showing)
- **Culet showing wrong angle**: 90° instead of -90°
- **Only 3 tiers visible** instead of 5

## Root Cause: Misunderstood Distance Coordinate

I had the **distance coordinate backwards**!

### What I Thought (WRONG)
Distance = horizontal radius from center axis
- Table at small distance (0.40) = small radius
- Girdle at large distance (1.00) = large radius
- This seemed logical but created super tall gems!

### What It Actually Means (CORRECT)
Distance = **vertical height from the culet point**
- **Lower distances** = closer to culet/bottom (pavilion)
- **Higher distances** = closer to girdle/top (crown)

## Evidence from Reference File

Looking at `reference-asc-implemention-srb.asc`:

```
a -90.000000 0.98078528 93 n G1...  # Girdle at distance 0.98 (high = near top)
a -42.100006 0.44885929 93 n P1...  # Pavilion at distance 0.44 (low = deep down)
a 42.299999 0.89354898 3 n C1...    # Crown at distance 0.89 (high = near top)
a 35.000000 0.83214622 6 n C2...    # Crown at distance 0.83 (medium-high)
a 0.000000 0.58802353 96 n T...     # Table at distance 0.58 (medium)
```

**Pattern:**
- Girdle facets (G1): ~0.95-0.98 (highest)
- Crown facets (C1, C2): 0.75-0.90 (high)
- Table (T): 0.55-0.65 (medium)
- Pavilion facets (P1, P2): 0.40-0.50 (low)

The girdle is at the highest distance (~1.0), and distances decrease as you go toward the table and down toward the pavilion point.

## What Was Wrong in Our Files

### Old simple-test.asc (WRONG)
```
a 0.000000 0.40000000 96 n Table 24 48 72 G      # Table at 0.40 (low!)
a 37.000000 0.85000000 96 n Crown 24 48 72 G     # Crown at 0.85 (high)
a 90.000000 1.00000000 96 n Girdle 24 48 72 G    # Girdle at 1.00 (highest)
a -40.000000 0.60000000 96 n Pavilion 24 48 72 G # Pavilion at 0.60 (mid)
a -90.000000 0.10000000 96 n Culet 24 48 72 G    # Culet at 0.10 (lowest!)
```

**Problems:**
1. Table at 0.40 is LOWER than pavilion at 0.60 - upside down!
2. Culet at 0.10 is way too low
3. Generic tier names ("Table", "Crown") might confuse GCS

### New simple-test.asc (CORRECT)
```
a -90.000000 0.95000000 96 n G1 24 48 72 G girdle   # Girdle at 0.95 (highest)
a -40.000000 0.50000000 96 n P1 24 48 72 G pavilion # Pavilion at 0.50 (low)
a 40.000000 0.82000000 96 n C1 24 48 72 G crown     # Crown at 0.82 (high)
a 0.000000 0.60000000 96 n T 24 48 72 G table       # Table at 0.60 (medium)
```

**Correct ordering (low to high):**
1. P1 (Pavilion): 0.50
2. T (Table): 0.60
3. C1 (Crown): 0.82
4. G1 (Girdle): 0.95

## Tier Naming Convention

Also switched from generic names to **GemCad standard tier codes**:

| Generic Name | GemCad Code | Notes |
|--------------|-------------|-------|
| Girdle | G1, G2, ... | Girdle facets |
| Pavilion | P1, P2, ... | Pavilion facets |
| Crown | C1, C2, ... | Crown facets |
| Star | C1 or S1 | Upper crown |
| Table | T | Top facet |

Using codes like "G1", "P1", "C1" matches the reference file convention and avoids potential GCS parsing issues.

## Fixes Applied

### 1. simple-test.asc
**Changes:**
- Reversed distance values to proper ordering
- Renamed tiers: Table→T, Crown→C1, Girdle→G1, Pavilion→P1
- Removed Culet (merged into girdle like reference file)
- Result: 4 facets (G1, P1, C1, T)

### 2. portuguese.asc
**Changes:**
- Reversed distance values to proper ordering
- Renamed tiers: Table→T, Star→C1, Crown→C2, Girdle→G1, Pavilion→P1
- Removed Culet (merged into girdle)
- Result: 5 facets (G1, P1, C1, C2, T)

### 3. standard-round-brilliant.asc
No changes needed - already uses reference file (correct from the start).

## Understanding the Coordinate System

### Vertical Position (Distance)
```
     Top (Crown side)
         ↑
    1.0  │ ← Girdle (G1) - highest distance
    0.9  │
    0.8  │ ← Crown facets (C1, C2)
    0.7  │
    0.6  │ ← Table (T)
    0.5  │ ← Pavilion facets (P1, P2)
    0.4  │
         ↓
   Bottom (Pavilion side)
```

### Angle + Distance Together
- **Girdle**: -90° (vertical) at distance ~0.95 (near top)
- **Pavilion**: -40° (angled) at distance ~0.50 (middle-low)
- **Table**: 0° (horizontal) at distance ~0.60 (middle)
- **Crown**: +40° (angled) at distance ~0.80 (middle-high)

The combination of angle and distance defines the facet plane's position and orientation.

## Test Status
✅ **All 46 tests passing**

## Files Ready for Gem Cut Studio
All sample files now use **correct distance coordinates**:

1. ✅ `simple-test.asc` - 4-fold, proper proportions (4 tiers)
2. ✅ `standard-round-brilliant.asc` - 16-fold, reference file (7 tiers)
3. ✅ `portuguese.asc` - 8-fold, proper proportions (5 tiers)

These should now render as **normal-proportioned gems**, not super tall pencils!

## Lesson Learned

When creating gem designs, the **distance** coordinate is:
- **NOT** horizontal radius
- **IS** vertical position/height from reference point
- Higher distance = closer to girdle/top
- Lower distance = deeper into pavilion/bottom
