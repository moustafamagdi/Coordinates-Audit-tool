using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CoordinatesAudit.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public sealed class OpenAuditorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string documentName = commandData.Application.ActiveUIDocument?.Document?.Title
                                  ?? "No active project";
            var dialog = new TaskDialog("Coordinate Auditor")
            {
                MainInstruction = "Coordinate Auditor loaded successfully.",
                MainContent = "Milestone 0 foundation is running correctly.\n\n" +
                              $"Active document: {documentName}\n\n" +
                              "No model data was changed.",
                CommonButtons = TaskDialogCommonButtons.Close,
                DefaultButton = TaskDialogResult.Close
            };
            dialog.Show();
            return Result.Succeeded;
        }
    }
}
