# Audit.Run method (1 of 2)

Entry point to access the library features in batch mode either on directories or single files

```csharp
public static Status Run(IBatchAuditOptions batchOptions, ILogger? logger = null)
```

| parameter | description |
| --- | --- |
| batchOptions | configuration options for the execution of audits on a file or folder |
| logger | the optional logger provides fine-grained feedback on all the audits performed |

## Return Value

A status enum that summarizes the result for all audits executed

## See Also

* enum [Status](../Audit.Status.md)
* interface [IBatchAuditOptions](../IBatchAuditOptions.md)
* class [Audit](../Audit.md)
* namespace [IdsLib](../../ids-lib.md)

---

# Audit.Run method (2 of 2)

Main entry point to access the library features via a stream to read the IDS content.

```csharp
public static Status Run(Stream idsSource, SingleAuditOptions options, ILogger? logger = null)
```

| parameter | description |
| --- | --- |
| idsSource | the stream providing access to the content of the IDS to be audited |
| options | specifies the behaviour of the audit |
| logger | the optional logger provides fine-grained feedback on all the audits performed and any issues encountered |

## Return Value

A status enum that summarizes the result for all audits on the single stream

## See Also

* enum [Status](../Audit.Status.md)
* class [SingleAuditOptions](../SingleAuditOptions.md)
* class [Audit](../Audit.md)
* namespace [IdsLib](../../ids-lib.md)

<!-- DO NOT EDIT: generated by xmldocmd for ids-lib.dll -->
