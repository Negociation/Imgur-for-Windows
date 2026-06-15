using Imgur.Contracts;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;

namespace Imgur.Uwp.Services
{
    public class SystemNotificationService : ISystemNotificationService
    {
        private readonly ISystemInfoProvider _systemInfoProvider;
        private const string ToastTag = "imgur_upload";
        private const string ToastGroup = "imgur";

        public SystemNotificationService(ISystemInfoProvider systemInfoProvider)
        {
            _systemInfoProvider = systemInfoProvider;
        }

        // ── Em Andamento ───────────────────────────────────────
        public void ShowUploadInProgress(string title= null)
        {
            if (_systemInfoProvider.IsMobile())
            {
                ShowMobileSpinner();
                return;
            }

            ShowToastProgress();
        }

        // ── Concluído ──────────────────────────────────────────
        public void ShowUploadCompleted()
        {
            if (_systemInfoProvider.IsMobile())
            {
                HideMobileSpinner();
                ShowMobileToast();
                return;
            }

            UpdateToastCompleted();
        }

        // ── Mobile — Spinner ───────────────────────────────────
        private async void ShowMobileSpinner()
        {
            try
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
                statusBar.ProgressIndicator.Text = resourceLoader.GetString("UploadNotificationSpinnerTitle");
                statusBar.ProgressIndicator.ProgressValue = null;
                await statusBar.ProgressIndicator.ShowAsync();
                
            }
            catch { }
        }

        // -- Sharetarget Blocked --

        public void ShowShareTargetBlocked()
        {
            try
            {
                // Remove toast de progresso
                ToastNotificationManager.History.Remove(ToastTag, ToastGroup);
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var title = resourceLoader.GetString("UploadCancelledNotificationTitle");
                var desc = resourceLoader.GetString("UploadCancelledApiKeysMissingDesc");
   
                // Mostra toast de conclusão
                var toastXml = $@"
                <toast duration='short'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{desc}</text>
                        </binding>
                    </visual>
                </toast>";

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXml);

                var toast = new ToastNotification(xmlDoc)
                {
                    Tag = ToastTag,
                    Group = ToastGroup
                };

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch { }
        }

        private async void HideMobileSpinner()
        {
            try
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ProgressIndicator.HideAsync();
                await statusBar.HideAsync();
            }
            catch { }
        }

        // ── Mobile — Toast simples ─────────────────────────────
        private void ShowMobileToast()
        {
            try
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var title = resourceLoader.GetString("UploadCompletedNotificationTitle");
                var desc = resourceLoader.GetString("UploadCompletedDesc");

                // Mostra toast de conclusão
                var toastXml = $@"
                <toast duration='short'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{desc}</text>
                        </binding>
                    </visual>
                </toast>";

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXml);

                ToastNotificationManager
                    .CreateToastNotifier()
                    .Show(new ToastNotification(xmlDoc));
            }
            catch { }
        }

        // ── PC — Toast com spinner indeterminado ───────────────
        private void ShowToastProgress()
        {
            try
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                var title = resourceLoader.GetString("UploadStartedNotificationTitle");
                var desc = resourceLoader.GetString("UploadOngoingDesc");

                var toastXml = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{desc}</text>
                            <progress value='indeterminate' status='Uploading...'/>
                        </binding>
                    </visual>
                </toast>";

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXml);

                var toast = new ToastNotification(xmlDoc)
                {
                    Tag = ToastTag,
                    Group = ToastGroup,
                    ExpiresOnReboot = true
                };

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch { }
        }

        // ── PC — Atualiza toast pra concluído ──────────────────
        private void UpdateToastCompleted()
        {
            try
            {
                // Remove toast de progresso
                ToastNotificationManager.History.Remove(ToastTag, ToastGroup);

                var resourceLoader = ResourceLoader.GetForCurrentView();
                var title = resourceLoader.GetString("UploadStartedNotificationTitle");
                var desc = resourceLoader.GetString("UploadCompletedDesc");

                var toastXml = $@"
                <toast duration='short'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{desc}</text>
                        </binding>
                    </visual>
                </toast>";

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXml);

                var toast = new ToastNotification(xmlDoc)
                {
                    Tag = ToastTag,
                    Group = ToastGroup
                };

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch { }
        }

        public void ShowUploadFailed()
        {
            //
        }
    }
}