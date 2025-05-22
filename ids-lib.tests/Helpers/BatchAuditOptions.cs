using IdsLib;
using System.Collections.Generic;
using System.Linq;

namespace idsTool.tests.Helpers
{
	internal class BatchAuditOptions : IBatchAuditOptions
	{
		public IEnumerable<string> SchemaFiles { get; set; } = Enumerable.Empty<string>();

		public bool AuditSchemaDefinition { get; set; }

		public string InputExtension { get; set; } = "ids";

		public string InputSource { get; set; } = string.Empty;

		public bool OmitIdsContentAudit { get; set; }

		public string OmitIdsContentAuditPattern { get; set; } = string.Empty;
	}
}