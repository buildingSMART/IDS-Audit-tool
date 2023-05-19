using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace IdsLib;

public static partial class Audit
{
    /// <summary>
    /// An interface to allow the specification of schemas to be loaded when reading the IDS xml file.
    /// </summary>
    public interface ISchemaProvider
    {
        /// <summary>
        /// This method should return the collection of schemas to be loaded.
        /// </summary>
        /// <param name="source">If available, this is the version of the schema that the application has 
        /// determined appropriate for the IDS.</param>
        /// <param name="logger">Logger for detailed feedback on any useful information.</param>
        /// <param name="schemas">The schemas to be loaded for the schema validation</param>
        /// <returns>The status of the function, if no problem encountered, return <see cref="Status.Ok"/>.</returns>
        Audit.Status GetSchemas(Stream source, ILogger? logger, out IEnumerable<XmlSchema> schemas);
    }
}
