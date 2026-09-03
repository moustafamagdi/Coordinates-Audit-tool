using System.Text;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Formatting
{
    public static class HostCoordinateReportFormatter
    {
        public static string Format(HostCoordinateReport report)
        {
            var output = new StringBuilder();
            output.AppendLine($"Model: {report.ModelTitle}");
            output.AppendLine($"Path: {report.ModelPath}");
            output.AppendLine($"Revit: {report.RevitBuild}");
            output.AppendLine($"Length unit: {report.LengthUnit}");
            output.AppendLine($"Project location: {report.ProjectLocationName}");
            output.AppendLine($"Angle to True North: {report.AngleToTrueNorth}");
            output.AppendLine($"Shared coordinates at Internal Origin: {report.SharedOriginPosition}");
            output.AppendLine();
            AppendPoint(output, report.ProjectBasePoint);
            output.AppendLine();
            AppendPoint(output, report.SurveyPoint);
            output.AppendLine();
            output.AppendLine("Internal Origin");
            output.AppendLine($"Position: {report.InternalOriginPosition}");
            return output.ToString();
        }

        private static void AppendPoint(StringBuilder output, CoordinatePointData point)
        {
            output.AppendLine(point.Name);
            output.AppendLine($"East/West: {point.EastWest}");
            output.AppendLine($"North/South: {point.NorthSouth}");
            output.AppendLine($"Elevation: {point.Elevation}");
            output.AppendLine($"Internal position: {point.InternalPosition}");
            output.AppendLine($"Pinned: {(point.Pinned ? "Yes" : "No")}");
        }
    }
}
