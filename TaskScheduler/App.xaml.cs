using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Management.Deployment;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Imaging;
using TaskScheduler.Utils;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;

namespace TaskScheduler
{
    sealed partial class App : Application
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        BackgroundTaskManager manager = null;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            using (var context= new TaskSchedulerDbContext())
            { 
                context.Database.Migrate();
            }

            // Start Background Tasks
            BackgroundTaskBuilder backgroundTaskBuildier = new BackgroundTaskBuilder();
            backgroundTaskBuildier.Name = "Reccuring";
            backgroundTaskBuildier.SetTrigger(new TimeTrigger(15U, false));
            BackgroundTaskRegistration task = backgroundTaskBuildier.Register();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                Window.Current.Activate();
            }
            AppListProvider.Init();
            manager = BackgroundTaskManager.Instance;
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (manager != null)
            {
                BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
                manager.ScheduleJobs();
                deferral.Complete();
            }
        }
    }
}
