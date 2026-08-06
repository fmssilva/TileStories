# Outline Table Refactoring TODO

## Task: Reorganize outline table to match marker/badge table pattern (Label/Type/Preview/Color columns)

### Implementation Steps:

- [x] 1. Analyze current implementation (DONE) - Lines ARE PNG sprites, confirmed via runtime code analysis
- [x] 2. Refactor DrawGlobalOutlineSection() method - reorganize columns to Label/Type/Preview/Color
- [x] 3. Test compilation - run Unity batch-mode compile log check (0 warnings, 0 errors)
- [x] 4. Run EditMode tests - verify 44/44 pass (PASSED)
- [x] 5. Run PlayMode tests - verify existing tests still pass (25/25 PASSED)

### Changes Made:

#### Modified `DrawGlobalOutlineSection()` method in `POIAuthoringToolWindow.cs`:

**Old column structure:**
- Type (preview + ObjectField combined) + Color + Remove

**New column structure (matching marker/badge pattern):**
- Level (110f - text field for entry.label)
- Type (140f - ObjectField for sprite picker)
- Preview (44f - 36x36 sprite thumbnail)
- Color (152f - color swatch + hex text field)
- Remove (26f - trash button)

### Files Modified:
- `Assets/Framework/Editor/POIAuthoringToolWindow.cs`

### Runtime Impact:
- NONE - purely an editor UI change

### Verification:
- Script validation: 0 warnings, 0 errors
- EditMode tests: 44/44 passed
- PlayMode tests: 25/25 passed