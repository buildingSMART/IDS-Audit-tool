using System.Text;
using Xbim.Common.Metadata;

namespace IdsLib.codegen;

public class IfcSchema_DatatypeNamesGenerator
{
    internal static string Execute()
    {
        var datatypeNames = GetAllDatatypeNames();

        var datatypeInfos = new Dictionary<string, List<string>>();
        var attNames = new Dictionary<string, List<string>>();
        foreach (var schema in Program.schemas)
        {
            System.Reflection.Module module = SchemaHelper.GetModule(schema);
            var metaD = ExpressMetaData.GetMetadata(module);
            foreach (var daMeasure in datatypeNames)
            {
                // measure class names
                try
                {
                    var t = metaD.ExpressType(daMeasure.ToUpperInvariant());
                    if (t is null)
                        continue;
                    if (datatypeInfos.TryGetValue(daMeasure, out var lst))
                    {
                        lst.Add(schema);
                    }
                    else
                    {
                        datatypeInfos.Add(daMeasure, new List<string>() { schema });
                    }
                }
                catch 
                {
                    continue;
                }                
            }
        }
        var source = stub;
        var sbMeasures = new StringBuilder();
        foreach (var clNm in datatypeInfos.Keys.OrderBy(x => x))
        {
            var schemes = datatypeInfos[clNm];
            sbMeasures.AppendLine($"""               yield return new IfcMeasureInformation("{clNm}", {CodeHelpers.NewStringArray(schemes)});""");
        }
        source = source.Replace($"<PlaceHolderMeasures>\r\n", sbMeasures.ToString());
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;
    }

    private static IEnumerable<string> GetAllDatatypeNames()
    {
        return GetDocumentationMeasureNames()
            .Concat(GetExtraMeasureNames())
            .Concat(GetPropsDatatypes())
            .Distinct();
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

    private static IEnumerable<string> GetDocumentationMeasureNames()
    {
        var markDown = File.ReadAllLines(@"buildingSMART\units.md");
        foreach (var line in markDown)
        {
            var modline = line.Trim(' ');
            var lineCells = modline.Split('|');
            if (lineCells.Length != 8)
                continue;
            var first = lineCells[1].Trim();
            if (first.Contains(' ') ||  first.Contains('\t') || first.Contains('-'))
                continue;
            yield return first;
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
        /// The names of classes across all schemas.
        /// </summary>
        public static IEnumerable<IfcMeasureInformation> AllMeasures
        {
            get
            {
<PlaceHolderMeasures>
            }
        }
    }
}

";

}
