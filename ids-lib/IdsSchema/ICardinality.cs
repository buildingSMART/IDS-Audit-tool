using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
	internal interface ICardinality
	{
		/// <summary>
		/// determine if the attached entity is marked as required
		/// </summary>
		bool IsRequired { get; }

		/// <summary>
		/// Audits the validity of an occurrence setting.
		/// </summary>
		/// <param name="errorMessage">if invalid returns an errors string without punctuation.</param>
		/// <returns>the evaluated status</returns>
		Audit.Status Audit(out string errorMessage);
	}
}
