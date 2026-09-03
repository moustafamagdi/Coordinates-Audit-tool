using System;
using Autodesk.Revit.DB;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Services
{
    public sealed class CoordinateComparisonEngine
    {
        public AuditComparisonResult Compare(LinkInstanceData link, ReferenceModelOption reference, double horizontalToleranceMm, double verticalToleranceMm, double angularToleranceDegrees)
        {
            if (link?.TransformData == null || link.CoordinateReport == null)
            {
                return new AuditComparisonResult { Status = "UNAVAILABLE", Reason = link?.TransformReadStatus ?? "Coordinate data is unavailable." };
            }

            LinkTransformData transform = link.TransformData;
            Distance internalDelta = Measure(transform.LinkedInternalOriginInHostRaw, reference.InternalOrigin);
            Distance projectBaseDelta = Measure(transform.LinkedProjectBasePointInHostRaw, reference.ProjectBasePoint);
            Distance surveyDelta = Measure(transform.LinkedSurveyPointInHostRaw, reference.SurveyPoint);
            double rotationDelta = NormalizeDegrees(ToDegrees(transform.TotalRotationRadians - reference.RotationRadians));

            bool internalMatches = Within(internalDelta, horizontalToleranceMm, verticalToleranceMm);
            bool projectBaseMatches = Within(projectBaseDelta, horizontalToleranceMm, verticalToleranceMm);
            bool surveyMatches = Within(surveyDelta, horizontalToleranceMm, verticalToleranceMm);
            bool rotationMatches = Math.Abs(rotationDelta) <= angularToleranceDegrees;
            bool scaleMatches = IsUnitScale(transform);
            bool isPinned = string.Equals(link.Pinned, "Yes", StringComparison.OrdinalIgnoreCase);

            string status;
            string reason;
            if (!surveyMatches)
            {
                status = "FAIL";
                reason = "Shared position or elevation differs from the selected reference.";
            }
            else if (!rotationMatches)
            {
                status = "FAIL";
                reason = "Rotation differs from the selected reference.";
            }
            else if (!scaleMatches)
            {
                status = "FAIL";
                reason = "Link scale is not 1.0 on all axes.";
            }
            else if (transform.IsMirrored)
            {
                status = "FAIL";
                reason = "The link transform is mirrored.";
            }
            else if (!isPinned)
            {
                status = "WARNING";
                reason = "Shared coordinates align, but the link instance is not pinned." + BuildOriginInformation(internalMatches, projectBaseMatches);
            }
            else
            {
                status = "PASS";
                reason = "Shared position, elevation, rotation, scale, and mirroring checks passed." + BuildOriginInformation(internalMatches, projectBaseMatches);
            }

            return new AuditComparisonResult
            {
                Status = status,
                InternalHorizontalMm = internalDelta.HorizontalMm,
                InternalVerticalMm = internalDelta.VerticalMm,
                ProjectBaseHorizontalMm = projectBaseDelta.HorizontalMm,
                ProjectBaseVerticalMm = projectBaseDelta.VerticalMm,
                SurveyHorizontalMm = surveyDelta.HorizontalMm,
                SurveyVerticalMm = surveyDelta.VerticalMm,
                RotationDegrees = rotationDelta,
                Reason = reason
            };
        }

        private static Distance Measure(XYZ point, XYZ reference)
        {
            double dx = point.X - reference.X;
            double dy = point.Y - reference.Y;
            double dz = point.Z - reference.Z;
            return new Distance
            {
                HorizontalMm = UnitUtils.ConvertFromInternalUnits(Math.Sqrt(dx * dx + dy * dy), UnitTypeId.Millimeters),
                VerticalMm = Math.Abs(UnitUtils.ConvertFromInternalUnits(dz, UnitTypeId.Millimeters))
            };
        }

        private static bool Within(Distance distance, double horizontalToleranceMm, double verticalToleranceMm)
        {
            return distance.HorizontalMm <= horizontalToleranceMm && distance.VerticalMm <= verticalToleranceMm;
        }

        private static bool IsUnitScale(LinkTransformData transform)
        {
            const double tolerance = 1e-9;
            return Math.Abs(transform.ScaleX - 1.0) <= tolerance &&
                   Math.Abs(transform.ScaleY - 1.0) <= tolerance &&
                   Math.Abs(transform.ScaleZ - 1.0) <= tolerance;
        }

        private static string BuildOriginInformation(bool internalMatches, bool projectBaseMatches)
        {
            if (internalMatches && projectBaseMatches) return string.Empty;
            if (!internalMatches && !projectBaseMatches)
                return " INFO: Internal Origin and Project Base Point differ from the reference.";
            if (!internalMatches)
                return " INFO: Internal Origin differs from the reference.";
            return " INFO: Project Base Point differs from the reference.";
        }

        private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

        private static double NormalizeDegrees(double degrees)
        {
            while (degrees > 180.0) degrees -= 360.0;
            while (degrees < -180.0) degrees += 360.0;
            return degrees;
        }

        private sealed class Distance
        {
            public double HorizontalMm { get; set; }
            public double VerticalMm { get; set; }
        }
    }
}
