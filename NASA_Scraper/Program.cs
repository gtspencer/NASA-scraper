using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using HtmlAgilityPack;
using Microsoft.Win32;

namespace NASA_Scraper
{
    class Program
    {
        const string url = "https://mars.nasa.gov/mars2020/multimedia/raw-images/";
        const string regex = "https.*png";
        static Timer timer;
        // 300000 is 5 minutes
        // 3600000 is 1 hr
        const int timerInterval_Mili = 3600000;

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        static void Main(string[] args)
        {
            ScrapePage();
            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = timerInterval_Mili;
            timer.Start();

            while (true) ;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ScrapePage();
        }

        private static void ScrapePage()
        {
            var response = CallUrl(url).Result;
            MatchCollection mc = Regex.Matches(response, regex);

            if (mc.Count > 0)
            {
                Set(mc[0].Value);
            }
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        public static void Set(string url)
        {
            System.IO.Stream s = new System.Net.WebClient().OpenRead(url);

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            key.SetValue(@"WallpaperStyle", 1.ToString());
            key.SetValue(@"TileWallpaper", 0.ToString());

            /*if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }*/

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            // Console.Write($"Set background to: {url}");
        }
    }
}
