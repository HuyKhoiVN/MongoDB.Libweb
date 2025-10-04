# JSON String Parsing Test Cases

## Test Cases for ParseJsonStringToList Method

### Test Case 1: Valid JSON Array String
**Input**: `["68dc0413c137925c6614f058","68d91895ff121d84abc243be"]`
**Expected Output**: `["68dc0413c137925c6614f058", "68d91895ff121d84abc243be"]`

### Test Case 2: Single ObjectId String
**Input**: `["68dc0413c137925c6614f058"]`
**Expected Output**: `["68dc0413c137925c6614f058"]`

### Test Case 3: Empty Array
**Input**: `[]`
**Expected Output**: `[]`

### Test Case 4: Invalid JSON (Fallback)
**Input**: `["invalid-json-format"`
**Expected Output**: `["invalid-json-format"]` (treated as single value)

### Test Case 5: Mixed Valid and Invalid
**Input**: `["68dc0413c137925c6614f058", "invalid-json"]`
**Expected Output**: `["68dc0413c137925c6614f058", "invalid-json"]`

### Test Case 6: Null Input
**Input**: `null`
**Expected Output**: `[]`

### Test Case 7: Empty String
**Input**: `[""]`
**Expected Output**: `[]` (empty strings filtered out)

## Implementation Logic

```csharp
private static List<string> ParseJsonStringToList(List<string> input)
{
    if (input == null || !input.Any())
        return new List<string>();

    var result = new List<string>();
    foreach (var item in input)
    {
        if (string.IsNullOrEmpty(item))
            continue;

        // Check if it's a JSON array string
        if (item.StartsWith("[") && item.EndsWith("]"))
        {
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<string[]>(item);
                if (parsed != null)
                {
                    result.AddRange(parsed.Where(x => !string.IsNullOrEmpty(x)));
                }
            }
            catch
            {
                // If JSON parsing fails, treat as single value
                result.Add(item);
            }
        }
        else
        {
            // Single value, add directly
            result.Add(item);
        }
    }
    return result;
}
```

## Client-Side Data Flow

1. **Select2 Multi-Select**: User selects multiple authors/categories
2. **JavaScript**: `$('#bookAuthors').val()` returns array of ObjectIds
3. **Form Submission**: `JSON.stringify(authors)` converts to JSON string
4. **Server**: `ParseJsonStringToList()` converts back to List<string>
5. **Validation**: `ValidateAuthorsExistAsync()` checks ObjectId existence
6. **Database**: Store as List<string> of ObjectIds

## Error Handling

- **JSON Parse Error**: Falls back to treating as single value
- **Empty Values**: Filtered out automatically
- **Null Input**: Returns empty list
- **Invalid ObjectId**: Caught by `IsValidObjectId()` validation
- **Non-existent ObjectId**: Caught by database existence validation

## Benefits

1. **Flexible Input**: Handles both JSON strings and single values
2. **Robust Parsing**: Graceful fallback for invalid JSON
3. **Data Integrity**: Maintains ObjectId validation
4. **Error Recovery**: Continues processing even with partial failures
5. **Client Compatibility**: Works with existing Select2 implementation
