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

            string status;
            string reason;
            if (internalMatches && projectBaseMatches && surveyMatches && rotationMatches)
            {
                status = "PASS";
                reason = "All coordinate points and rotation match the selected reference within tolerance.";
            }
            else if (surveyMatches && rotationMatches)
            {
                status = "WARNING";
                reason = "Survey Point and rotation match, but the Internal Origin or Project Base Point differs.";
            }
            else
            {
                status = "FAIL";
                reason = !rotationMatches
                    ? "Rotation differs from the selected reference."
                    : "Survey Point position differs from the selected reference.";
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
