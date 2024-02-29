using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

internal record typeMetadata
{
    public string Name { get; set; } = string.Empty;
    public List<string> Schemas { get; set; } = new();
    public string Exponents { get; set; } = string.Empty;
    public string[]? Fields { get; set; }

    internal void AddSchema(string schema)
    {
        Schemas.Add(schema);
    }
}

public class IfcSchema_DatatypeNamesGenerator
{
    internal static string Execute()
    {

        // start from documented... detail the exponents
        // then add their schemas and the others, with relevant schemas

        var documentedMeasures = GetDocumentationMeasures().ToList();
        var dTypes = documentedMeasures.ToDictionary(x => x.Name, x => x);

        //var datatypeNames = GetAllDatatypeNames().ToList();
        //var dttNames = new Dictionary<string, List<string>>();

        foreach (var schema in Program.schemas)
        {
            System.Reflection.Module module = SchemaHelper.GetModule(schema);
            var metaD = ExpressMetaData.GetMetadata(module);

            var values = GetExpressValues(metaD)
                .Concat(GetEnumValueTypes(schema))
                .Select(x=>x.ToUpperInvariant())
                .Distinct();
            foreach (var daDataType in values)
            {
                if (dTypes.TryGetValue(daDataType, out var lst))
                {
                    lst.AddSchema(schema);
                }
                else
                {
                    var t = new typeMetadata() { Name = daDataType };
                    t.AddSchema(schema);
                    dTypes.Add(daDataType, t);
                }
            }
        }

        foreach (var datatype in dTypes.Keys)
        {
            // check if measure is available
            if (datatype.EndsWith("MEASURE"))
            {
                if (!NonConvertibleMeasures.Contains(datatype) && !documentedMeasures.Any(x=>x.Name == datatype.ToUpperInvariant()))
                {
                    Program.Message($"Warning: dataType {datatype} is missing in documentation", ConsoleColor.DarkYellow);
                    Debug.WriteLine(datatype);
                }
            }
        }


        var source = stub;
        var sbMeasures = new StringBuilder();
        foreach (var clNm in dTypes.Keys.OrderBy(x => x))
        {
            var fnd = dTypes[clNm];		
            if (fnd.Fields is not null)
            {
                var t = $"""new IfcMeasureInformation("{fnd.Fields[0]}","{fnd.Fields[1]}","{fnd.Fields[2]}","{fnd.Fields[3]}","{fnd.Fields[4]}","{fnd.Fields[5]}","{fnd.Fields[6]}")""";
                sbMeasures.AppendLine($"""               yield return new IfcDataTypeInformation("{clNm}", {CodeHelpers.NewStringArray(fnd.Schemas)}, {t});""");
            }
            else
                sbMeasures.AppendLine($"""               yield return new IfcDataTypeInformation("{clNm}", {CodeHelpers.NewStringArray(fnd.Schemas)});""");
        }
        source = source.Replace($"<PlaceHolderDataTypes>\r\n", sbMeasures.ToString());
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        
        return source;

    }

    public static IEnumerable<string> NonConvertibleMeasures { get; } =
	[
		"IFCCONTEXTDEPENDENTMEASURE","IFCCOUNTMEASURE","IFCDESCRIPTIVEMEASURE","IFCMONETARYMEASURE","IFCNORMALISEDRATIOMEASURE","IFCNUMERICMEASURE","IFCPOSITIVERATIOMEASURE","IFCRATIOMEASURE"
    ];

    private static IEnumerable<string> GetExpressValues(ExpressMetaData metaD)
    {
        var HandledTypes = metaD.Types().Select(x => x.Name.ToUpperInvariant()).ToList();
        foreach (var className in HandledTypes)
        {
            var daType = metaD.ExpressType(className.ToUpperInvariant());
            var t = daType.Type.GetInterfaces().Select(x => x.Name).Contains("IExpressValueType");
            if (t && !daType.UnderlyingType.Name.StartsWith("List"))
            {
                 yield return className;
            }
        }
    }

    private static IEnumerable<string> GetEnumValueTypes(string schema)
    {
        System.Reflection.Module module = SchemaHelper.GetModule(schema);
        var tp2 = module.GetTypes().Where(x => !string.IsNullOrEmpty(x.BaseType?.Name) && x.BaseType.Name == "Enum").ToList();
        return tp2.Select(x => x.Name.ToUpperInvariant()).Where(x => x.EndsWith("ENUM") && x.StartsWith("IFC"));
    }


