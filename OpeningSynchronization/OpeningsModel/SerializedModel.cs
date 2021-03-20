using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class SerializedModel
    {
        public int ElementId { get; set; }
        public int HostId { get; set; }
        public OpeningStatus OpeningStatus { get; set; }
        public HostStatus HostStatus { get; set; }
        public string GtbPairGuid { get; set; }
        public double LocationPointX { get; set; }
        public double LocationPointY { get; set; }
        public double LocationPointZ { get; set; }
        public OpeningType OpeningType { get; set; }
        public double Diameter { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Offset { get; set; }
        public double Depth { get; set; }
    }
}
