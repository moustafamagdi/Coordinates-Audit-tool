using Autodesk.Revit.DB;

namespace CoordinatesAudit.Models
{
    public sealed class LinkTransformData
    {
        public string InstanceTranslation { get; set; }
        public string InstanceRotation { get; set; }
        public string TotalTranslation { get; set; }
        public string TotalRotation { get; set; }
        public string Scale { get; set; }
        public string Mirrored { get; set; }
        public string LinkedInternalOriginInHost { get; set; }
        public string LinkedProjectBasePointInHost { get; set; }
        public string LinkedSurveyPointInHost { get; set; }
        public XYZ LinkedInternalOriginInHostRaw { get; set; }
        public XYZ LinkedProjectBasePointInHostRaw { get; set; }
        public XYZ LinkedSurveyPointInHostRaw { get; set; }
        public double TotalRotationRadians { get; set; }
    }
}
