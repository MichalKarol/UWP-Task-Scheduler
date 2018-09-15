using TaskScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace TaskScheduler.Utils
{
    public class AppListProvider
    {
        public static AppListProvider instance = null;
        private TaskCompletionSource<bool> IsDoneTaskSource;
        public Task<bool> IsDoneTask;
        public List<AppEntry> AppList = new List<AppEntry>();

        public static void Init()
        {
            if (instance == null)
            {
                instance = new AppListProvider();
            }
        }

        public static AppListProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppListProvider();
                }
                return instance;
            }
        }

        private AppListProvider()
        {
            IsDoneTaskSource = new TaskCompletionSource<bool>();
            IsDoneTask = IsDoneTaskSource.Task;
            CoreApplication.MainView.CoreWindow.Dispatcher.RunIdleAsync((IdleDispatchedHandlerArgs) => ApplicationSearcher());
        }

        private async void ApplicationSearcher()
        {
            PackageManager packageManager = new PackageManager();
            Task<AppEntry>[] appListTasks = packageManager.FindPackagesForUser("").Where(x => !x.IsFramework && !x.IsResourcePackage).Select(async x =>
            {
                try
                {
                    IReadOnlyList<AppListEntry> apps = await x.GetAppListEntriesAsync();
                    if (apps == null || apps.Count == 0)
                        return null;

                    AppListEntry app = apps.First();
                    BitmapImage logo = new BitmapImage();

                    logo.SetSource(await app.DisplayInfo.GetLogo(new Size(16, 16)).OpenReadAsync());
                    return new AppEntry { Name = app.DisplayInfo.DisplayName, Logo = logo, Entry = app, Package = x };
                }
                catch (Exception e)
                {
                    return null;
                }

            }).ToArray();
            AppList = (await Task.WhenAll(appListTasks)).Where(x => x != null).OrderBy(x => x.Name).ToList();
            IsDoneTaskSource.SetResult(true);
        }
    }
}
