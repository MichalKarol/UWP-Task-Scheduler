using TaskScheduler.Models;
using TaskScheduler.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TaskScheduler
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<Job> JobsList
        {
            get
            {
               using(var context = new TaskSchedulerDbContext())
               {
                    return context.Jobs.ToList();
               }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e) {
            Job job = (Job)e.ClickedItem;

            localSettings.Values[Settings.JOBID] = job.Id;
            localSettings.Values[Settings.JOBACTION] = Settings.Actions.EDIT;

            Frame.Navigate(typeof(JobPage));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values[Settings.JOBID] = null;
            localSettings.Values[Settings.JOBACTION] = Settings.Actions.CREATE;

            Frame.Navigate(typeof(JobPage));
        }

        //

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    try {
        //        Cron.CronStructure str = Cron.ParseString(CronTextBox.Text);
        //        var a = str.NextOccurence.Take(5).ToList();
        //        var b = str.NextOccurence.Take(5).ToList();
        //        int x = 0;
        //    } catch (ArgumentException exc) {
        //        int x = 0;
        //    }
        //}
    }
}
