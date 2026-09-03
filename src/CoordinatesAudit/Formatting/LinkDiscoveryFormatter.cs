using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Formatting
{
    public static class LinkDiscoveryFormatter
    {
        public static string Format(IReadOnlyList<LinkInstanceData> links)
        {
            var output = new StringBuilder();
            if (links.Count == 0)
            {
                output.AppendLine("No Revit links were found in the active model.");
                return output.ToString();
            }

            for (int index = 0; index < links.Count; index++)
            {
                LinkInstanceData link = links[index];
                output.AppendLine($"[{index + 1}] {link.LinkTypeName}");
                output.AppendLine($"Type ID: {link.LinkTypeId}");
                output.AppendLine($"Instance: {link.InstanceName}");
                output.AppendLine($"Instance ID: {link.InstanceId}");
                output.AppendLine($"Status: {link.Status}");
                output.AppendLine($"Path type: {link.ReferenceType}");
                output.AppendLine($"Attachment: {link.AttachmentType}");
                output.AppendLine($"Workset: {link.Workset}");
                output.AppendLine($"Pinned: {link.Pinned}");
                output.AppendLine($"Path: {link.Path}");
                output.AppendLine($"Coordinate data: {link.CoordinateReadStatus}");
                AppendCoordinates(output, link.CoordinateReport);
                if (index < links.Count - 1) output.AppendLine();
            }

            return output.ToString();
        }

        public static string FormatSummary(IReadOnlyList<LinkInstanceData> links)
        {
            int placedInstances = links.Count(link => link.HasInstance);
            int loadedInstances = links.Count(link => link.HasInstance && link.IsLoaded);
            int unavailableInstances = placedInstances - loadedInstances;
            int typeOnlyRows = links.Count(link => !link.HasInstance);
            return $"Placed instances: {placedInstances}\nLoaded: {loadedInstances}\nUnavailable: {unavailableInstances}\nTypes without instances: {typeOnlyRows}";
        }

        private static void AppendCoordinates(StringBuilder output, HostCoordinateReport report)
        {
            if (report == null) return;

            output.AppendLine($"Linked project location: {report.ProjectLocationName}");
            output.AppendLine($"Angle to True North: {report.AngleToTrueNorth}");
            output.AppendLine($"Project Base Point E/W: {report.ProjectBasePoint.EastWest}");
            output.AppendLine($"Project Base Point N/S: {report.ProjectBasePoint.NorthSouth}");
            output.AppendLine($"Project Base Point Elevation: {report.ProjectBasePoint.Elevation}");
            output.AppendLine($"Project Base Point internal position: {report.ProjectBasePoint.InternalPosition}");
            output.AppendLine($"Survey Point E/W: {report.SurveyPoint.EastWest}");
            output.AppendLine($"Survey Point N/S: {report.SurveyPoint.NorthSouth}");
            output.AppendLine($"Survey Point Elevation: {report.SurveyPoint.Elevation}");
            output.AppendLine($"Survey Point internal position: {report.SurveyPoint.InternalPosition}");
            output.AppendLine($"Internal Origin: {report.InternalOriginPosition}");
        }
    }
}
