using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

//using System.IO;
using Newtonsoft.Json.Linq;
//using Windows.UI.Notifications;
//using Windows.ApplicationModel.Background;
//using System.Threading.Tasks;
using Windows.Storage;
using System.Text;
using System.Collections.Generic;

namespace ThunderbirdLiveTileRunner
{
    public sealed class TileUpdaterTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        StringBuilder sb = new StringBuilder();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            System.Diagnostics.Debug.WriteLine("TileUpdaterTask started.");

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
                System.Diagnostics.Debug.WriteLine($"TileUpdaterTask Error: {ex.Message}");

            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("TileUpdaterTask completed.");
                _deferral.Complete();
            }
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
                StorageFile jsonFile = await subFolder.CreateFileAsync("thunderbird_unread.json",CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(jsonFile, "{}");
                return "{}";
            }
        }

    }
}
