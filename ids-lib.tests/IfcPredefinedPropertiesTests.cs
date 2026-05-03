using FluentAssertions;
using IdsLib.IfcSchema;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace idsLib.tests
{
    public class IfcPredefinedPropertiesTests
    {
        private readonly ITestOutputHelper output;

        public IfcPredefinedPropertiesTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// we know that predefined properties have different types across the schemas
        /// they can be listed in the output of this test
        /// </summary>
        [Fact]
        public void IfcMeasureCoherenceAcrossSchemasPerProperty()
        {
            Dictionary<string, List<string>> propToMeasure = new ();

            var schemas = SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions);
            foreach (var schema in schemas)
            {
                foreach (var pset in schema.PropertySets)
                {
                    foreach (var prop in pset.Properties)
                    {
                        if (!prop.HasDataTypes(out var type))
                            continue;
                        if (type.Count() > 1)
                            continue;
						if (string.IsNullOrWhiteSpace(type.First()))
						{
							output.WriteLine($"no type for {pset.Name}.{prop.Name} in {schema.Version}");
						}

						var fullPropName = $"{pset.Name}.{prop.Name}";
                        if (propToMeasure.TryGetValue(fullPropName, out var measures))
                            measures.Add(type.First());
                        else
                            propToMeasure.Add(fullPropName, new List<string>() { type.First() });
                    }
                }
            }

			List<string> acknowledgedDifferences =
				[
				"IFCAREAMEASURE, IFCCOUNTMEASURE",
				"IFCCOUNTMEASURE, IFCINTEGER",
				"IFCCOUNTMEASURE, IFCVOLUMEMEASURE",
				"IFCDURATION, IFCTIMEMEASURE",
				"IFCELECTRICCURRENTMEASURE, IFCPOWERMEASURE",
				"IFCLABEL, IFCPRESSUREMEASURE",
				"IFCLABEL, IFCTEXT",
				"IFCLENGTHMEASURE, IFCPOSITIVELENGTHMEASURE",
				"IFCNONNEGATIVELENGTHMEASURE, IFCPOSITIVELENGTHMEASURE",
				"IFCNORMALISEDRATIOMEASURE, IFCPOSITIVERATIOMEASURE",
				"IFCNORMALISEDRATIOMEASURE, IFCREAL",
				"IFCPOSITIVERATIOMEASURE, IFCREAL",
				"IFCPRESSUREMEASURE, IFCTIMESERIES",
				];

            var unexpectedMeasureTypes = 0;
            foreach (var item in propToMeasure)
            {
                var val = item.Value;
                var dist = val.Distinct().OrderBy(x=>x).ToArray();

                if (dist.Length > 1)
                {
					var orderedJoin = string.Join(", ", dist);
					if (acknowledgedDifferences.Contains(orderedJoin))
						continue;
					output.WriteLine($"{dist.Length} measure values for {item.Key}: '{orderedJoin}'");
                    unexpectedMeasureTypes++;
                }   
            }
            unexpectedMeasureTypes.Should().Be(0, "there should be no un-acknowledged variations");
        }

        /// <summary>
        /// we know that predefined properties have different types across the schemas
        /// they can be listed in the output of this test
        /// </summary>
        [Fact]
        public void IfcPropertyNameCoherenceAcrossSchemas()
        {
            Dictionary<string, List<string>> propNameToPset = new();

            var schemas = SchemaInfo.GetSchemas(IfcSchemaVersions.IfcAllVersions);
            foreach (var schema in schemas)
            {
                foreach (var pset in schema.PropertySets)
                {
                    foreach (var prop in pset.Properties)
                    {
                        var fullPropName = $"{prop.Name}";
                        if (propNameToPset.TryGetValue(fullPropName, out var psets))
                            psets.Add(pset.Name);
                        else
                            propNameToPset.Add(fullPropName, new List<string>() { pset.Name });
                    }
                }
            }
            var unexpectedMeasureTypes = 0;
            foreach (var item in propNameToPset.OrderByDescending(x=>x.Value.Count))
            {
                var propertySets = item.Value;
                var distinctpropertySets = propertySets.Distinct().ToArray();

                if (distinctpropertySets.Length > 1)
                {
                    output.WriteLine($"{distinctpropertySets.Length} property sets for {item.Key}: {string.Join(", ", distinctpropertySets)}");
                    unexpectedMeasureTypes++;
                }
            }
			var expectedCount = 641;
            unexpectedMeasureTypes.Should().Be(expectedCount, $"{expectedCount} is the count of acknowledged variations");
        }
    }
}
