using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningsModel
{
    public class OpeningViewModel
    {
        public string ElementId { get; set; }
        public string Shape { get; set; }
        public string HostId { get; set; }
        public string OpeningStatus { get; set; }
        public string HostStatus { get; set; }
        public OpeningModel OpeningModel { get; set; }

        public OpeningViewModel(OpeningModel openingModel)
        {
            OpeningModel = openingModel;
            SetProperties();
        }

        private void SetProperties()
        {
            ElementId = OpeningModel.ElementId.IntegerValue.ToString();
            Shape = Enum.GetName(typeof(OpeningType), OpeningModel.OpeningType);
            HostId = OpeningModel.HostId.IntegerValue.ToString();
            OpeningStatus = Enum.GetName(typeof(OpeningStatus), OpeningModel.OpeningStatus);
            HostStatus = Enum.GetName(typeof(HostStatus), OpeningModel.HostStatus);
        }
    }
}
