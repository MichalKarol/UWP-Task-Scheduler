using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using TaskScheduler.Utils;
using static TaskScheduler.Models.Action;
using TaskScheduler.Models;
using Windows.UI;
using Windows.Phone.UI.Input;
using TaskScheduler.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TaskScheduler
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActionPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private AppListProvider appListProvider;

        public int Index { get { return ActionBox.SelectedIndex; } set { ActionBox.SelectedIndex = value; FlipView.SelectedIndex = value; } }

        public string Uri { get { return URIBox.Text; } set { URIBox.Text = value ?? ""; } }

        public string Text { get { return TextBox.Text; } set { TextBox.Text = value ?? ""; } }
        public string Image { get { return ImageBox.Text; } set { ImageBox.Text = value ?? ""; } }
        public int Audio { get { return AudioBox.SelectedIndex; } set { AudioBox.SelectedIndex = value; } }
        public string Timeout { get { return TimeoutBox.Text; } set { TimeoutBox.Text = value ?? ""; } }
        private List<string> AudioList = new List<string>()
        {
            "Default", "IM", "Mail", "Reminder", "SMS",
            "Alarm", "Alarm 2", "Alarm 3", "Alarm 4", "Alarm 5", "Alarm 6", "Alarm 7", "Alarm 8", "Alarm 9", "Alarm 10",
            "Call", "Call 2", "Call 3", "Call 4", "Call 5", "Call 6", "Call 7", "Call 8", "Call 9", "Call 10",
        };

        public ActionPage()
        {
            appListProvider = AppListProvider.Instance;
            
            this.InitializeComponent();
            ActionBox.SelectedIndex = 0;

            appListProvider.IsDoneTask.ContinueWith(async (result)=> 
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LoadingRing.Visibility = Visibility.Collapsed;
                    LoadedContent.Visibility = Visibility.Visible;
                    LoadedContent.ItemsSource = appListProvider.AppList;
                });
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AudioBox.ItemsSource = AudioList;

            if ((int)localSettings.Values[Settings.ACTIONACTION] == Settings.Actions.CREATE)
                return;

            using (var context = new TaskSchedulerDbContext())
            {
                Models.Action action = context.Actions.Where(x => x.Id == (int)localSettings.Values[Settings.ACTIONID]).First();

                switch (action.Type)
                {
                    case ActionType.URI:
                        {
                            UriAction uriAction = context.UriActions.Where(x => x.Id == action.ActionId).First();
                            Uri = uriAction.Uri;
                            Index = 0;
                        }; break;
                    case ActionType.NOTIFICATION:
                        {
                            NotificationAction notificationAction = context.NotificationActions.Where(x => x.Id == action.ActionId).First();
                            Text = notificationAction.Text;
                            Image = notificationAction.Image;
                            Audio = notificationAction.Audio.GetValueOrDefault(0);
                            if (notificationAction.Timeout.HasValue)
                                Timeout = notificationAction.Timeout.Value.ToString();
                            Index = 1;
                        }; break;
                    case ActionType.APPLICATION:
                        {
                            ApplicationAction applicationAction = context.ApplicationActions.Where(x => x.Id == action.ActionId).First();
                            appListProvider.IsDoneTask.ContinueWith(async (result) =>
                            {
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    bool ok = appListProvider.AppList.Where(x => x.Package.Id.FullName == applicationAction.ApplicationName).Any();
                                    if (ok)
                                    {
                                        AppEntry entry = appListProvider.AppList.Where(x => x.Package.Id.FullName == applicationAction.ApplicationName).First();
                                        LoadedContent.SelectedItem = entry;
                                        LoadedContent.ScrollIntoView(entry);
                                    }
                                        
                                });
                            });
                            Index = 2;
                        }; break;
                }
            }
        }

        private void AppBarButton_Save(object sender, RoutedEventArgs e)
        {
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
            SolidColorBrush defaultBrush = new SolidColorBrush();

            switch (Index)
            {
                case 0:
                    {
                        URIBox.BorderBrush = (String.IsNullOrEmpty(Uri) ? redBrush : defaultBrush);
                    }; break;
                case 1:
                    {
                        TextBox.BorderBrush = (String.IsNullOrEmpty(Text) ? redBrush : defaultBrush);
                    }; break;
                case 2:
                    {
                        LoadedContent.BorderBrush = (LoadedContent.SelectedItems.Count == 0 ? redBrush : defaultBrush);
                        LoadedContent.BorderThickness = (LoadedContent.SelectedItems.Count == 0 ? URIBox.BorderThickness : new Thickness());
                    }; break;
            }

            if (Index == 0 && String.IsNullOrEmpty(Uri) || Index == 1 && String.IsNullOrEmpty(Text) || Index == 2 && LoadedContent.SelectedItems.Count == 0)
                return;

            using (var context = new TaskSchedulerDbContext())
            {
                int actionId = 0;
                ActionType actionType = ActionType.URI;
                switch (Index)
                {
                    case 0:
                        {
                            UriAction act = new UriAction() { Uri = Uri };
                            context.UriActions.Add(act);
                            context.SaveChanges();
                            actionId = act.Id;
                            actionType = ActionType.URI;
                        }; break;
                    case 1:
                        {
                            NotificationAction act = new NotificationAction() { Text = Text, Image = Image, Audio = Audio };
                            if (!String.IsNullOrEmpty(Timeout))
                                act.Timeout = int.Parse(Timeout);
                            context.NotificationActions.Add(act);
                            context.SaveChanges();
                            actionId = act.Id;
                            actionType = ActionType.NOTIFICATION;
                        }; break;
                    case 2:
                        {
                            AppEntry appEntry = (AppEntry)LoadedContent.SelectedItem;
                            ApplicationAction act = new ApplicationAction() { ApplicationName = appEntry.Package.Id.FullName };
                            context.ApplicationActions.Add(act);
                            context.SaveChanges();
                            actionId = act.Id;
                            actionType = ActionType.APPLICATION;
                        }; break;
                }

                

                

                Models.Action action = new Models.Action() { ActionId = actionId, Type = actionType };
                if ((int)localSettings.Values[Settings.JOBACTION] == Settings.Actions.EDIT)
                {
                    Job job = context.Jobs.Where(x => x.Id == (int)localSettings.Values[Settings.JOBID]).First();
                    action.JobId = job.Id;
                }
                context.Actions.Add(action);
                context.SaveChanges();
                DeleteInitAction();
            }

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }

        private void DeleteInitAction()
        {
            if ((int)localSettings.Values[Settings.ACTIONACTION] == Settings.Actions.CREATE)
                return;

            using (var context = new TaskSchedulerDbContext())
            {
                int deleted_action_id = (int)localSettings.Values[Settings.ACTIONID];
                Models.Action action = context.Actions.Where(x => x.Id == deleted_action_id).First();

                switch (action.Type)
                {
                    case ActionType.URI:
                        {
                            UriAction uriAction = context.UriActions.Where(x => x.Id == action.ActionId).First();
                            context.UriActions.Remove(uriAction);
                        }; break;
                    case ActionType.NOTIFICATION:
                        {
                            NotificationAction notificationAction = context.NotificationActions.Where(x => x.Id == action.ActionId).First();
                            context.NotificationActions.Remove(notificationAction);
                        }; break;
                    case ActionType.APPLICATION:
                        {
                            ApplicationAction applicationActions = context.ApplicationActions.Where(x => x.Id == action.ActionId).First();
                            context.ApplicationActions.Remove(applicationActions);
                        }; break;
                }

                context.Actions.Remove(action);
                context.SaveChanges();
            }
        }

        private void AppBarButton_Delete(object sender, RoutedEventArgs e)
        {
            DeleteInitAction();
            Frame.GoBack();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            Image = file.Path;
        }

        private void ActionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Index = Index;
        }
    }
}
