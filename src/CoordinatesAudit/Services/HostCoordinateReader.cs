using System;
using Autodesk.Revit.DB;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Services
{
    public sealed class HostCoordinateReader
    {
        public HostCoordinateReport Read(Document document, string revitBuild)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (document.IsFamilyDocument) throw new InvalidOperationException("Coordinate Audit supports Revit project documents only.");

            BasePoint projectBasePoint = BasePoint.GetProjectBasePoint(document);
            BasePoint surveyPoint = BasePoint.GetSurveyPoint(document);
            InternalOrigin internalOrigin = InternalOrigin.Get(document);
            ProjectLocation projectLocation = document.ActiveProjectLocation;
            ProjectPosition sharedOrigin = projectLocation.GetProjectPosition(XYZ.Zero);

            return new HostCoordinateReport
            {
                ModelTitle = document.Title,
                ModelPath = GetModelPath(document),
                RevitBuild = revitBuild,
                LengthUnit = GetLengthUnitLabel(document),
                ProjectLocationName = projectLocation.Name,
                AngleToTrueNorth = FormatAngle(document, sharedOrigin.Angle),
                SharedOriginPosition = FormatPosition(document, new XYZ(sharedOrigin.EastWest, sharedOrigin.NorthSouth, sharedOrigin.Elevation)),
                ProjectBasePoint = ReadBasePoint(document, projectBasePoint, "Project Base Point"),
                SurveyPoint = ReadBasePoint(document, surveyPoint, "Survey Point"),
                InternalOriginPosition = FormatPosition(document, internalOrigin.Position)
            };
        }

        private static CoordinatePointData ReadBasePoint(Document document, BasePoint point, string name)
        {
            return new CoordinatePointData(
                name,
                FormatParameter(document, point.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM), SpecTypeId.Length, point.Position.X),
                FormatParameter(document, point.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM), SpecTypeId.Length, point.Position.Y),
                FormatParameter(document, point.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM), SpecTypeId.Length, point.Position.Z),
                FormatPosition(document, point.Position),
                point.Pinned);
        }

        private static string FormatParameter(Document document, Parameter parameter, ForgeTypeId specTypeId, double fallbackValue)
        {
            if (parameter != null && parameter.StorageType == StorageType.Double && parameter.HasValue)
            {
                string displayed = parameter.AsValueString();
                return string.IsNullOrWhiteSpace(displayed)
                    ? UnitFormatUtils.Format(document.GetUnits(), specTypeId, parameter.AsDouble(), false)
                    : displayed;
            }

            // In Revit 2024, Project Base Point offset parameters can report HasValue=false
            // even though BasePoint.Position contains the valid internal-coordinate offset.
            return UnitFormatUtils.Format(document.GetUnits(), specTypeId, fallbackValue, false);
        }

        private static string FormatPosition(Document document, XYZ position)
        {
            if (position == null) return "Unavailable";
            return $"X: {FormatLength(document, position.X)} | Y: {FormatLength(document, position.Y)} | Z: {FormatLength(document, position.Z)}";
        }

        private static string FormatLength(Document document, double value)
        {
            return UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, value, false);
        }

        private static string FormatAngle(Document document, double value)
        {
            return UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Angle, value, false);
        }

        private static string GetLengthUnitLabel(Document document)
        {
            ForgeTypeId unitTypeId = document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            return LabelUtils.GetLabelForUnit(unitTypeId);
        }

        private static string GetModelPath(Document document)
        {
            if (document.IsWorkshared)
            {
                ModelPath centralPath = document.GetWorksharingCentralModelPath();
                if (centralPath != null) return ModelPathUtils.ConvertModelPathToUserVisiblePath(centralPath);
            }

            return string.IsNullOrWhiteSpace(document.PathName) ? "Not saved" : document.PathName;
        }
    }
}
