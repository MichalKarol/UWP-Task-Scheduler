using TaskScheduler.Models;
using TaskScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace TaskScheduler.Utils
{
    public class BackgroundTaskManager
    {
        public static BackgroundTaskManager instance = null;
        public static BackgroundTaskManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BackgroundTaskManager();
                }
                return instance;
            }
        }

        private Dictionary<Job, Cron.CronStructure> cronMapping = new Dictionary<Job, Cron.CronStructure>();
        private SortedDictionary<DateTime, List<Job>> scheduledJobsMapping = new SortedDictionary<DateTime, List<Job>>();
        
        AppListProvider appListProvider = AppListProvider.Instance;

        private BackgroundTaskManager()
        {

            using (var context = new TaskSchedulerDbContext())
            {
                foreach (Job job in context.Jobs)
                {
                    AddJob(job);
                }
            }
        }

        private void ScheduleJob(Job job, Cron.CronStructure cs)
        {
            DateTime current = DateTime.Now;
            while (true)
            {
                DateTime dateTime = cs.NextOccurence.First();
                Debug.WriteLine((uint)(dateTime - current).Minutes);

                if (scheduledJobsMapping.ContainsKey(dateTime))
                {
                    scheduledJobsMapping[dateTime].Add(job);
                }
                else
                {
                    scheduledJobsMapping.Add(dateTime, new List<Job>() { job });
                }

                if ((dateTime - current).TotalMinutes > 20)
                    break;
            }
        }

        public void AddJob(Job job)
        {
            Cron.CronStructure cs = Cron.ParseString(job.Cron);
            cronMapping.Add(job, cs);
            ScheduleJob(job, cs);
        }

        public void RemoveJob(Job job)
        {
            List<DateTime> times = scheduledJobsMapping.Keys.Where(x => scheduledJobsMapping[x].Contains(job)).ToList();
            times.ForEach(x =>
            {
                if (scheduledJobsMapping[x].Count == 1)
                {
                    scheduledJobsMapping.Remove(x);
                }
                else
                {
                    scheduledJobsMapping[x].Remove(job);
                }
                
            });
            cronMapping.Remove(job);
        }

        private async void RunJobsAtTime(List<Job> jobs, int milisecondsDelay)
        {
            await Task.Delay(milisecondsDelay);

            foreach (Job job in jobs)
            {
                Debug.WriteLine("RUN JOB: " + job.Name);
                Cron.CronStructure cs = cronMapping[job];
                ScheduleJob(job, cs);

                using (var context = new TaskSchedulerDbContext())
                {
                    UriAction[] uriActions = context.UriActionsForActionPredicate(x => x.JobId == job.Id);
                    NotificationAction[] notificationActions = context.NotificationActionsForActionPredicate(x => x.JobId == job.Id);
                    ApplicationAction[] applicationActions = context.ApplicationActionsForActionPredicate(x => x.JobId == job.Id);
                    try
                    {
                        foreach (var action in uriActions)
                        {
                            await Launcher.LaunchUriAsync(new Uri(action.Uri));
                        }

                        foreach (var action in notificationActions)
                        {
                            Toast.ShowToastNotification(job.Name, action.Text, action.Image, action.Audio, action.Timeout);
                        }

                        await appListProvider.IsDoneTask.ContinueWith(async (Task) =>
                        {

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                            {
                                foreach (var action in applicationActions)
                                {

                                    var appQuery = appListProvider.AppList.Where(x => x.Package.Id.FullName == action.ApplicationName);
                                    if (appQuery.Any())
                                    {
                                        AppEntry entry = appQuery.First();
                                        bool opr = await entry.Entry.LaunchAsync();
                                        while (!opr)
                                        {
                                            await Task.Delay(1000);
                                            opr = await entry.Entry.LaunchAsync();
                                        }
                                    }
                                }
                            });
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        public async void ScheduleJobs()
        {
            List<DateTime> times = scheduledJobsMapping.Keys.ToList();
            DateTime current = DateTime.Now;
            for(int i = 0; i < times.Count; i++)
            {
                DateTime time = times[i];

                // Skip those we could not run
                if (time < current)
                {
                    scheduledJobsMapping.Remove(time);
                    continue;
                }

                // Exit if more than 15 minutes
                if (time > current.AddMinutes(15))
                    break;

                // Now get to businnes
                List<Job> jobs = scheduledJobsMapping[time];
                scheduledJobsMapping.Remove(time);
                System.Action task = () => RunJobsAtTime(jobs, (int)(time - current).TotalMilliseconds);
                Task.Run(task);
            }
        }
    }
}
