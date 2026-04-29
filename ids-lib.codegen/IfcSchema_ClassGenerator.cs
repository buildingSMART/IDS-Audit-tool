using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xbim.Common.Metadata;
using Xbim.IO.Xml.BsConf;
using static IdsLib.codegen.IfcSchema_Ifc2x3MapperGenerator;

namespace IdsLib.codegen;

public class IfcSchema_ClassGenerator
{
    /// <summary>
    /// SchemaInfo.GeneratedClass.cs
    /// </summary>
    internal static string Execute(List<Ifc2x3EntityMappingInformation> maps)
    {
        var source = stub;
        foreach (var schema in Program.schemas)
        {
			HashSet<string> typeNames = new();
			List<TypeMapper> entities = TypeMapper.GetFor(schema, maps, out var metaD);
			var sb = new StringBuilder();

            foreach (var classMap in entities)
            {
                var t = classMap.IfcMapToExpressType.Type.GetInterfaces().Select(x => x.Name).Contains("IExpressValueType");
                //if (t ) //!string.IsNullOrEmpty(daType?.UnderlyingType?.Name))
                //{
                //    Debug.WriteLine($"{daType.Name}: {daType.UnderlyingType.Name} - {t}");
                //}

                // Enriching schema with predefined types
                var propPdefT = classMap.IfcMapToExpressType.Properties.Values.FirstOrDefault(x => x.Name == "PredefinedType");
                var predType = "Enumerable.Empty<string>()";
                if (propPdefT != null)
                {
                    var pt = propPdefT.PropertyInfo.PropertyType;
                    pt = Nullable.GetUnderlyingType(pt) ?? pt;
                    var vals = Enum.GetValues(pt);

                    List<string> pdtypes = new();
                    foreach (var val in vals)
                    {
                        if (val is null)
                            continue;
                        pdtypes.Add(val.ToString()!);
                    }
                    predType = NewStringArray(pdtypes.ToArray());
                }

                // other fields
                var abstractOrNot = classMap.IfcMapToExpressType.Type.IsAbstract ? "ClassType.Abstract" : "ClassType.Concrete";
                var ns = classMap.IfcMapToExpressType.Type.Namespace![5..];

                // Enriching schema with attribute names
                var attnames = NewStringArray(classMap.IfcMapToExpressType.Properties.Values.Select(x => x.Name).ToArray());

				if (IsDuplicate(classMap.IdsName, schema, typeNames))
					continue;
				sb.AppendLine($@"			new ClassInfo(""{classMap.IdsName}"", ""{classMap.IfcMapToExpressType.SuperType?.Name}"", {abstractOrNot}, {predType}, ""{ns}"", {attnames}),");

            }
			foreach (var enumerationClass in IfcSchema_DatatypeNamesGenerator.GetEnumTypes(schema))
			{
				var ns = enumerationClass.Namespace!.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
				if (ns == "Interfaces")
				{
					ns = GetIfc4EnumNamespaces(enumerationClass.Name);
					if (ns == "")
					{
						throw new Exception("Must have a valid namespace hardcoded");
					}
				}
				if (IsDuplicate(enumerationClass.Name, schema, typeNames))
					continue;

				var vals = Enum.GetValues(enumerationClass).Cast<object>().Select(x => x.ToString()!).ToArray()!;
				var attnames = NewStringArray(vals);
				sb.AppendLine($@"			new ClassInfo(""{enumerationClass.Name}"", ""{schema}.{ns}"", {attnames}),");
			}
			source = source.Replace($"<PlaceHolder{schema}>\r\n", sb.ToString());
        }
        source = source.Replace($"<PlaceHolderVersion>", VersionHelper.GetFileVersion(typeof(ExpressMetaData)));
        return source;
    }

	private static bool IsDuplicate(string idsName, string schemaName, HashSet<string> typeNames)
	{
		if (typeNames.Contains(idsName))
		{
			Program.Message($"Warning: skipping duplicate type name `{idsName}` in `{schemaName}`", ConsoleColor.DarkYellow);
			return true;
		}
		typeNames.Add(idsName);
		return false;
	}

