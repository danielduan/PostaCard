using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace PostaCard
{
    class Image
    {
        public string image { get; set; }
    }

    public partial class DesignPage : PhoneApplicationPage
    {
        BitmapImage im;
        String imageFilename;

        String sendType = "Email";

        public DesignPage()
        {
            InitializeComponent();
            progressPage.Visibility = System.Windows.Visibility.Collapsed;
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            /*
            var localFile = "isostore:/Shared/";
             * */
            imageFilename = NavigationContext.QueryString["imageFilename"];
            //localFile += imageFilename;
            /*
            imgPostcard.ImageFailed += (s, e1) => { Debug.Assert(false); };
            imgPostcard.ImageOpened += (s, e2) => { Debug.Assert(false); };
            */

            // imgPostcard.Source = new BitmapImage(new Uri(imageFilename, UriKind.Absolute));


             // The image will be read from isolated storage into the following byte array 
            byte[] data; 
 
            // Read the entire image in one go into a byte array 
            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Open the file - error handling omitted for brevity 
                    // Note: If the image does not exist in isolated storage the following exception will be generated: 
                    // System.IO.IsolatedStorage.IsolatedStorageException was unhandled  
                    // Message=Operation not permitted on IsolatedStorageFileStream  
                    using (IsolatedStorageFileStream isfs = isf.OpenFile(imageFilename, FileMode.Open, FileAccess.Read))
                    {
                        // Allocate an array large enough for the entire file 
                        data = new byte[isfs.Length];

                        // Read the entire file and then close it 
                        isfs.Read(data, 0, data.Length);
                        isfs.Close();
                    }
                }

                // Create memory stream and bitmap 
                MemoryStream ms = new MemoryStream(data);
                BitmapImage bi = new BitmapImage();
                im = bi;
                bi.SetSource(ms);


                imgPostcard.Source = bi;
            }
            catch (Exception e1)
            {
                // handle the exception 
                Debug.WriteLine(e1.Message);
            } 
            //txtboxEmail.Text = imageFilename;
        }

        public void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            sendType = "Email";
            panelPostcardText.Visibility = System.Windows.Visibility.Collapsed;
            panelPostcardTextBox.Visibility = System.Windows.Visibility.Collapsed;
            panelEmailText.Visibility = System.Windows.Visibility.Visible;
            panelEmailTextBox.Visibility = System.Windows.Visibility.Visible;

            SolidColorBrush darkbrush = new SolidColorBrush();
            darkbrush.Color = Color.FromArgb(255, 189, 195, 199);
            gridPostcard.Background = darkbrush;
            gridEmail.Background = null;
        }

        public void PostcardButton_Click(object sender, RoutedEventArgs e)
        {
            sendType = "Postcard";
            panelEmailText.Visibility = System.Windows.Visibility.Collapsed;
            panelEmailTextBox.Visibility = System.Windows.Visibility.Collapsed;
            panelPostcardText.Visibility = System.Windows.Visibility.Visible;
            panelPostcardTextBox.Visibility = System.Windows.Visibility.Visible;

            SolidColorBrush darkbrush = new SolidColorBrush();
            darkbrush.Color = Color.FromArgb(255, 189, 195, 199);
            gridEmail.Background = darkbrush;
            gridPostcard.Background = null;

            
        }

        public async Task<String> GetPDF()
        {
            byte[] bytearray = null;

            using (MemoryStream ms = new MemoryStream())
            {
                    WriteableBitmap wbitmp = new WriteableBitmap(im);

                    wbitmp.SaveJpeg(ms, 1200, 800, 0, 80);
                    bytearray = ms.ToArray();
            }
            string str = Convert.ToBase64String(bytearray);


            var image = new Image(){image = str};

            //var url = "http://cardsend.herokuapp.com/mobile/v1/pdf";
            var url = "http://cardsend.herokuapp.com/";

            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await client.PostAsJsonAsync("mobile/v1/pdf", image);
                if (response.IsSuccessStatusCode)
                {
                    // Get the URI of the created resource.
                    //Uri gizmoUrl = response.Headers.Location;
                }
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async void SendPostCard(List<KeyValuePair<string, string>> postcard)
        {
            var url = "http://cardsend.herokuapp.com/mobile/v1/card/send";
            var httpClient = new HttpClient(new HttpClientHandler());
            HttpResponseMessage response = await httpClient.PostAsync(url, new FormUrlEncodedContent(postcard));
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            progressPage.Visibility = System.Windows.Visibility.Collapsed;
            SendButton.IsEnabled = true;
            MessageBox.Show(responseString);
        }

        public async void SendEmail(List<KeyValuePair<string, string>> email)
        {
            var url = "http://cardsend.herokuapp.com/mobile/v1/email/send";
            var httpClient = new HttpClient(new HttpClientHandler());
            HttpResponseMessage response = await httpClient.PostAsync(url, new FormUrlEncodedContent(email));
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            progressPage.Visibility = System.Windows.Visibility.Collapsed;
            SendButton.IsEnabled = true;
            MessageBox.Show(responseString);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            progressPage.Visibility = System.Windows.Visibility.Visible;
            SendButton.IsEnabled = false;

            var pdfurl = await GetPDF();

            if (sendType == "Email")
            {
                var email = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("to_name", txtboxName.Text),
                            new KeyValuePair<string, string>("from_name", "PostaCard"),
                            new KeyValuePair<string, string>("to_email", txtboxEmail.Text),
                            new KeyValuePair<string, string>("message", txtboxMessage.Text),
                            new KeyValuePair<string, string>("card_design", pdfurl),
                            new KeyValuePair<string, string>("apikey", "11111")
                        };
                SendEmail(email);
            }
            else if (sendType == "Postcard") {
                var postcard = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("to_name", txtboxName.Text),
                            new KeyValuePair<string, string>("to_address1", txtboxPostcardAdd.Text),
                            new KeyValuePair<string, string>("to_address2", txtboxPostcardAdd2.Text),
                            new KeyValuePair<string, string>("to_city", txtboxPostcardCity.Text),
                            new KeyValuePair<string, string>("to_state", txtboxPostcardState.Text),
                            new KeyValuePair<string, string>("to_zip", txtboxPostcardZip.Text),
                            new KeyValuePair<string, string>("to_country", txtboxPostcardCountry.Text),
                            new KeyValuePair<string, string>("from_name", "PostaCard"),
                            new KeyValuePair<string, string>("from_address1", "405 Hilgard Ave"),
                            new KeyValuePair<string, string>("from_address2", ""),
                            new KeyValuePair<string, string>("from_city", "Los Angeles"),
                            new KeyValuePair<string, string>("from_state", "CA"),
                            new KeyValuePair<string, string>("from_zip", "90095"),
                            new KeyValuePair<string, string>("from_country", "United States"),
                            new KeyValuePair<string, string>("message", txtboxMessage.Text),
                            new KeyValuePair<string, string>("card_design", pdfurl),
                            new KeyValuePair<string, string>("apikey", "11111")
                        };
                SendPostCard(postcard);
            }
        }

    }
}