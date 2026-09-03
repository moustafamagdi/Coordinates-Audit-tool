namespace CoordinatesAudit.Models
{
    public sealed class AuditComparisonResult
    {
        public string Status { get; set; }
        public double InternalHorizontalMm { get; set; }
        public double InternalVerticalMm { get; set; }
        public double ProjectBaseHorizontalMm { get; set; }
        public double ProjectBaseVerticalMm { get; set; }
        public double SurveyHorizontalMm { get; set; }
        public double SurveyVerticalMm { get; set; }
        public double RotationDegrees { get; set; }
        public string Reason { get; set; }
    }
}