	private static string GetIfc4EnumNamespaces(string name)
	{
		return name switch
		{
			"IfcAddressTypeEnum" => "IfcActorResource",
			"IfcRoleEnum" => "IfcActorResource",
			"IfcDoorPanelOperationEnum" => "IfcArchitectureDomain",
			"IfcDoorPanelPositionEnum" => "IfcArchitectureDomain",
			"IfcDoorStyleConstructionEnum" => "IfcArchitectureDomain",
			"IfcDoorStyleOperationEnum" => "IfcArchitectureDomain",
			"IfcPermeableCoveringOperationEnum" => "IfcArchitectureDomain",
			"IfcWindowPanelOperationEnum" => "IfcArchitectureDomain",
			"IfcWindowPanelPositionEnum" => "IfcArchitectureDomain",
			"IfcWindowStyleConstructionEnum" => "IfcArchitectureDomain",
			"IfcWindowStyleOperationEnum" => "IfcArchitectureDomain",
			"IfcActuatorTypeEnum" => "IfcBuildingControlsDomain",
			"IfcAlarmTypeEnum" => "IfcBuildingControlsDomain",
			"IfcControllerTypeEnum" => "IfcBuildingControlsDomain",
			"IfcFlowInstrumentTypeEnum" => "IfcBuildingControlsDomain",
			"IfcSensorTypeEnum" => "IfcBuildingControlsDomain",
			"IfcUnitaryControlElementTypeEnum" => "IfcBuildingControlsDomain",
			"IfcBenchmarkEnum" => "IfcConstraintResource",
			"IfcConstraintEnum" => "IfcConstraintResource",
			"IfcLogicalOperatorEnum" => "IfcConstraintResource",
			"IfcObjectiveEnum" => "IfcConstraintResource",
			"IfcConstructionEquipmentResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcConstructionMaterialResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcConstructionProductResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcCrewResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcLaborResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcSubContractResourceTypeEnum" => "IfcConstructionMgmtDomain",
			"IfcPerformanceHistoryTypeEnum" => "IfcControlExtension",
			"IfcArithmeticOperatorEnum" => "IfcCostResource",
			"IfcDataOriginEnum" => "IfcDateTimeResource",
			"IfcRecurrenceTypeEnum" => "IfcDateTimeResource",
			"IfcTaskDurationEnum" => "IfcDateTimeResource",
			"IfcTimeSeriesDataTypeEnum" => "IfcDateTimeResource",
			"IfcAudioVisualApplianceTypeEnum" => "IfcElectricalDomain",
			"IfcCableCarrierFittingTypeEnum" => "IfcElectricalDomain",
			"IfcCableCarrierSegmentTypeEnum" => "IfcElectricalDomain",
			"IfcCableFittingTypeEnum" => "IfcElectricalDomain",
			"IfcCableSegmentTypeEnum" => "IfcElectricalDomain",
			"IfcCommunicationsApplianceTypeEnum" => "IfcElectricalDomain",
			"IfcElectricApplianceTypeEnum" => "IfcElectricalDomain",
			"IfcElectricDistributionBoardTypeEnum" => "IfcElectricalDomain",
			"IfcElectricFlowStorageDeviceTypeEnum" => "IfcElectricalDomain",
			"IfcElectricGeneratorTypeEnum" => "IfcElectricalDomain",
			"IfcElectricMotorTypeEnum" => "IfcElectricalDomain",
			"IfcElectricTimeControlTypeEnum" => "IfcElectricalDomain",
			"IfcJunctionBoxTypeEnum" => "IfcElectricalDomain",
			"IfcLampTypeEnum" => "IfcElectricalDomain",
			"IfcLightFixtureTypeEnum" => "IfcElectricalDomain",
			"IfcMotorConnectionTypeEnum" => "IfcElectricalDomain",
			"IfcOutletTypeEnum" => "IfcElectricalDomain",
			"IfcProtectiveDeviceTrippingUnitTypeEnum" => "IfcElectricalDomain",
			"IfcProtectiveDeviceTypeEnum" => "IfcElectricalDomain",
			"IfcSolarDeviceTypeEnum" => "IfcElectricalDomain",
			"IfcSwitchingDeviceTypeEnum" => "IfcElectricalDomain",
			"IfcTransformerTypeEnum" => "IfcElectricalDomain",
			"IfcDocumentConfidentialityEnum" => "IfcExternalReferenceResource",
			"IfcDocumentStatusEnum" => "IfcExternalReferenceResource",
			"IfcBooleanOperator" => "IfcGeometricModelResource",
			"IfcBSplineCurveForm" => "IfcGeometryResource",
			"IfcBSplineSurfaceForm" => "IfcGeometryResource",
			"IfcKnotType" => "IfcGeometryResource",
			"IfcPreferredSurfaceCurveRepresentation" => "IfcGeometryResource",
			"IfcTransitionCode" => "IfcGeometryResource",
			"IfcTrimmingPreference" => "IfcGeometryResource",
			"IfcAirTerminalBoxTypeEnum" => "IfcHvacDomain",
			"IfcAirTerminalTypeEnum" => "IfcHvacDomain",
			"IfcAirToAirHeatRecoveryTypeEnum" => "IfcHvacDomain",
			"IfcBoilerTypeEnum" => "IfcHvacDomain",
			"IfcBurnerTypeEnum" => "IfcHvacDomain",
			"IfcChillerTypeEnum" => "IfcHvacDomain",
			"IfcCoilTypeEnum" => "IfcHvacDomain",
			"IfcCompressorTypeEnum" => "IfcHvacDomain",
			"IfcCondenserTypeEnum" => "IfcHvacDomain",
			"IfcCooledBeamTypeEnum" => "IfcHvacDomain",
			"IfcCoolingTowerTypeEnum" => "IfcHvacDomain",
			"IfcDamperTypeEnum" => "IfcHvacDomain",
			"IfcDuctFittingTypeEnum" => "IfcHvacDomain",
			"IfcDuctSegmentTypeEnum" => "IfcHvacDomain",
			"IfcDuctSilencerTypeEnum" => "IfcHvacDomain",
			"IfcEngineTypeEnum" => "IfcHvacDomain",
			"IfcEvaporativeCoolerTypeEnum" => "IfcHvacDomain",
			"IfcEvaporatorTypeEnum" => "IfcHvacDomain",
			"IfcFanTypeEnum" => "IfcHvacDomain",
			"IfcFilterTypeEnum" => "IfcHvacDomain",
			"IfcFlowMeterTypeEnum" => "IfcHvacDomain",
			"IfcHeatExchangerTypeEnum" => "IfcHvacDomain",
			"IfcHumidifierTypeEnum" => "IfcHvacDomain",
			"IfcMedicalDeviceTypeEnum" => "IfcHvacDomain",
			"IfcPipeFittingTypeEnum" => "IfcHvacDomain",
			"IfcPipeSegmentTypeEnum" => "IfcHvacDomain",
			"IfcPumpTypeEnum" => "IfcHvacDomain",
			"IfcSpaceHeaterTypeEnum" => "IfcHvacDomain",
			"IfcTankTypeEnum" => "IfcHvacDomain",
			"IfcTubeBundleTypeEnum" => "IfcHvacDomain",
			"IfcUnitaryEquipmentTypeEnum" => "IfcHvacDomain",
			"IfcValveTypeEnum" => "IfcHvacDomain",
			"IfcVibrationIsolatorTypeEnum" => "IfcHvacDomain",
			"IfcComplexPropertyTemplateTypeEnum" => "IfcKernel",
			"IfcObjectTypeEnum" => "IfcKernel",
			"IfcPropertySetTemplateTypeEnum" => "IfcKernel",
			"IfcSimplePropertyTemplateTypeEnum" => "IfcKernel",
			"IfcDirectionSenseEnum" => "IfcMaterialResource",
			"IfcLayerSetDirectionEnum" => "IfcMaterialResource",
			"IfcDerivedUnitEnum" => "IfcMeasureResource",
			"IfcSIPrefix" => "IfcMeasureResource",
			"IfcSIUnitName" => "IfcMeasureResource",
			"IfcUnitEnum" => "IfcMeasureResource",
			"IfcFireSuppressionTerminalTypeEnum" => "IfcPlumbingFireProtectionDomain",
			"IfcInterceptorTypeEnum" => "IfcPlumbingFireProtectionDomain",
			"IfcSanitaryTerminalTypeEnum" => "IfcPlumbingFireProtectionDomain",
			"IfcStackTerminalTypeEnum" => "IfcPlumbingFireProtectionDomain",
			"IfcWasteTerminalTypeEnum" => "IfcPlumbingFireProtectionDomain",
			"IfcNullStyle" => "IfcPresentationAppearanceResource",
			"IfcReflectanceMethodEnum" => "IfcPresentationAppearanceResource",
			"IfcSurfaceSide" => "IfcPresentationAppearanceResource",
			"IfcTextPath" => "IfcPresentationDefinitionResource",
			"IfcLightDistributionCurveEnum" => "IfcPresentationOrganizationResource",
			"IfcLightEmissionSourceEnum" => "IfcPresentationOrganizationResource",
			"IfcEventTriggerTypeEnum" => "IfcProcessExtension",
			"IfcEventTypeEnum" => "IfcProcessExtension",
			"IfcProcedureTypeEnum" => "IfcProcessExtension",
			"IfcSequenceEnum" => "IfcProcessExtension",
			"IfcTaskTypeEnum" => "IfcProcessExtension",
			"IfcWorkCalendarTypeEnum" => "IfcProcessExtension",
			"IfcWorkPlanTypeEnum" => "IfcProcessExtension",
			"IfcWorkScheduleTypeEnum" => "IfcProcessExtension",
			"IfcAssemblyPlaceEnum" => "IfcProductExtension",
			"IfcElementAssemblyTypeEnum" => "IfcProductExtension",
			"IfcElementCompositionEnum" => "IfcProductExtension",
			"IfcExternalSpatialElementTypeEnum" => "IfcProductExtension",
			"IfcGeographicElementTypeEnum" => "IfcProductExtension",
			"IfcGridTypeEnum" => "IfcProductExtension",
			"IfcInternalOrExternalEnum" => "IfcProductExtension",
			"IfcOpeningElementTypeEnum" => "IfcProductExtension",
			"IfcPhysicalOrVirtualEnum" => "IfcProductExtension",
			"IfcProjectionElementTypeEnum" => "IfcProductExtension",
			"IfcSpaceTypeEnum" => "IfcProductExtension",
			"IfcSpatialZoneTypeEnum" => "IfcProductExtension",
			"IfcTransportElementTypeEnum" => "IfcProductExtension",
			"IfcProfileTypeEnum" => "IfcProfileResource",
			"IfcReinforcingBarRoleEnum" => "IfcProfileResource",
			"IfcReinforcingBarSurfaceEnum" => "IfcProfileResource",
			"IfcSectionTypeEnum" => "IfcProfileResource",
			"IfcCurveInterpolationEnum" => "IfcPropertyResource",
			"IfcGeometricProjectionEnum" => "IfcRepresentationResource",
			"IfcGlobalOrLocalEnum" => "IfcRepresentationResource",
			"IfcBeamTypeEnum" => "IfcSharedBldgElements",
			"IfcBuildingElementProxyTypeEnum" => "IfcSharedBldgElements",
			"IfcBuildingSystemTypeEnum" => "IfcSharedBldgElements",
			"IfcChimneyTypeEnum" => "IfcSharedBldgElements",
			"IfcColumnTypeEnum" => "IfcSharedBldgElements",
			"IfcConnectionTypeEnum" => "IfcSharedBldgElements",
			"IfcCoveringTypeEnum" => "IfcSharedBldgElements",
			"IfcCurtainWallTypeEnum" => "IfcSharedBldgElements",
			"IfcDoorTypeEnum" => "IfcSharedBldgElements",
			"IfcDoorTypeOperationEnum" => "IfcSharedBldgElements",
			"IfcMemberTypeEnum" => "IfcSharedBldgElements",
			"IfcPlateTypeEnum" => "IfcSharedBldgElements",
			"IfcRailingTypeEnum" => "IfcSharedBldgElements",
			"IfcRampFlightTypeEnum" => "IfcSharedBldgElements",
			"IfcRampTypeEnum" => "IfcSharedBldgElements",
			"IfcRoofTypeEnum" => "IfcSharedBldgElements",
			"IfcShadingDeviceTypeEnum" => "IfcSharedBldgElements",
			"IfcSlabTypeEnum" => "IfcSharedBldgElements",
			"IfcStairFlightTypeEnum" => "IfcSharedBldgElements",
			"IfcStairTypeEnum" => "IfcSharedBldgElements",
			"IfcWallTypeEnum" => "IfcSharedBldgElements",
			"IfcWindowTypeEnum" => "IfcSharedBldgElements",
			"IfcWindowTypePartitioningEnum" => "IfcSharedBldgElements",
			"IfcDistributionChamberElementTypeEnum" => "IfcSharedBldgServiceElements",
			"IfcDistributionPortTypeEnum" => "IfcSharedBldgServiceElements",
			"IfcDistributionSystemEnum" => "IfcSharedBldgServiceElements",
			"IfcFlowDirectionEnum" => "IfcSharedBldgServiceElements",
			"IfcBuildingElementPartTypeEnum" => "IfcSharedComponentElements",
			"IfcDiscreteAccessoryTypeEnum" => "IfcSharedComponentElements",
			"IfcFastenerTypeEnum" => "IfcSharedComponentElements",
			"IfcMechanicalFastenerTypeEnum" => "IfcSharedComponentElements",
			"IfcFurnitureTypeEnum" => "IfcSharedFacilitiesElements",
			"IfcInventoryTypeEnum" => "IfcSharedFacilitiesElements",
			"IfcOccupantTypeEnum" => "IfcSharedFacilitiesElements",
			"IfcSystemFurnitureElementTypeEnum" => "IfcSharedFacilitiesElements",
			"IfcActionRequestTypeEnum" => "IfcSharedMgmtElements",
			"IfcCostItemTypeEnum" => "IfcSharedMgmtElements",
			"IfcCostScheduleTypeEnum" => "IfcSharedMgmtElements",
			"IfcPermitTypeEnum" => "IfcSharedMgmtElements",
			"IfcProjectOrderTypeEnum" => "IfcSharedMgmtElements",
			"IfcActionSourceTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcActionTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcAnalysisModelTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcAnalysisTheoryTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcLoadGroupTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcProjectedOrTrueLengthEnum" => "IfcStructuralAnalysisDomain",
			"IfcStructuralCurveActivityTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcStructuralCurveMemberTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcStructuralSurfaceActivityTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcStructuralSurfaceMemberTypeEnum" => "IfcStructuralAnalysisDomain",
			"IfcFootingTypeEnum" => "IfcStructuralElementsDomain",
			"IfcPileConstructionEnum" => "IfcStructuralElementsDomain",
			"IfcPileTypeEnum" => "IfcStructuralElementsDomain",
			"IfcReinforcingBarTypeEnum" => "IfcStructuralElementsDomain",
			"IfcReinforcingMeshTypeEnum" => "IfcStructuralElementsDomain",
			"IfcSurfaceFeatureTypeEnum" => "IfcStructuralElementsDomain",
			"IfcTendonAnchorTypeEnum" => "IfcStructuralElementsDomain",
			"IfcTendonTypeEnum" => "IfcStructuralElementsDomain",
			"IfcVoidingFeatureTypeEnum" => "IfcStructuralElementsDomain",
			"IfcChangeActionEnum" => "IfcUtilityResource",
			"IfcStateEnum" => "IfcUtilityResource",
			_ => ""
		};
	}


    private static string NewStringArray(string[] classes)
    {
        return @$"new[] {{ ""{string.Join("\", \"", classes)}"" }}";
    }

    private const string stub = """
		// generated code via ids-lib.codegen using Xbim.Essentials <PlaceHolderVersion>, any changes to this file will be lost at next regeneration

		using System.Linq;

		namespace IdsLib.IfcSchema;

		public partial class SchemaInfo
		{
			private static partial SchemaInfo GetClassesIFC2x3()
			{
				var schema = new SchemaInfo(IfcSchemaVersions.Ifc2x3) {
		<PlaceHolderIfc2x3>
				};
				return schema;
			}

			private static partial SchemaInfo GetClassesIFC4() 
			{
				var schema = new SchemaInfo(IfcSchemaVersions.Ifc4) {
		<PlaceHolderIfc4>
				};
				return schema;
			}

		    private static partial SchemaInfo GetClassesIFC4x3() 
			{
				var schema = new SchemaInfo(IfcSchemaVersions.Ifc4x3) {
		<PlaceHolderIfc4x3>
				};
				return schema;
			}
		}
		""";
}