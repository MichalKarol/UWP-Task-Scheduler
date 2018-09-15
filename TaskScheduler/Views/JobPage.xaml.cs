using TaskScheduler.Models;
using TaskScheduler.Utils;
using TaskScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static TaskScheduler.Models.Action;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TaskScheduler
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JobPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        BackgroundTaskManager manager = BackgroundTaskManager.Instance;
        Func<Models.Action, bool> predicate = x => x.JobId == null;

        public string JobName { get { return NameBox.Text; } set { NameBox.Text = value ?? ""; } }
        public string JobCron { get { return CronBox.Text; } set { CronBox.Text = value ?? ""; } }
        public List<ActionViewModel> Actions
        {
            get
            {
                using (var context = new TaskSchedulerDbContext())
                {
                    return context.Actions.Where(predicate).Select(x => {
                        string name = "";
                        Symbol logo = Symbol.Globe;
                        switch (x.Type)
                        {
                            case ActionType.URI:
                                {
                                    name = "URI Action";
                                    logo = Symbol.Globe;
                                }; break;
                            case ActionType.NOTIFICATION:
                                {
                                    name = "Notification Action";
                                    logo = Symbol.Message;
                                }; break;
                            case ActionType.APPLICATION:
                                {
                                    name = "Application Action";
                                    logo = Symbol.CellPhone;
                                }; break;
                        }
                        return new ActionViewModel() { Name = name, Action = x, Logo = logo };
                    }).ToList();
                }
            }
        }

        static JobPage()
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values.Remove(Settings.JOBNAME);
                localSettings.Values.Remove(Settings.JOBCRON);
                using (var context = new TaskSchedulerDbContext())
                {
                    Func<Models.Action, bool> predicate = x => x.JobId == null;
                    context.Actions.RemoveRange(context.ActionsForActionPredicate(predicate));
                    context.UriActions.RemoveRange(context.UriActionsForActionPredicate(predicate));
                    context.NotificationActions.RemoveRange(context.NotificationActionsForActionPredicate(predicate));
                    context.ApplicationActions.RemoveRange(context.ApplicationActionsForActionPredicate(predicate));
                    context.SaveChanges();
                }
            };
        }

        public JobPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            using (var context = new TaskSchedulerDbContext())
            {
                if (localSettings.Values.ContainsKey(Settings.JOBNAME))
                    JobName = (string)localSettings.Values[Settings.JOBNAME];

                if (localSettings.Values.ContainsKey(Settings.JOBCRON))
                    JobCron = (string)localSettings.Values[Settings.JOBCRON];

                if ((int)localSettings.Values[Settings.JOBACTION] == Settings.Actions.CREATE)
                    return;

                Job job = context.Jobs.Where(x => x.Id == (int)localSettings.Values[Settings.JOBID]).First();
                predicate = x => x.JobId == job.Id|| x.JobId == null;

                if (!localSettings.Values.ContainsKey(Settings.JOBNAME))
                    JobName = job.Name;

                if (!localSettings.Values.ContainsKey(Settings.JOBCRON))
                    JobCron = job.Cron;
            }
        }


        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ActionViewModel action = (ActionViewModel)e.ClickedItem;
            localSettings.Values[Settings.ACTIONID] = action.Action.Id;
            localSettings.Values[Settings.ACTIONACTION] = Settings.Actions.EDIT;
            localSettings.Values[Settings.JOBNAME] = JobName;
            localSettings.Values[Settings.JOBCRON] = JobCron;
            Frame.Navigate(typeof(ActionPage), action.Action.Id);
        }

        private void AppBarButton_Add(object sender, RoutedEventArgs e)
        {
            localSettings.Values[Settings.ACTIONID] = null;
            localSettings.Values[Settings.ACTIONACTION] = Settings.Actions.CREATE;
            localSettings.Values[Settings.JOBNAME] = JobName;
            localSettings.Values[Settings.JOBCRON] = JobCron;
            Frame.Navigate(typeof(ActionPage));
        }

        private void AppBarButton_Save(object sender, RoutedEventArgs e)
        {
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
            SolidColorBrush defaultBrush = new SolidColorBrush();

            NameBox.BorderBrush = (String.IsNullOrEmpty(JobName) ? redBrush : defaultBrush);
            CronBox.BorderBrush = (String.IsNullOrEmpty(JobCron) ? redBrush : defaultBrush);
            ListView.BorderBrush = (ListView.Items.Count == 0 ? redBrush : defaultBrush);
            ListView.BorderThickness = (ListView.Items.Count == 0 ? CronBox.BorderThickness : new Thickness());

            if (String.IsNullOrEmpty(JobName) || String.IsNullOrEmpty(JobCron) || ListView.Items.Count == 0)
                return;

            

            using (var context = new TaskSchedulerDbContext())
            {
                try
                {
                    Cron.CronStructure cs = Cron.ParseString(JobCron);
                }
                catch (Exception exc)
                {
                    Debug.WriteLine("SHOW ERROR");
                    Debug.WriteLine(exc.Message);
                    return;
                }

                Job job = null;
                if ((int)localSettings.Values[Settings.JOBACTION] == Settings.Actions.CREATE)
                {
                    job = new Job();
                    context.Add(job);
                    context.SaveChanges();
                }
                else
                {
                    job = context.Jobs.Where(x => x.Id == (int)localSettings.Values[Settings.JOBID]).First();
                    if (job.Cron != JobCron)
                        manager.RemoveJob(job);
                }

                Models.Action[] actions = context.ActionsForActionPredicate(predicate);
                foreach (var action in actions)
                    action.JobId = job.Id;

                job.Name = JobName;
                job.Cron = JobCron;
                context.SaveChanges();
               
                manager.AddJob(job);
            }

            Frame.GoBack();
        }

        private void AppBarButton_Delete(object sender, RoutedEventArgs e)
        {
            if ((int)localSettings.Values[Settings.JOBACTION] == Settings.Actions.CREATE)
                return;

            using (var context = new TaskSchedulerDbContext())
            {

                Job job = context.Jobs.Where(x => x.Id == (int)localSettings.Values[Settings.JOBID]).First();
                context.Actions.RemoveRange(context.ActionsForActionPredicate(predicate));
                context.UriActions.RemoveRange(context.UriActionsForActionPredicate(predicate));
                context.NotificationActions.RemoveRange(context.NotificationActionsForActionPredicate(predicate));
                context.ApplicationActions.RemoveRange(context.ApplicationActionsForActionPredicate(predicate));
                context.Jobs.Remove(job);
                context.SaveChanges();
            }
            Frame.GoBack();
        }
    }
}
