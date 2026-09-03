using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CoordinatesAudit.Models;
using CoordinatesAudit.ViewModels;

namespace CoordinatesAudit.Services
{
    public sealed class CsvAuditExporter
    {
        public void Export(string filePath, HostCoordinateReport host, ReferenceModelOption reference,
            double horizontalToleranceMm, double verticalToleranceMm, double angularToleranceDegrees,
            IReadOnlyCollection<AuditRowViewModel> rows)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("An export path is required.", nameof(filePath));

            var csv = new StringBuilder();
            AppendRow(csv, "Coordinate Audit Report");
            AppendRow(csv, "Generated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            AppendRow(csv, "Host Model", host.ModelTitle);
            AppendRow(csv, "Host Path", host.ModelPath);
            AppendRow(csv, "Project Location", host.ProjectLocationName);
            AppendRow(csv, "Revit Build", host.RevitBuild);
            AppendRow(csv, "Reference", reference.DisplayName);
            AppendRow(csv, "Horizontal Tolerance (mm)", Format(horizontalToleranceMm));
            AppendRow(csv, "Vertical Tolerance (mm)", Format(verticalToleranceMm));
            AppendRow(csv, "Rotation Tolerance (deg)", Format(angularToleranceDegrees));
            csv.AppendLine();

            AppendRow(csv, "Status", "Model", "Link Instance", "Instance ID", "Loaded", "Link Status",
                "Path Type", "Path", "Attachment", "Workset", "Pinned",
                "Internal Origin Horizontal Delta (mm)", "Internal Origin Vertical Delta (mm)",
                "PBP Horizontal Delta (mm)", "PBP Vertical Delta (mm)",
                "Survey Horizontal Delta (mm)", "Survey Vertical Delta (mm)",
                "Rotation Delta (deg)", "Reason", "Project Location", "Angle to True North",
                "Total Translation", "Total Rotation", "Scale", "Mirrored");

            foreach (AuditRowViewModel row in rows.Where(item => item.Link != null))
            {
                LinkInstanceData link = row.Link;
                AuditComparisonResult result = row.Comparison;
                AppendRow(csv, result.Status, link.LinkTypeName, link.InstanceName, link.InstanceId,
                    link.IsLoaded ? "Yes" : "No", link.Status, link.ReferenceType, link.Path,
                    link.AttachmentType, link.Workset, link.Pinned,
                    ValueOrBlank(result, result.InternalHorizontalMm), ValueOrBlank(result, result.InternalVerticalMm),
                    ValueOrBlank(result, result.ProjectBaseHorizontalMm), ValueOrBlank(result, result.ProjectBaseVerticalMm),
                    ValueOrBlank(result, result.SurveyHorizontalMm), ValueOrBlank(result, result.SurveyVerticalMm),
                    ValueOrBlank(result, result.RotationDegrees), result.Reason,
                    link.CoordinateReport?.ProjectLocationName ?? string.Empty,
                    link.CoordinateReport?.AngleToTrueNorth ?? string.Empty,
                    link.TransformData?.TotalTranslation ?? string.Empty,
                    link.TransformData?.TotalRotation ?? string.Empty,
                    link.TransformData?.Scale ?? string.Empty,
                    link.TransformData?.Mirrored ?? string.Empty);
            }

            File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));
        }

        private static string ValueOrBlank(AuditComparisonResult result, double value) =>
            result.Status == "UNAVAILABLE" ? string.Empty : Format(value);

        private static string Format(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

        private static void AppendRow(StringBuilder csv, params string[] values) =>
            csv.AppendLine(string.Join(",", values.Select(Escape)));

        private static string Escape(string value)
        {
            string safe = value ?? string.Empty;
            bool requiresQuotes = safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            safe = safe.Replace("\"", "\"\"");
            return requiresQuotes ? "\"" + safe + "\"" : safe;
        }
    }
}
