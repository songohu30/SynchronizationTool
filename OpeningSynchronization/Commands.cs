using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Functions;

namespace OpeningSynchronization
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Synchronization : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            App.Instance.SynchronizationTool.DisplayWindow();
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Settings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            return Result.Succeeded;
        }
    }

    class SynchronizationEvent : IExternalEventHandler
    {
        public void Execute(UIApplication uiapp)
        {
            try
            {
                SynchronizationTool synchronizationTool = App.Instance.SynchronizationTool;
                synchronizationTool.Document = uiapp.ActiveUIDocument.Document;
                if (synchronizationTool.ToolAction == ToolAction.CreateCloudResource)
                {
                    synchronizationTool.SetProjectOpenings();
                    synchronizationTool.SetProjectHosts();
                    synchronizationTool.CreateNewCloudResource();
                    TaskDialog.Show("Info", "Openigs have been uploaded to cloud!");
                }

                if (synchronizationTool.ToolAction == ToolAction.CheckWithCloud)
                {
                    synchronizationTool.SetProjectOpenings();
                    synchronizationTool.SetProjectHosts();
                    synchronizationTool.GetCloudResource();
                    synchronizationTool.CompareResources();
                    synchronizationTool.SetOpeningHostStatus();
                    synchronizationTool.CreateViewModel();
                    if (synchronizationTool.OpeningViewModels.Count == 0) TaskDialog.Show("Info", "All openings are up to date!");
                    synchronizationTool.SignalEvent.Set();
                }

                if (synchronizationTool.ToolAction == ToolAction.UpdateProject)
                {
                    synchronizationTool.UpdateProject();
                }

                if(synchronizationTool.ToolAction == ToolAction.ShowItem)
                {
                    ElementId id = synchronizationTool.SelectedId;
                    if(id.IntegerValue > 0)
                    {
                        Element element = uiapp.ActiveUIDocument.Document.GetElement(id);
                        if(element != null)
                        {
                            uiapp.ActiveUIDocument.Selection.SetElementIds(new List<ElementId>() { id });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }
        public string GetName()
        {
            return "SynchronizationTool";
        }
    }
}
