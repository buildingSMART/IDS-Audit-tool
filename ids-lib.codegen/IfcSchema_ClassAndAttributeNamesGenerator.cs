﻿using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

public class IfcSchema_ClassAndAttributeNamesGenerator
{
    internal static string Execute()
    {
        var classNames = new Dictionary<string, List<string>>();
        var attNames = new Dictionary<string, List<string>>();
        foreach (var schema in Program.schemas)
        {
            System.Reflection.Module module = SchemaHelper.GetModule(schema);
            var metaD = ExpressMetaData.GetMetadata(module);
            foreach (var daType in metaD.Types())
            {
                // only concrete classes are valid
                if (daType.Type.IsAbstract)
                    continue;

                // just class names
                if (classNames.TryGetValue(daType.Name, out var lst))
                    lst.Add(schema);
                else
                    classNames.Add(daType.Name, new List<string>() { schema });

                // Enriching schema with attribute names
                var thisattnames = daType.Properties.Values.Select(x => x.Name);
                foreach (var attributeName in thisattnames)
                {
                    if (attNames.TryGetValue(attributeName, out var attlst))
                    {
                        attlst.Add(schema);
                    }
                    else
                    {
                        attNames.Add(attributeName, new List<string>() { schema });
                    }
                }
            }
        }
        var source = stub;
        var sbClasses = new StringBuilder();
        var sbAtts = new StringBuilder();
        foreach (var clNm in classNames.Keys.OrderBy(x => x))
        {
            var schemes = classNames[clNm];
            sbClasses.AppendLine($"""               yield return new IfcClassInformation("{clNm}", {CodeHelpers.NewStringArray(schemes)});""");
        }
        foreach (var clNm in attNames.Keys.OrderBy(x => x))
        {
            var schemes = attNames[clNm];
            sbAtts.AppendLine($"""               yield return new IfcAttributeInformation("{clNm}", {CodeHelpers.NewStringArray(schemes.Distinct())});""");
        }
        source = source.Replace($"<PlaceHolderClasses>\r\n", sbClasses.ToString());
        source = source.Replace($"<PlaceHolderAttributes>\r\n", sbAtts.ToString());
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;
    }

    private const string stub = @"// <auto-generated/>
// This code was automatically generated with information from Xbim.Essentials <PlaceHolderVersion>.
// Any changes made to this file will be lost.

using System;
using System.Collections.Generic;

namespace IdsLib.IfcSchema
{
    public partial class SchemaInfo
    {
		/// <summary>
		/// This is obsolete since the name was misleading, use <see cref=""AllConcreteClasses""/> instead.
		/// </summary>
		[Obsolete(""Use AllConcreteClasses instead."")]
		public static IEnumerable<IfcClassInformation> AllClasses => AllConcreteClasses;
        

		/// <summary>
		/// The names of all concrete classes across known IFC schemas.
		/// </summary>
		public static IEnumerable<IfcClassInformation> AllConcreteClasses
        {
            get
            {
<PlaceHolderClasses>
            }
        }

        /// <summary>
        /// The names of all attributes across all schemas.
        /// </summary>
        public static IEnumerable<IfcAttributeInformation> AllAttributes
        {
            get
            {
<PlaceHolderAttributes>
            }
        }
    }
}

";

}
