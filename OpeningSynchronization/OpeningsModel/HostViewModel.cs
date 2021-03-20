using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class HostViewModel
    {
        public string ElementId { get; set; }
        public string HostType { get; set; }
        public string Thickness { get; set; }
        public string HostStatus { get; set; }

        public HostModel HostModel;

        public HostViewModel(HostModel hostModel)
        {
            HostModel = hostModel;
            SetProperties();
        }

        private void SetProperties()
        {
            ElementId = HostModel.ElementId.IntegerValue.ToString();
            HostType = Enum.GetName(typeof(HostType), HostModel.HostType);
            Thickness = Math.Round(HostModel.Thickness).ToString();
            HostStatus = Enum.GetName(typeof(HostStatus), HostModel.HostStatus);
        }
    }
}
