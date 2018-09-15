using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskScheduler.ViewModels
{
    public class AppEntry
    {
        public string Name { get; set; }
        public BitmapImage Logo { get; set; }
        public AppListEntry Entry { get; set; }
        public Package Package { get; set; }
    }
}
