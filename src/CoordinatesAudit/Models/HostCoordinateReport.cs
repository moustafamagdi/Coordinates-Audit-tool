using Autodesk.Revit.DB;

namespace CoordinatesAudit.Models
{
    public sealed class HostCoordinateReport
    {
        public string ModelTitle { get; set; }
        public string ModelPath { get; set; }
        public string RevitBuild { get; set; }
        public string LengthUnit { get; set; }
        public string ProjectLocationName { get; set; }
        public string AngleToTrueNorth { get; set; }
        public string SharedOriginPosition { get; set; }
        public CoordinatePointData ProjectBasePoint { get; set; }
        public CoordinatePointData SurveyPoint { get; set; }
        public string InternalOriginPosition { get; set; }
        public XYZ InternalOriginRaw { get; set; }
        public double AngleToTrueNorthRaw { get; set; }
    }
}
