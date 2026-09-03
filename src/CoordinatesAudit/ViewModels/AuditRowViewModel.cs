using System.Globalization;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.ViewModels
{
    public sealed class AuditRowViewModel
    {
        public LinkInstanceData Link { get; set; }
        public AuditComparisonResult Comparison { get; set; }
        public string Status { get; set; }
        public string RowType { get; set; }
        public string Model { get; set; }
        public string Instance { get; set; }
        public string Loaded { get; set; }
        public string ProjectLocation { get; set; }
        public string ProjectBasePoint { get; set; }
        public string SurveyPoint { get; set; }
        public string InternalOrigin { get; set; }
        public string TrueNorth { get; set; }
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
                RowType = "Link",
                Model = link.LinkTypeName,
                Instance = link.InstanceName,
                Loaded = link.IsLoaded ? "Yes" : "No",
                ProjectLocation = link.CoordinateReport?.ProjectLocationName ?? "Unavailable",
                ProjectBasePoint = link.CoordinateReport?.ProjectBasePoint?.InternalPosition ?? "Unavailable",
                SurveyPoint = link.CoordinateReport?.SurveyPoint?.InternalPosition ?? "Unavailable",
                InternalOrigin = link.CoordinateReport?.InternalOriginPosition ?? "Unavailable",
                TrueNorth = link.CoordinateReport?.AngleToTrueNorth ?? "Unavailable",
                InternalDelta = FormatDistance(comparison.InternalHorizontalMm, comparison.InternalVerticalMm),
                ProjectBaseDelta = FormatDistance(comparison.ProjectBaseHorizontalMm, comparison.ProjectBaseVerticalMm),
                SurveyDelta = FormatDistance(comparison.SurveyHorizontalMm, comparison.SurveyVerticalMm),
                RotationDelta = comparison.Status == "UNAVAILABLE" ? "-" : comparison.RotationDegrees.ToString("0.###", CultureInfo.InvariantCulture) + "°",
                Reason = comparison.Reason,
                Details = BuildDetails(link, comparison)
            };
        }

        public static AuditRowViewModel CreateHost(HostCoordinateReport host, bool isReference)
        {
            return new AuditRowViewModel
            {
                Link = null,
                Comparison = null,
                Status = isReference ? "REFERENCE" : "HOST",
                RowType = "Host",
                Model = host.ModelTitle,
                Instance = "Current Model",
                Loaded = "Yes",
                ProjectLocation = host.ProjectLocationName,
                ProjectBasePoint = host.ProjectBasePoint.InternalPosition,
                SurveyPoint = host.SurveyPoint.InternalPosition,
                InternalOrigin = host.InternalOriginPosition,
                TrueNorth = host.AngleToTrueNorth,
                InternalDelta = isReference ? "H 0 / V 0 mm" : "-",
                ProjectBaseDelta = isReference ? "H 0 / V 0 mm" : "-",
                SurveyDelta = isReference ? "H 0 / V 0 mm" : "-",
                RotationDelta = isReference ? "0°" : "-",
                Reason = isReference ? "Current model is the selected reference." : "Current host model data.",
                Details = BuildHostDetails(host)
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

        private static string BuildHostDetails(HostCoordinateReport host)
        {
            return $"HOST MODEL\nModel: {host.ModelTitle}\nPath: {host.ModelPath}\nRevit: {host.RevitBuild}\nLength Unit: {host.LengthUnit}\nProject Location: {host.ProjectLocationName}\nAngle to True North: {host.AngleToTrueNorth}\n\nProject Base Point\nE/W: {host.ProjectBasePoint.EastWest}\nN/S: {host.ProjectBasePoint.NorthSouth}\nElevation: {host.ProjectBasePoint.Elevation}\nInternal Position: {host.ProjectBasePoint.InternalPosition}\nPinned: {(host.ProjectBasePoint.Pinned ? "Yes" : "No")}\n\nSurvey Point\nE/W: {host.SurveyPoint.EastWest}\nN/S: {host.SurveyPoint.NorthSouth}\nElevation: {host.SurveyPoint.Elevation}\nInternal Position: {host.SurveyPoint.InternalPosition}\nPinned: {(host.SurveyPoint.Pinned ? "Yes" : "No")}\n\nInternal Origin\nPosition: {host.InternalOriginPosition}";
        }
    }
}
