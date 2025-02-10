using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

using Newtonsoft.Json.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
namespace ThunderbirdLiveTileRunner
{
    public sealed class UtilityMethods
    {
        public static async void ProduceErrorLog(string output_string, string fileName)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, output_string);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to write error in UtilityMethods.ProduceErrorLog");
            }
        }

        private static async Task UpdateLiveTileHelper(int unreadCount, IList<string> emailAuthors, IList<string> emailDates, IList<string> emailSubjects)
        {
            System.Diagnostics.Debug.WriteLine("Updating Live Tile...");
            //TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            //await Task.Delay(500);

            string tileXmlString = $@"
            <tile>
                <visual>
                    <binding template='TileWide'>
                        <text hint-style='body'>Unread Emails: {unreadCount}</text>";
            int index = 0;
            int count = Math.Min(emailSubjects.Count, Math.Min(emailAuthors.Count, emailDates.Count));
            while (index < count)
            {
                if (index < 2)
                {
                    tileXmlString += $"<text hint-style='caption'>{emailAuthors[index].ToString().Replace('<', '[').Replace('>', ']')}</text>";
                    tileXmlString += $"<text hint-style='captionSubtle'> - {emailDates[index].ToString().Replace('<', '[').Replace('>', ']')}</text>";
                }
                index += 1;
            }
            tileXmlString += $@"
                    </binding>
                    <binding template='TileLarge'>
                        <text hint-style='body'>Unread Emails: {unreadCount}</text>";
            index = 0;
            while (index < count)
            {
                if (index < 4)
                {
                    tileXmlString += $"<text hint-style='caption'>{emailAuthors[index].ToString().Replace('<', '[').Replace('>', ']')}</text>";
                    tileXmlString += $"<text hint-style='captionSubtle'> - {emailDates[index].ToString().Replace('<', '[').Replace('>', ']')}</text><text />";
                }
                index += 1;
            }

            tileXmlString += @"
                    </binding>
                </visual>
            </tile>";

            XmlDocument tileXml = new XmlDocument();
            System.Diagnostics.Debug.WriteLine(tileXmlString);

            tileXml.LoadXml(tileXmlString);

            TileNotification notification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

            System.Diagnostics.Debug.WriteLine(unreadCount.ToString());

            System.Diagnostics.Debug.WriteLine("Live Tile updated.");
            await Task.Delay(500);
        }

        public static async void UpdateLiveTile(int unreadCount, IList<string> emailAuthors, IList<string> emailDates, IList<string> emailSubjects)
        {
            await UpdateLiveTileHelper(unreadCount, emailAuthors, emailDates, emailSubjects);
        }

    }
}
