using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class OpeningModel
    {       
        public ElementId ElementId { get; set; }
        public ElementId HostId { get; set; }
        public OpeningStatus OpeningStatus { get; set; }
        public HostStatus HostStatus { get; set; }
        public string GtbPairGuid { get; set; }
        public XYZ LocationPoint { get; set; }
        public OpeningType OpeningType { get; set; }
        public double Diameter { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Offset { get; set; }
        public double Depth { get; set; }

        private FamilyInstance _familyInstance;
        private OpeningModel _cloudOpening;

        public OpeningModel()
        {

        }

        public static OpeningModel Initialize(FamilyInstance familyInstance)
        {
            OpeningModel result = new OpeningModel();
            result._familyInstance = familyInstance;
            result.SetProperties();
            return result;
        }

        public bool InsertGuid()
        {
            bool result = true;
            Parameter gtbPairGuid = _familyInstance.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3"));
            if(gtbPairGuid != null)
            {
                string text = gtbPairGuid.AsString();
                if(String.IsNullOrEmpty(text) || String.IsNullOrWhiteSpace(text) || text == "")
                {
                    gtbPairGuid.Set(GtbPairGuid);
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public string SetGuid()
        {
            string result = "";
            Parameter gtbPairGuid = _familyInstance.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3"));
            if (gtbPairGuid != null)
            {
                string text = gtbPairGuid.AsString();
                if (String.IsNullOrEmpty(text) || String.IsNullOrWhiteSpace(text) || text == "")
                {
                    result = Guid.NewGuid().ToString();
                }
                else
                {
                    result = text;
                }
            }
            return result;
        }

        public void SetCloudOpening(OpeningModel cloudModel)
        {
            _cloudOpening = cloudModel;
        }
        
        private void SetProperties()
        {
            ElementId = _familyInstance.Id;
            if(_familyInstance.Host == null)
            {
                HostStatus = HostStatus.DeletedInProject;
                HostId = ElementId.InvalidElementId;
            }
            else
            {
                HostId = _familyInstance.Host.Id;
            }
            OpeningStatus = OpeningStatus.Cloud;
            GtbPairGuid = SetGuid();
            LocationPoint = (_familyInstance.Location as LocationPoint).Point;
            Parameter d = _familyInstance.LookupParameter("D");
            Parameter b = _familyInstance.LookupParameter("b");
            Parameter h = _familyInstance.LookupParameter("h");
            Parameter o = _familyInstance.LookupParameter("Cut Offset");
            Parameter depth = _familyInstance.LookupParameter("Depth");
            if (d != null && o != null)
            {
                Diameter = d.AsDouble();
                Offset = o.AsDouble();
                OpeningType = OpeningType.Round;
            }
            if (b != null && h != null && o != null)
            {
                Width = b.AsDouble();
                Height = h.AsDouble();
                Offset = o.AsDouble();
                OpeningType = OpeningType.Rectangular;
            }
            if (depth != null) Depth = depth.AsDouble();
        }

        public void InsertOpening(Document doc, FamilySymbol familySymbol)
        {
            Element host = doc.GetElement(HostId);

            if(host.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
            {
                Wall wall = host as Wall;
                Reference reference = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior).First();                
                ElementId levelId = host.LevelId;

                Face face = host.GetGeometryObjectFromReference(reference) as Face;
                IntersectionResult intResult = face.Project(LocationPoint);
                if (intResult == null) return; // important to prompt user if no face found


                double distance = intResult.Distance;
                if (distance > 0.001)
                {
                    reference = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior).First(); ;
                }

                FamilyInstance fi = doc.Create.NewFamilyInstance(reference, LocationPoint, new XYZ(0, 0, 0), familySymbol);
                Parameter parameter = fi.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM);
                if (parameter != null) parameter.Set(levelId);

                if (OpeningType == OpeningType.Round)
                {
                    fi.LookupParameter("D").Set(_cloudOpening.Diameter);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                if (OpeningType == OpeningType.Rectangular)
                {
                    fi.LookupParameter("b").Set(_cloudOpening.Width);
                    fi.LookupParameter("h").Set(_cloudOpening.Height);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                fi.LookupParameter("Depth").Set(Depth);
            }


            if (host.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors)
            {
                Floor floor = host as Floor;
                Reference reference = HostObjectUtils.GetTopFaces(floor).First();
                ElementId levelId = host.LevelId;
                FamilyInstance fi = doc.Create.NewFamilyInstance(reference, LocationPoint, new XYZ(0, 0, 0), familySymbol);
                Parameter parameter = fi.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM);
                if (parameter != null) parameter.Set(levelId);

                if (OpeningType == OpeningType.Round)
                {
                    fi.LookupParameter("D").Set(_cloudOpening.Diameter);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                if (OpeningType == OpeningType.Rectangular)
                {
                    fi.LookupParameter("b").Set(_cloudOpening.Width);
                    fi.LookupParameter("h").Set(_cloudOpening.Height);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                fi.LookupParameter("Depth").Set(Depth);
            }


            if (host.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Ceilings)
            {
                Ceiling ceiling = host as Ceiling;
                Reference reference = HostObjectUtils.GetTopFaces(ceiling).First();
                ElementId levelId = host.LevelId;
                FamilyInstance fi = doc.Create.NewFamilyInstance(reference, LocationPoint, new XYZ(0, 0, 0), familySymbol);
                Parameter parameter = fi.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM);
                if (parameter != null) parameter.Set(levelId);

                if (OpeningType == OpeningType.Round)
                {
                    fi.LookupParameter("D").Set(_cloudOpening.Diameter);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                if (OpeningType == OpeningType.Rectangular)
                {
                    fi.LookupParameter("b").Set(_cloudOpening.Width);
                    fi.LookupParameter("h").Set(_cloudOpening.Height);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                fi.LookupParameter("Depth").Set(_cloudOpening.Depth);
            }

            if (host.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Roofs)
            {
                Reference reference = null;
                ExtrusionRoof eRoof = host as ExtrusionRoof;
                List<Reference> references = HostObjectUtils.GetTopFaces(eRoof).ToList();
                foreach (Reference r in references)
                {
                    Face face = host.GetGeometryObjectFromReference(r) as Face;
                    //bool test = face.IsInside(new UV(LocationPoint.X, LocationPoint.Y));
                    IntersectionResult intResult = face.Project(LocationPoint);
                    if (intResult == null) continue;
                    double distance = intResult.Distance;
                    if (distance < 0.001)
                    {
                        reference = r;
                    }
                }
                if (reference == null) return;

                ElementId levelId = host.LevelId;
                FamilyInstance fi = doc.Create.NewFamilyInstance(reference, LocationPoint, new XYZ(0, 0, 0), familySymbol);
                Parameter parameter = fi.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM);
                if (parameter != null) parameter.Set(levelId);

                if (OpeningType == OpeningType.Round)
                {
                    fi.LookupParameter("D").Set(_cloudOpening.Diameter);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                if (OpeningType == OpeningType.Rectangular)
                {
                    fi.LookupParameter("b").Set(_cloudOpening.Width);
                    fi.LookupParameter("h").Set(_cloudOpening.Height);
                    fi.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
                    fi.get_Parameter(new Guid("f417eece-19f0-4253-9820-f876661146e3")).Set(GtbPairGuid);
                }
                fi.LookupParameter("Depth").Set(_cloudOpening.Depth);
            }
        }
        public void UpdateOpeningSize()
        {
            if (OpeningType == OpeningType.Round)
            {
                _familyInstance.LookupParameter("D").Set(_cloudOpening.Diameter);
                _familyInstance.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
            }
            if (OpeningType == OpeningType.Rectangular)
            {
                _familyInstance.LookupParameter("b").Set(_cloudOpening.Width);
                _familyInstance.LookupParameter("h").Set(_cloudOpening.Height);
                _familyInstance.LookupParameter("Cut Offset").Set(_cloudOpening.Offset);
            }
            _familyInstance.LookupParameter("Depth").Set(_cloudOpening.Depth);
        }
    }
}
