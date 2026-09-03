using System.Globalization;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.ViewModels
{
    public sealed class AuditRowViewModel
    {
        public LinkInstanceData Link { get; set; }
        public AuditComparisonResult Comparison { get; set; }
        public string Status { get; set; }
        public string Model => Link.LinkTypeName;
        public string Instance => Link.InstanceName;
        public string Loaded => Link.IsLoaded ? "Yes" : "No";
        public string InternalDelta { get; set; }
        public string ProjectBaseDelta { get; set; }
        public string SurveyDelta { get; set; }
        public string RotationDelta { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }

        public static AuditRowViewModel Create(LinkInstanceData link, AuditComparisonResult comparison)
        {
            return new AuditRowViewModel
            {
                Link = link,
                Comparison = comparison,
                Status = comparison.Status,
                InternalDelta = FormatDistance(comparison.InternalHorizontalMm, comparison.InternalVerticalMm),
                ProjectBaseDelta = FormatDistance(comparison.ProjectBaseHorizontalMm, comparison.ProjectBaseVerticalMm),
                SurveyDelta = FormatDistance(comparison.SurveyHorizontalMm, comparison.SurveyVerticalMm),
                RotationDelta = comparison.Status == "UNAVAILABLE" ? "-" : comparison.RotationDegrees.ToString("0.###", CultureInfo.InvariantCulture) + "°",
                Reason = comparison.Reason,
                Details = BuildDetails(link, comparison)
            };
        }

        private static string FormatDistance(double horizontal, double vertical)
        {
            return $"H {horizontal:0.##} / V {vertical:0.##} mm";
        }

        private static string BuildDetails(LinkInstanceData link, AuditComparisonResult comparison)
        {
            return $"Model: {link.LinkTypeName}\nInstance: {link.InstanceName}\nInstance ID: {link.InstanceId}\nStatus: {link.Status}\nPath: {link.Path}\nWorkset: {link.Workset}\nPinned: {link.Pinned}\n\nComparison: {comparison.Status}\nReason: {comparison.Reason}\n\nProject Location: {link.CoordinateReport?.ProjectLocationName ?? "Unavailable"}\nAngle to True North: {link.CoordinateReport?.AngleToTrueNorth ?? "Unavailable"}\nProject Base Point: {link.CoordinateReport?.ProjectBasePoint?.InternalPosition ?? "Unavailable"}\nSurvey Point: {link.CoordinateReport?.SurveyPoint?.InternalPosition ?? "Unavailable"}\nInternal Origin: {link.CoordinateReport?.InternalOriginPosition ?? "Unavailable"}\n\nTotal Translation: {link.TransformData?.TotalTranslation ?? "Unavailable"}\nTotal Rotation: {link.TransformData?.TotalRotation ?? "Unavailable"}\nScale: {link.TransformData?.Scale ?? "Unavailable"}\nMirrored: {link.TransformData?.Mirrored ?? "Unavailable"}";
        }
    }
}
