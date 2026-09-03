using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace CoordinatesAudit
{
    public sealed class App : IExternalApplication
    {
        private const string RibbonTabName = "Coordinates Audit";
        private const string RibbonPanelName = "Coordinate Auditor";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                CreateRibbonTabIfMissing(application);
                RibbonPanel panel = GetOrCreatePanel(application);
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                var buttonData = new PushButtonData(
                    "CoordinatesAudit.OpenAuditor",
                    "Open\nAuditor",
                    assemblyPath,
                    "CoordinatesAudit.Commands.OpenAuditorCommand")
                {
                    ToolTip = "Open Revit Coordinate Auditor.",
                    LongDescription = "Starts Coordinate Auditor. M0 only verifies that the add-in loaded correctly."
                };

                panel.AddItem(buttonData);
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Coordinate Auditor", "The add-in could not be initialized.\n\n" + exception.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        private static void CreateRibbonTabIfMissing(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(RibbonTabName);
            }
            catch (ArgumentException)
            {
                // Revit throws when a loaded add-in has already created this tab.
            }
        }

        private static RibbonPanel GetOrCreatePanel(UIControlledApplication application)
        {
            RibbonPanel existingPanel = application.GetRibbonPanels(RibbonTabName)
                .FirstOrDefault(panel => panel.Name == RibbonPanelName);
            return existingPanel ?? application.CreateRibbonPanel(RibbonTabName, RibbonPanelName);
        }
    }
}
