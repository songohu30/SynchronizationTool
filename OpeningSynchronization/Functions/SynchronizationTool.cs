using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpeningsModel;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace Functions
{
    public class SynchronizationTool
    {
        public ExternalEvent TheEvent { get; set; }
        public List<OpeningModel> ProjectOpenings { get; set; }
        public List<SerializedModel> ProjectOpeningsSerialized { get; set; }
        public List<OpeningModel> CloudOpenings { get; set; }
        public List<SerializedModel> CloudOpeningsSerialized { get; set; }
        public List<OpeningModel> ComparedOpenings { get; set; }
        public List<OpeningViewModel> OpeningViewModels { get; set; }
        public List<HostViewModel> HostViewModels { get; set; }
        public List<HostModel> ProjectHosts { get; set; }
        public List<SerializedHostInfo> ProjectHostsSerialized { get; set; }
        public List<HostModel> CloudHosts { get; set; }
        public List<SerializedHostInfo> CloudHostsSerialized { get; set; }
        public List<HostModel> ComparedHosts { get; set; }
        public ToolAction ToolAction { get; set; }
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        public FamilySymbol RoundSymbol { get; set; }
        public FamilySymbol RectangularSymbol { get; set; }
        public List<FamilySymbol> GenericFamilySymbols { get; set; }
        public Document Document { get; set; }
        public ElementId SelectedId { get; set; }

        public string SavePath = @"H:\Revit\Makros\Test\CloudSimulation\ConfigurationOpenings.json";
        public string SavePathHosts = @"H:\Revit\Makros\Test\CloudSimulation\ConfigurationHosts.json";


        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                SynchronizationWindow window = new SynchronizationWindow(this);
                window.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void SetEvent(ExternalEvent externalEvent)
        {
            TheEvent = externalEvent;
        }

        public void SetProjectOpenings()
        {
            ProjectOpenings = new List<OpeningModel>();
            List<FamilyInstance> _allOpenings = new List<FamilyInstance>();
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();

            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker") _allOpenings.Add(fi);
            }

            string info = "";
            using (Transaction tx = new Transaction(Document, "Adding GtbPairGuid"))
            {
                tx.Start();
                foreach (FamilyInstance fi in _allOpenings)
                {
                    OpeningModel openingModel = OpeningModel.Initialize(fi);
                    ProjectOpenings.Add(openingModel);
                    if (!openingModel.InsertGuid()) info += "ID: " + fi.Id.IntegerValue.ToString() + " - Can't add pair guid to instance!";
                }
                tx.Commit();
            }
            if (info != "") TaskDialog.Show("Error!", info);
        }

        public void SetGenericModelTypeList()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            GenericFamilySymbols = ficol.OfClass(typeof(FamilySymbol)).Select(x => x as FamilySymbol)
                                     .Where(y => y.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
        }

        public void CreateNewCloudResource()
        {
            SetSerializedProjectOpenings();
            SetSerializedProjectHosts();
            string output = JsonConvert.SerializeObject(ProjectOpeningsSerialized);
            string hosts = JsonConvert.SerializeObject(ProjectHostsSerialized);
            File.WriteAllText(SavePath, output, Encoding.ASCII);
            File.WriteAllText(SavePathHosts, hosts, Encoding.ASCII);
        }

        public void GetCloudResource()
        {
            string content = File.ReadAllText(SavePath, Encoding.ASCII);
            CloudOpeningsSerialized = JsonConvert.DeserializeObject<List<SerializedModel>>(content);
            SetCloudOpenings();
            string hosts = File.ReadAllText(SavePathHosts, Encoding.ASCII);
            CloudHostsSerialized = JsonConvert.DeserializeObject<List<SerializedHostInfo>>(hosts);
            SetCloudHosts();
        }

        public void SetOpeningHostStatus()
        {
            foreach (OpeningModel com in ComparedOpenings)
            {
                HostModel comparedHost = ComparedHosts.Where(e => e.ElementId.IntegerValue == com.HostId.IntegerValue).FirstOrDefault();
                if (comparedHost != null)
                {
                    com.HostStatus = comparedHost.HostStatus;
                }
            }

        }

        public void CompareResources()
        {
            ComparedOpenings = new List<OpeningModel>();
            ComparedHosts = new List<HostModel>();

            //loop through existing openings in project
            foreach (OpeningModel po in ProjectOpenings)
            {
                OpeningModel model = CloudOpenings.Where(e => e.GtbPairGuid == po.GtbPairGuid).FirstOrDefault();
                if(model == null)
                {
                    po.OpeningStatus = OpeningStatus.DeletedInCloud;
                    ComparedOpenings.Add(po);
                }
                else
                {
                    po.OpeningStatus = OpeningStatus.Unchanged;
                    if(po.LocationPoint.DistanceTo(model.LocationPoint) > 0.004) //around 1+ mm
                    {
                        po.OpeningStatus = OpeningStatus.Moved;
                    }

                    if(po.OpeningType == OpeningType.Round)
                    {
                        if(Math.Abs(po.Diameter - model.Diameter) > 0.003 || Math.Abs(po.Offset - model.Offset) > 0.003) //around 1- mm
                        {
                            if (po.OpeningStatus == OpeningStatus.Moved)
                            {
                                po.OpeningStatus = OpeningStatus.MovedAndResized;
                            }
                            else
                            {
                                po.OpeningStatus = OpeningStatus.Resized;
                            }
                        }
                    }

                    if (po.OpeningType == OpeningType.Rectangular)
                    {
                        if (Math.Abs(po.Height - model.Height) > 0.003 || Math.Abs(po.Width - model.Width) > 0.003 || Math.Abs(po.Offset - model.Offset) > 0.003) //around 1- mm
                        {
                            if (po.OpeningStatus == OpeningStatus.Moved)
                            {
                                po.OpeningStatus = OpeningStatus.MovedAndResized;
                            }
                            else
                            {
                                po.OpeningStatus = OpeningStatus.Resized;
                            }
                        }
                    }
                    po.SetCloudOpening(model);
                    ComparedOpenings.Add(po);
                }
            }

            //loop through cloud openings
            foreach (OpeningModel co in CloudOpenings)
            {
                OpeningModel model = ProjectOpenings.Where(e => e.GtbPairGuid == co.GtbPairGuid).FirstOrDefault();
                if(model == null)
                {
                    co.OpeningStatus = OpeningStatus.NewInCloud;
                    co.SetCloudOpening(co);
                    ComparedOpenings.Add(co);
                }
            }

            //loop existing hosts in project
            foreach (HostModel hm in ProjectHosts)
            {
                HostModel cloudModel = CloudHosts.Where(e => e.ElementId.IntegerValue == hm.ElementId.IntegerValue).FirstOrDefault();

                if(cloudModel == null)
                {
                    hm.HostStatus = HostStatus.DeletedInCloud;
                    ComparedHosts.Add(hm);
                }
                else
                {
                    hm.HostStatus = HostStatus.Unchanged;
                    if(Math.Abs(hm.Thickness - cloudModel.Thickness) > 0.003)
                    {
                        hm.HostStatus = HostStatus.ThicknessChanged;
                    }
                    if (hm.BoundingBoxXYZ.Min.DistanceTo(cloudModel.BoundingBoxXYZ.Min) > 0.001 || hm.BoundingBoxXYZ.Max.DistanceTo(cloudModel.BoundingBoxXYZ.Max) > 0.001)
                    {
                        if(hm.HostStatus == HostStatus.Unchanged)
                        {
                            hm.HostStatus = HostStatus.GeometryChanged;
                        }
                    }
                    ComparedHosts.Add(hm);
                }
            }

            //loop cloud hosts
            foreach (HostModel cloudHost in CloudHosts)
            {
                HostModel projectHost = ProjectHosts.Where(e => e.ElementId.IntegerValue == cloudHost.ElementId.IntegerValue).FirstOrDefault();
                if(projectHost == null)
                {
                    cloudHost.HostStatus = HostStatus.NewInCloud;
                    ComparedHosts.Add(cloudHost);
                }
            }
        }

        public void CreateViewModel()
        {
            OpeningViewModels = new List<OpeningViewModel>();
            HostViewModels = new List<HostViewModel>();
            foreach (OpeningModel opm in ComparedOpenings)
            {
                OpeningViewModel viewModel = new OpeningViewModel(opm);
                OpeningViewModels.Add(viewModel);
            }
            foreach (HostModel hm in ComparedHosts)
            {
                HostViewModel hostViewModel = new HostViewModel(hm);
                HostViewModels.Add(hostViewModel);
            }
        }

        public void UpdateProject()
        {
            using (Transaction tx = new Transaction(Document, "Update hosts from cloud"))
            {
                tx.Start();
                foreach (HostModel comparedHost in ComparedHosts)
                {
                    if(comparedHost.HostStatus == HostStatus.DeletedInCloud)
                    {
                        Document.Delete(comparedHost.ElementId);
                    }
                }
                tx.Commit();
            }
                using (Transaction tx = new Transaction(Document, "Update openings from cloud"))
            {
                tx.Start();
                foreach (OpeningModel comparedOpening in ComparedOpenings)
                {
                    //if deleted
                    if (comparedOpening.OpeningStatus == OpeningStatus.DeletedInCloud)
                    {
                        Document.Delete(comparedOpening.ElementId);
                    }

                    //if resized
                    if (comparedOpening.OpeningStatus == OpeningStatus.Resized)
                    {
                        comparedOpening.UpdateOpeningSize();
                    }

                    if (comparedOpening.OpeningType == OpeningType.Round)
                    {
                        if (comparedOpening.OpeningStatus == OpeningStatus.NewInCloud)
                        {
                            comparedOpening.InsertOpening(Document, RoundSymbol);
                        }
                        //other statuses
                    }

                    if (comparedOpening.OpeningType == OpeningType.Rectangular)
                    {
                        if (comparedOpening.OpeningStatus == OpeningStatus.NewInCloud)
                        {
                            comparedOpening.InsertOpening(Document, RectangularSymbol);
                        }
                        //other statuses
                    }
                }
                tx.Commit();
            }
        }

        public void SetProjectHosts()
        {
            ProjectHosts = new List<HostModel>();
            List<Element> walls = new FilteredElementCollector(Document).OfClass(typeof(Wall)).ToList();
            List<Element> floors = new FilteredElementCollector(Document).OfClass(typeof(Floor)).ToList();
            List<Element> ceilings = new FilteredElementCollector(Document).OfClass(typeof(Ceiling)).ToList();
            List<Element> roofs = new FilteredElementCollector(Document).OfClass(typeof(ExtrusionRoof)).ToList();

            foreach (Element e in walls)
            {
                HostModel hostModel = HostModel.Initialize(e, Document);
                hostModel.HostStatus = HostStatus.NewInCloud;
                ProjectHosts.Add(hostModel);
            }
            foreach (Element e in floors)
            {
                HostModel hostModel = HostModel.Initialize(e, Document);
                hostModel.HostStatus = HostStatus.NewInCloud;
                ProjectHosts.Add(hostModel);
            }
            foreach (Element e in ceilings)
            {
                HostModel hostModel = HostModel.Initialize(e, Document);
                hostModel.HostStatus = HostStatus.NewInCloud;
                ProjectHosts.Add(hostModel);
            }
            foreach (Element e in roofs)
            {
                HostModel hostModel = HostModel.Initialize(e, Document);
                hostModel.HostStatus = HostStatus.NewInCloud;
                ProjectHosts.Add(hostModel);
            }
        }

        private void SetSerializedProjectHosts()
        {
            ProjectHostsSerialized = new List<SerializedHostInfo>();
            foreach (HostModel hm in ProjectHosts)
            {
                SerializedHostInfo shi = new SerializedHostInfo();
                shi.ElementId = hm.ElementId.IntegerValue;
                shi.HostStatus = hm.HostStatus;
                shi.HostType = hm.HostType;
                XYZ boxMin = hm.BoundingBoxXYZ.Min;
                XYZ boxMax = hm.BoundingBoxXYZ.Max;
                shi.BoxMinX = boxMin.X;
                shi.BoxMinY = boxMin.Y;
                shi.BoxMinZ = boxMin.Z;
                shi.BoxMaxX = boxMax.X;
                shi.BoxMaxY = boxMax.Y;
                shi.BoxMaxZ = boxMax.Z;
                shi.Thickness = hm.Thickness;
                ProjectHostsSerialized.Add(shi);
            }
        }

        private void SetSerializedProjectOpenings()
        {
            ProjectOpeningsSerialized = new List<SerializedModel>();
            foreach (OpeningModel opm in ProjectOpenings)
            {
                SerializedModel serializedModel = new SerializedModel();
                serializedModel.ElementId = opm.ElementId.IntegerValue;
                serializedModel.HostId = opm.HostId.IntegerValue;
                serializedModel.OpeningStatus = opm.OpeningStatus;
                serializedModel.GtbPairGuid = opm.GtbPairGuid;
                serializedModel.LocationPointX = opm.LocationPoint.X;
                serializedModel.LocationPointY = opm.LocationPoint.Y;
                serializedModel.LocationPointZ = opm.LocationPoint.Z;
                serializedModel.OpeningType = opm.OpeningType;
                serializedModel.Diameter = opm.Diameter;
                serializedModel.Height = opm.Height;
                serializedModel.Width = opm.Width;
                serializedModel.Offset = opm.Offset;
                serializedModel.Depth = opm.Depth;
                ProjectOpeningsSerialized.Add(serializedModel);
            }
        }

        private void SetCloudOpenings()
        {
            CloudOpenings = new List<OpeningModel>();
            foreach (SerializedModel sm in CloudOpeningsSerialized)
            {
                OpeningModel opm = new OpeningModel();
                opm.ElementId = new ElementId(sm.ElementId);
                opm.HostId = new ElementId(sm.HostId);
                opm.OpeningStatus = sm.OpeningStatus;
                opm.GtbPairGuid = sm.GtbPairGuid;
                opm.LocationPoint = new XYZ(sm.LocationPointX, sm.LocationPointY, sm.LocationPointZ);
                opm.OpeningType = sm.OpeningType;
                opm.Diameter = sm.Diameter;
                opm.Height = sm.Height;
                opm.Width = sm.Width;
                opm.Offset = sm.Offset;
                opm.Depth = sm.Depth;
                CloudOpenings.Add(opm);
            }
        }

        private void SetCloudHosts()
        {
            CloudHosts = new List<HostModel>();
            foreach (SerializedHostInfo shi in CloudHostsSerialized)
            {
                HostModel hostModel = new HostModel();
                hostModel.ElementId = new ElementId(shi.ElementId);
                hostModel.HostStatus = shi.HostStatus;
                hostModel.HostType = shi.HostType;
                BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();
                XYZ minBox = new XYZ(shi.BoxMinX, shi.BoxMinY, shi.BoxMinZ);
                XYZ maxBox = new XYZ(shi.BoxMaxX, shi.BoxMaxY, shi.BoxMaxZ);
                boundingBoxXYZ.Min = minBox;
                boundingBoxXYZ.Max = maxBox;
                hostModel.BoundingBoxXYZ = boundingBoxXYZ;
                hostModel.Thickness = shi.Thickness;
                CloudHosts.Add(hostModel);
            }
        }
    }
}
