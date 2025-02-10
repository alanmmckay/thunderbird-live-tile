using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.Background;

using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

using ThunderbirdLiveTileRunner;
using Windows.System;
using Windows.UI.ViewManagement;

namespace ThunderbirdLiveTile
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        StringBuilder sb = new StringBuilder();
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            RegisterBackgroundTask();
            this.Suspending += OnSuspending;
            string tileXmlString = @"
            <tile>
                <visual>
                    <binding template='TileWide'>
                        <text hint-style='title'>Thunderbird Live Tile</text>
                        <text hint-style='subtitle'>No Data Available</text>
                        <text hint-style='base'>Please start Thunderbird and ensure plugin is enabled.</text>
                    </binding>
                    <binding template='TileLarge'>
                        <text hint-style='title'>Thunderbird Live Tile</text>
                        <text hint-style='subtitle'>No Data Available</text>
                        <text hint-style='base'>Please start Thunderbird and ensure plugin is enabled.</text>
                    </binding>
                </visual>
            </tile>";

            XmlDocument tileXml = new XmlDocument();
            tileXml.LoadXml(tileXmlString);

            TileNotification notification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            _ = CreatePredefinedFolderAsync();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                string jsonContent = await ReadJsonFileAsync();

                if (jsonContent != "{}")
                {
                    JArray jsonData = JArray.Parse(jsonContent);

                    int unreadCount = jsonData[0].ToObject<int>();
                    JArray emails = jsonData[1] as JArray;

                    List<string> emailSubjects = new List<string>();
                    List<string> emailAuthors = new List<string>(); ;
                    List<string> emailDates = new List<string>(); ;

                    foreach (var email in emails)
                    {
                        emailSubjects.Add(email["subject"].ToString());
                        emailAuthors.Add(email["author"].ToString());
                        emailDates.Add(email["date"].ToString());
                    }

                    UtilityMethods.UpdateLiveTile(unreadCount, emailAuthors, emailDates, emailSubjects);

                }
            }
            catch (Exception ex)
            {

            }
            System.Diagnostics.Debug.WriteLine("App launched. Calling RegisterBackgroundTask...");
            sb.Append("App launched. Calling RegisterBackgroundTask...");
            RegisterBackgroundTask();
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
                Window.Current.Activate();
            }

            UtilityMethods.ProduceErrorLog(sb.ToString(), "error.txt");
            await Task.Delay(6000);
            //App.Current.Exit
            //await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private async void RegisterBackgroundTask()
        {
            string taskName = "ThunderbirdLiveTileRunner";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    System.Diagnostics.Debug.WriteLine("Background task already registered.");
                    sb.Append("Background task already registered.");
                    return;
                }
            }
            System.Diagnostics.Debug.WriteLine("Registering background task...");
            sb.Append("Registering background task...");
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.DeniedByUser || status == BackgroundAccessStatus.DeniedBySystemPolicy)
            {
                System.Diagnostics.Debug.WriteLine("Background task registration denied.");
                sb.Append("Background task registration denied.");
                return;
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = "ThunderbirdLiveTileRunner.TileUpdaterTask"
            };
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.Register();

            System.Diagnostics.Debug.WriteLine("Background task registered successfully.");
            sb.Append("Background task registered successfully.");
            UtilityMethods.ProduceErrorLog(sb.ToString(),"error.txt");
        }

        private async Task CreatePredefinedFolderAsync()
        {
            string appId = Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine($"UWP App ID: {appId}");
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                await localFolder.CreateFolderAsync("ThunderbirdData", CreationCollisionOption.OpenIfExists);
                await GetAppIdentifierAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating predefined folder.");
            }
        }

        private async Task<string> GetAppIdentifierAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync("label.txt", CreationCollisionOption.OpenIfExists);
            string content = await FileIO.ReadTextAsync(file);

            if (string.IsNullOrEmpty(content))
            {
                string defaultId = "ThunderbirdLiveTile";
                await FileIO.WriteTextAsync(file, defaultId);
                return defaultId;
            }
            return content;
        }

        private async Task<string> ReadJsonFileAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder subFolder = await localFolder.CreateFolderAsync("ThunderbirdData", CreationCollisionOption.OpenIfExists);
                StorageFile file = await subFolder.GetFileAsync("thunderbird_unread.json");
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder subFolder = await localFolder.CreateFolderAsync("ThunderbirdData", CreationCollisionOption.OpenIfExists);
                StorageFile jsonFile = await subFolder.CreateFileAsync("thunderbird_unread.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(jsonFile, "{}");
                return "{}";
            }
        }
    }
}