    // this list is taken from the properties. 
    // a debug statement in IfcSchema_PropertiesGenerator can help updating it.
    private static IEnumerable<string> GetPropsDatatypes()
    {
		yield return "IfcText";
		yield return "IfcLabel";
		yield return "IfcCountMeasure";
		yield return "IfcBoolean";
		yield return "IfcPowerMeasure";
		yield return "IfcVolumetricFlowRateMeasure";
		yield return "IfcPressureMeasure";
		yield return "IfcForceMeasure";
		yield return "IfcLengthMeasure";
		yield return "IfcPlaneAngleMeasure";
		yield return "IfcTorqueMeasure";
		yield return "IfcPositiveRatioMeasure";
		yield return "IfcThermodynamicTemperatureMeasure";
		yield return "IfcPositiveLengthMeasure";
		yield return "IfcMassMeasure";
		yield return "IfcReal";
		yield return "IfcAreaMeasure";
		yield return "IfcInteger";
		yield return "IfcIdentifier";
		yield return "IfcVolumeMeasure";
		yield return "IfcLogical";
		yield return "IfcRotationalFrequencyMeasure";
		yield return "IfcTimeMeasure";
		yield return "IfcThermalTransmittanceMeasure";
		yield return "IfcNormalisedRatioMeasure";
		yield return "IfcLinearVelocityMeasure";
		yield return "IfcMassPerLengthMeasure";
		yield return "IfcElectricVoltageMeasure";
		yield return "IfcElectricResistanceMeasure";
		yield return "IfcElectricCurrentMeasure";
		yield return "IfcPositivePlaneAngleMeasure";
		yield return "IfcMassFlowRateMeasure";
		yield return "IfcLuminousFluxMeasure";
		yield return "IfcIlluminanceMeasure";
		yield return "IfcSoundPressureMeasure";
		yield return "IfcHeatFluxDensityMeasure";
		yield return "IfcRatioMeasure";
		yield return "IfcFrequencyMeasure";
		yield return "IfcThermalResistanceMeasure";
		yield return "IfcThermalConductivityMeasure";
		yield return "IfcPlanarForceMeasure";
		yield return "IfcAreaDensityMeasure";
		yield return "IfcMassDensityMeasure";
		yield return "IfcDate";
		yield return "IfcEnergyMeasure";
		yield return "IfcSpecificHeatCapacityMeasure";
		yield return "IfcMolecularWeightMeasure";
		yield return "IfcHeatingValueMeasure";
		yield return "IfcIsothermalMoistureCapacityMeasure";
		yield return "IfcMoistureDiffusivityMeasure";
		yield return "IfcVaporPermeabilityMeasure";
		yield return "IfcDynamicViscosityMeasure";
		yield return "IfcModulusOfElasticityMeasure";
		yield return "IfcThermalExpansionCoefficientMeasure";
		yield return "IfcIonConcentrationMeasure";
		yield return "IfcPHMeasure";
		yield return "IfcDateTime";
		yield return "IfcNonNegativeLengthMeasure";
		yield return "IfcSectionModulusMeasure";
		yield return "IfcMomentOfInertiaMeasure";
		yield return "IfcWarpingConstantMeasure";
		yield return "IfcDuration";
		yield return "IfcTemperatureRateOfChangeMeasure";
		yield return "IfcComplexNumber";
		yield return "IfcTime";
		yield return "IfcURIReference";
		yield return "IfcAccelerationMeasure";
		yield return "IfcNumericMeasure";
		yield return "IfcSoundPowerLevelMeasure";
		yield return "IfcIntegerCountRateMeasure";
		yield return "IfcElectricChargeMeasure";
		yield return "IfcElectricCapacitanceMeasure";
		yield return "IfcInductanceMeasure";
		yield return "IfcAngularVelocityMeasure";
	}

    private static IEnumerable<string> GetExtraMeasureNames()
    {
		// IfcSimpleValue classes:
		yield return "IfcBinary";
        yield return "IfcBoolean";
        yield return "IfcDate";
        yield return "IfcDateTime";
        yield return "IfcDuration";
        yield return "IfcIdentifier";
        yield return "IfcInteger";
        yield return "IfcLabel";
        yield return "IfcLogical";
        yield return "IfcPositiveInteger";
        yield return "IfcReal";
        yield return "IfcText";
        yield return "IfcTime";
        yield return "IfcTimeStamp";
        yield return "IfcURIReference";

		// all inheriting from string in ifc4x3 dev 7079993 that are not in the previous list
        // todo: these need to be discussed for 1.0.
		yield return "IfcDescriptiveMeasure";
		yield return "IfcFontStyle";
		yield return "IfcFontVariant";
		yield return "IfcFontWeight";
		yield return "IfcGloballyUniqueId";
		yield return "IfcPresentableText";
		yield return "IfcTextAlignment";
		yield return "IfcTextDecoration";
		yield return "IfcTextFontName";
		yield return "IfcTextTransformation";
		yield return "IfcWellKnownTextLiteral";

		// extra measure
		yield return "IfcThermalTransmittanceMeasure";
    }

    private static IEnumerable<typeMetadata> GetDocumentationMeasures()
    {
        var markDown = File.ReadAllLines(@"buildingSMART\units.md");
        foreach (var line in markDown)
        {
            var modline = line.Trim(' ');
            var lineCells = modline.Split('|');
            if (lineCells.Length != 9)
                continue;
            var firstCell = lineCells[1].Trim();
            if (firstCell.Contains(' ') ||  firstCell.Contains('\t') || firstCell.Contains('-'))
                continue;

            var ret = new typeMetadata() {
                Name = firstCell,
                Exponents = lineCells[6].Trim(),
                Fields = lineCells.Skip(1).Take(7).Select(x => x.Trim()).ToArray(),
            };
            yield return ret;
        }
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
        /// The names of dataType classes across all schemas.
        /// </summary>
        public static IEnumerable<IfcDataTypeInformation> AllDataTypes
        {
            get
            {
<PlaceHolderDataTypes>
            }
        }
    }
}

";

}
