using FluentAssertions;
using IdsLib.IfcSchema;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
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
                        if (!prop.HasDataType(out var type))
                            continue;

                        var fullPropName = $"{pset.Name}.{prop.Name}";
                        if (propToMeasure.TryGetValue(fullPropName, out var measures))
                            measures.Add(type);
                        else
                            propToMeasure.Add(fullPropName, new List<string>() { type });
                    }
                }
            }
            var unexpectedMeasureTypes = 0;
            foreach (var item in propToMeasure)
            {
                var val = item.Value;
                var dist = val.Distinct().ToArray();

                if (dist.Length > 1)
                {
                    output.WriteLine($"{dist.Length} measure values for {item.Key}: {string.Join(", ", dist)}");
                    unexpectedMeasureTypes++;
                }   
            }
            unexpectedMeasureTypes.Should().Be(79, "these are the acknowledged variations");
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
                    output.WriteLine($"{distinctpropertySets.Length} propety sets for {item.Key}: {string.Join(", ", distinctpropertySets)}");
                    unexpectedMeasureTypes++;
                }
            }
            var expectedCount = 587;
            unexpectedMeasureTypes.Should().Be(expectedCount, $"{expectedCount} is the count of acknowledged variations");
        }
    }
}
