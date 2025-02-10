using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ThunderbirdLiveTileRunner;
using Windows.ApplicationModel.Background;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json.Linq;
using Windows.UI.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ThunderbirdLiveTile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //LoadHtmlContent();          
            Size windowSize = new Size(640, 480);
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(windowSize);
            ApplicationView.PreferredLaunchViewSize = windowSize;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            _ = LoadEmailDataAsync();
        }

        private async Task LoadEmailDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("Running MainPage.LoadEmailDataAsync()");
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

                    UnreadCountText.Text = $"Unread Email Count: {unreadCount}";
                    EmailListPanel.Children.Clear();
                    for (int i=0; i<Math.Min(emailSubjects.Count, 5); i++)
                    {
                        string subject = emailSubjects[i];
                        string author = emailAuthors[i];
                        string date = emailDates[i];

                        TextBlock emailText = new TextBlock
                        {
                            Text = $"{subject} from {author} sent on {date}",
                            FontSize = 14,
                            TextWrapping = TextWrapping.Wrap,
                            Padding = new Thickness(5, 5, 5, 5),
                            FontWeight = FontWeights.Light
                        };
                        EmailListPanel.Children.Add(emailText);;
                        UtilityMethods.UpdateLiveTile(unreadCount, emailAuthors, emailDates, emailSubjects);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in MainPage.LoadEmailDataAsync");
                System.Diagnostics.Debug.WriteLine(ex);
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
                return "{}";
            }
        }
        /*
       private void LoadHtmlContent()
       {
           string htmlContent = @"
           <!DOCTYPE html>
           <html>
           <head>
               <style>
                   body { font-family: Arial, sans-serif; padding: 20px; }
                   h2 { color: #0078D7; text-align: center; }
               </style>
           </head>
           <body>
               <h2>Thunderbird Live Tile</h2>
               <p>This UWP app acts as wrapper to allow Thunderbird to interact with Windows' Live Tile feature</p>
               <p>Launching this app will have refreshed the Live Tile.</p>
               <p>The process will continue to run in the background and update the Live Tile at an interval.</p>
               <p>Closing window in <span id='timer'>5</span> second(s).</p>
               <script>
                   var count = 5;
                   setInterval(function(){
                       document.getElementById('timer').innerHTML = count;
                       if(count > 1){
                           count = count - 1;
                       }
                   },1000);
               </script>
           </body>
           </html>"
           EmailWebView.NavigateToString(htmlContent); //derived from x:Name="" in template file
       }
       */
    }
}
