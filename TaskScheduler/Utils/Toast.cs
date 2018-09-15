using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace TaskScheduler
{
    class Toast
    {
        private static List<string> AudioList = new List<string>()
        {
            "ms-winsoundevent:Notification.Default",
            "ms-winsoundevent:Notification.IM",
            "ms-winsoundevent:Notification.Mail",
            "ms-winsoundevent:Notification.Reminder",
            "ms-winsoundevent:Notification.SMS",
            "ms-winsoundevent:Notification.Looping.Alarm",
            "ms-winsoundevent:Notification.Looping.Alarm2",
            "ms-winsoundevent:Notification.Looping.Alarm3",
            "ms-winsoundevent:Notification.Looping.Alarm4",
            "ms-winsoundevent:Notification.Looping.Alarm5",
            "ms-winsoundevent:Notification.Looping.Alarm6",
            "ms-winsoundevent:Notification.Looping.Alarm7",
            "ms-winsoundevent:Notification.Looping.Alarm8",
            "ms-winsoundevent:Notification.Looping.Alarm9",
            "ms-winsoundevent:Notification.Looping.Alarm10",
            "ms-winsoundevent:Notification.Looping.Call",
            "ms-winsoundevent:Notification.Looping.Call2",
            "ms-winsoundevent:Notification.Looping.Call3",
            "ms-winsoundevent:Notification.Looping.Call4",
            "ms-winsoundevent:Notification.Looping.Call5",
            "ms-winsoundevent:Notification.Looping.Call6",
            "ms-winsoundevent:Notification.Looping.Call7",
            "ms-winsoundevent:Notification.Looping.Call8",
            "ms-winsoundevent:Notification.Looping.Call9",
            "ms-winsoundevent:Notification.Looping.Call10",
        };

        public static void ShowToastNotification(string title, string text, string image, int? audio, int? timeout)
        {
            ToastContent content = new ToastContent()
            {
                Launch = "app-defined-string",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1
                            },

                            new AdaptiveText()
                            {
                                Text = text
                            },
                        },
                        
                    },
                },
            };

            if (!String.IsNullOrEmpty(image))
            {
                content.Visual.BindingGeneric.HeroImage = new ToastGenericHeroImage()
                {
                    Source = image
                };
            }

            content.Audio = new ToastAudio() { Src = new Uri(uriString: AudioList[audio.GetValueOrDefault(0)]) };

            ToastNotification notification = new ToastNotification(content.GetXml());
            if (timeout.HasValue)
                notification.ExpirationTime = DateTime.Now.AddSeconds(timeout.Value);

            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
