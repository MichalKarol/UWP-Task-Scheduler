using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static TaskScheduler.Models.Action;

namespace TaskScheduler.ViewModels
{
    public class ActionViewModel
    {
        public string Name { get; set; }
        public Symbol Logo { get; set; }
        public Models.Action Action { get; set; }
    }
}
