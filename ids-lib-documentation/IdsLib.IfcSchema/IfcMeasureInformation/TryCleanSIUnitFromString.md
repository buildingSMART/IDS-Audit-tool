# IfcMeasureInformation.TryCleanSIUnitFromString method

Tries to parse a string into a SI unit name, and returns the relevant components.

```csharp
public static bool TryCleanSIUnitFromString(string unitName, out string? bareUnit, 
    out SiPrefix siPrefix, out int pow)
```

## See Also

* enum [SiPrefix](../IfcConversionUnitInformation.SiPrefix.md)
* record [IfcMeasureInformation](../IfcMeasureInformation.md)
* namespace [IdsLib.IfcSchema](../../ids-lib.md)

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->
