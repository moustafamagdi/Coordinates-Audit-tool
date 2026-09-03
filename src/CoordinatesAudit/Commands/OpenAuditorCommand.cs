using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CoordinatesAudit.Models;
using CoordinatesAudit.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Interop;
using CoordinatesAudit.Views;

namespace CoordinatesAudit.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public sealed class OpenAuditorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            if (uiDocument == null)
            {
                TaskDialog.Show("Coordinate Auditor", "Open a Revit project before running the audit.");
                return Result.Cancelled;
            }

            try
            {
                var reader = new HostCoordinateReader();
                HostCoordinateReport report = reader.Read(
                    uiDocument.Document,
                    commandData.Application.Application.VersionBuild);
                var linkDiscovery = new LinkDiscoveryService();
                IReadOnlyList<LinkInstanceData> links = linkDiscovery.Discover(uiDocument.Document);

                var window = new AuditWindow(report, links);
                new WindowInteropHelper(window).Owner = Process.GetCurrentProcess().MainWindowHandle;
                window.ShowDialog();
                return Result.Succeeded;
            }
            catch (System.Exception exception)
            {
                message = exception.Message;
                TaskDialog.Show("Coordinate Auditor", "The host model could not be audited.\n\n" + exception.Message);
                return Result.Failed;
            }
        }
    }
}
