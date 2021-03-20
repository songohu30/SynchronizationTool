using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class SerializedHostInfo
    {
        public int ElementId { get; set; }
        public HostStatus HostStatus { get; set; }
        public HostType HostType { get; set; }
        public double BoxMinX { get; set; }
        public double BoxMinY { get; set; }
        public double BoxMinZ { get; set; }
        public double BoxMaxX { get; set; }
        public double BoxMaxY { get; set; }
        public double BoxMaxZ { get; set; }
        public double Thickness { get; set; }
    }
}
