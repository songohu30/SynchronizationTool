using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class HostModel
    {
        public ElementId ElementId { get; set; }
        public HostStatus HostStatus { get; set; }
        public HostType HostType { get; set; } 
        public BoundingBoxXYZ BoundingBoxXYZ { get; set; }
        public double Thickness { get; set; }

        Element _element;
        Document _document;

        public HostModel()
        {

        }

        public static HostModel Initialize(Element element, Document document)
        {
            HostModel result = new HostModel();
            result._element = element;
            result._document = document;
            result.ElementId = element.Id;
            result.SetProperties();
            return result;
        }

        private void SetProperties()
        {
            BoundingBoxXYZ = _element.get_BoundingBox(null);
            if(_element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
            {
                HostType = HostType.Wall;
                Wall wall = _element as Wall;
                Thickness = Math.Round(UnitUtils.ConvertFromInternalUnits(wall.Width, DisplayUnitType.DUT_MILLIMETERS), 1);
            }
            if (_element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors)
            {
                HostType = HostType.Floor;
                Floor floor = _element as Floor;
                Thickness = Math.Round(UnitUtils.ConvertFromInternalUnits(floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS), 1);
            }
            if (_element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Ceilings)
            {
                HostType = HostType.Ceiling;
                Ceiling ceiling = _element as Ceiling;
                ElementId typeId = ceiling.GetTypeId();
                CeilingType cType = _document.GetElement(typeId) as CeilingType;
                Thickness = Math.Round(UnitUtils.ConvertFromInternalUnits(cType.get_Parameter(BuiltInParameter.CEILING_THICKNESS).AsDouble(), DisplayUnitType.DUT_MILLIMETERS), 1);
            }
            if (_element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Roofs)
            {
                HostType = HostType.Roof;
                Thickness = Math.Round(UnitUtils.ConvertFromInternalUnits(_element.get_Parameter(BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS), 1);
            }
        }
    }
}
