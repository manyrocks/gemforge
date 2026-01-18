# .ASC Format Fix - What Changed

## Issue
The generated .asc files didn't open in Gem Cut Studio because the format didn't match the actual GemCad specification.

## What Was Wrong

### 1. Gear Line (Line 2)
**Incorrect:** `g 96 0.0`
**Correct:** `96 0.0`

The gear line should NOT have a 'g' prefix in the standard format.

### 2. Facet Line Format
**Incorrect:**
```
a <angle> <distance> <index1> <index2> <index3> ... n <tier_name>
```

**Correct:**
```
a <angle> <distance> <first_index> n <tier_name> <additional_indices...> G [comment]
```

The key differences:
- First index comes BEFORE the 'n' marker
- Tier name comes immediately after 'n'
- Additional indices come AFTER the tier name
- 'G' marker at the end (with optional comment after it)

### Example Comparison

**Our original format:**
```
a 90.000000 0.00000000 96 24 48 72 n Table
```

**Correct format:**
```
a 90.000000 0.00000000 96 n Table 24 48 72 G
```

## Files Changed

### Parser (AscParser.cs)
1. Added check for numeric first part to handle gear lines without 'g' prefix
2. Rewrote `ParseFacetLine()` to parse first index, then tier name, then additional indices
3. Updated `Write()` method to output correct format:
   - Removed 'g' prefix from gear line
   - Split indices into first + additional for facet lines
   - Added 'G' marker at end of facet lines

### Sample Files
Updated all three sample files to use correct format:
- `simple-test.asc`
- `standard-round-brilliant.asc`
- `portuguese.asc`

Also removed spaces from tier names (e.g., "Crown Main 1" → "CrownMain1") for compatibility.

## Testing

### Test Results
- **46 tests** passing (was 43, added 3 new tests)
- New tests include:
  - `ReferenceFileTests.cs` - Validates parsing of known-good reference file
  - `FileOutputTest.cs` - Generates round-trip output for manual verification

### Round-Trip Validation
The reference file (`reference-asc-implemention-srb.asc`) parses correctly and writes back in identical format:

**Input:**
```
a -90.000000 0.98078528 93 n G1 87 81 75 69 63 57 51 45 39 33 27 21 15 9 3 G Cut girdle
```

**Output (after parse → write):**
```
a -90.000000 0.98078528 93 n G1 87 81 75 69 63 57 51 45 39 33 27 21 15 9 3 G
```

Only difference: Comment text after 'G' is not preserved (we don't store comments in the Facet model currently).

## Files for Manual Testing

The following files can now be tested in Gem Cut Studio:
1. `samples/designs/simple-test.asc` - Basic 5-facet test
2. `samples/designs/standard-round-brilliant.asc` - Classic brilliant
3. `samples/designs/portuguese.asc` - Complex multi-tier design
4. `test-output/roundtrip-output.asc` - Round-trip of reference file

All should now open correctly in Gem Cut Studio.

## Future Enhancement (Optional)

Consider adding a `Comment` property to the `Facet` class to preserve the comment text after the 'G' marker:

```csharp
public class Facet
{
    // ... existing properties ...
    public string? Comment { get; set; }
}
```

This would allow full round-trip preservation of all data in the .asc file.
