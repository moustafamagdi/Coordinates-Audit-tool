using Autodesk.Revit.DB;

namespace CoordinatesAudit.Models
{
    public sealed class ReferenceModelOption
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public XYZ InternalOrigin { get; set; }
        public XYZ ProjectBasePoint { get; set; }
        public XYZ SurveyPoint { get; set; }
        public double RotationRadians { get; set; }

        public override string ToString() => DisplayName;
    }
}
