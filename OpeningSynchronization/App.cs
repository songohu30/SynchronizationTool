using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Functions;

namespace OpeningSynchronization
{
    public class App : IExternalApplication
    {
        public SynchronizationTool SynchronizationTool { get; set; }
        internal static App _app = null;
        public static App Instance
        {
            get { return _app; }
        }

        public Result OnStartup(UIControlledApplication application)
        {
            _app = this;
            IExternalEventHandler synchronizationEvent = new SynchronizationEvent();
            ExternalEvent synExEvent = ExternalEvent.Create(synchronizationEvent);
            SynchronizationTool = new SynchronizationTool();
            SynchronizationTool.SetEvent(synExEvent);

            string path = Assembly.GetExecutingAssembly().Location;
            RibbonPanel gtbSynchroPanel = application.CreateRibbonPanel("GTB-Openings");
            PushButtonData synchroButton = new PushButtonData("GTB-Synchronization", "Synchronize", path, "OpeningSynchronization.Synchronization");
            synchroButton.LargeImage = GetEmbeddedImage("Resources.gtb_sync.png");
            gtbSynchroPanel.AddItem(synchroButton);

            PushButtonData settingsButton = new PushButtonData("GTB-Settings", "Settings", path, "OpeningSynchronization.Settings");
            settingsButton.LargeImage = GetEmbeddedImage("Resources.gtb_settings.png");
            gtbSynchroPanel.AddItem(settingsButton);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(name);
                return BitmapFrame.Create(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
