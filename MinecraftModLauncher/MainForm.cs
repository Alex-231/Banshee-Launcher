using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace Banshee
{
    public partial class MainForm : Form
    {
        //where is the website being hosted?
        //There's definately a better way of doing this,
        //But considering I had 9 users sign up, and 5 were tests, no biggie.
#if DEBUG
        const string domain = @"http://localhost:8080/";
#else
        const string domain = @"http://banshee.alexnewark.co.uk/";
#endif

        //Definately shouldn't be storing these here... What was I thinking!
        public Version installedVersion;
        public Version latestVersion;

        public MainForm()
        {
            InitializeComponent();
        }


        //Rename this!
        private void button1_Click(object sender, EventArgs e)
        {
            Offline username = new Offline(installedVersion.ToString());
            this.Hide();
            username.ShowDialog();
            this.Show();
        }


        //And this!
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.ToString().Contains("/downloadupdate") && !System.AppDomain.CurrentDomain.FriendlyName.Contains("_old"))
            {
                File.Copy(System.AppDomain.CurrentDomain.FriendlyName, Environment.CurrentDirectory + "\\Client\\" + System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "_old.exe"), true);
                Process.Start(Environment.CurrentDirectory + "\\Client\\" + System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "_old.exe"));
                Environment.Exit(0);
            }
            if (webBrowser1.Url.ToString().Contains("/play"))
            {
                webBrowser1.Hide();
                string document = webBrowser1.DocumentText;
                string username = document.Remove(document.IndexOf(':'));
                string session = document.Remove(0, document.IndexOf(':') + 1);

                ProcessStartInfo launchInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process minecraftProcess = new Process();
                minecraftProcess.StartInfo = launchInfo;
                minecraftProcess.Start();

                minecraftProcess.StandardInput.WriteLine("cd \"" + Environment.CurrentDirectory + "\\Client\"");
                minecraftProcess.StandardInput.WriteLine("java -cp minecraft.jar;\"libs\\lwjgl.jar\";\"libs\\lwjgl_util.jar\" -Djava.library.path=\"natives\" net.minecraft.client.Minecraft " + username + " " + session);

                Environment.Exit(0);
            }
        }

        //I think this is right, just needs renaming.
        //It's not checking if there's an internet conection, 
        //It's checking if the launcher can reach the website.
        //Maybe a readonly boolean property would have been nicer too.
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(domain))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        //Break this down into more methods.
        //There's actually a bunch of update handling stuff in here. Shouldn't be.
        //Update class needs refactoring, make this stuff public static, call methods from update.
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!System.AppDomain.CurrentDomain.FriendlyName.Contains("_old") && File.Exists(Environment.CurrentDirectory + "\\Client\\" + System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "_old.exe")))
            {
                File.Delete(Environment.CurrentDirectory + "\\Client\\" + System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "_old.exe"));
            }

            //If there's no version file, make one.
            //Should be using properties for this I imagine.
            if (!File.Exists(Environment.CurrentDirectory + "\\Client\\version"))
            {
                if (!Directory.Exists(Environment.CurrentDirectory + "\\Client"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\Client");
                }
                File.WriteAllText(Environment.CurrentDirectory + "\\Client\\version", "0.0.0.0");
            }

            //Read the version from the text file....
            //Yeah I really should be using properties.
            installedVersion = new Version(File.ReadAllText(Environment.CurrentDirectory + "\\Client\\version"));

            //If the user is disconnected and doesn't have a build installed, let them know, exit.
            //Otherwise, instanciate the username form.
            if (!CheckForInternetConnection())
            {
                if (installedVersion.ToString() == "0.0.0.0")
                {
                    MessageBox.Show("Banshee: Offline, with no version installed.\nConnect to the internet to download and install Banshee.", "Banshee - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                else
                {
                    //This is rediculous! This is why I shouldn't have been storing the version here!
                    //This constructor needs eradicating.
                    Offline username = new Offline(installedVersion.ToString());
                    this.Hide();
                    username.ShowDialog();
                    this.Close();
                }
            }

            //Retrieve the current version from the banshee site API... if you could call it an API...
            string version = new WebClient().DownloadString(domain + "version");
            //domain/version also returns a url, where to download the update. That's after a colon.
            //Should have seperated these really.
            latestVersion = new Version(version.Remove(version.IndexOf(':')));

            //...k
            webBrowser1.Url = new Uri(domain);

            //If the exe is "BansheeLauncher_old.exe", it's updating. Continue.
            //I did this because I wasn't sure how to replace the exe while it was executing.
            //I imagine there's a better way but, I was rushed. This was more prototype.
            if (System.AppDomain.CurrentDomain.FriendlyName.Contains("_old"))
            {
                Update updateForm = new Update(latestVersion.ToString(), domain);
                this.Hide();
                updateForm.ShowDialog();

#if DEBUG
                //This was actually a debug string that made it into every release! I got a lot of questions about this :p
                MessageBox.Show(Environment.CurrentDirectory.Replace("Client", "\\\\") + "\\" + System.AppDomain.CurrentDomain.FriendlyName.Replace("_old.exe", ".exe"));
#endif

                Process.Start(Environment.CurrentDirectory.Replace("Client", "\\\\") + "\\" +  System.AppDomain.CurrentDomain.FriendlyName.Replace("_old.exe", ".exe"));
                Environment.Exit(0);
            }

            //This integer represents the difference between the installed version, and the online version.
            int comparison = latestVersion.CompareTo(installedVersion);

            //If the installed version is old, go to the update page.
            if(comparison > 0)
            {
                //An update is available.
                webBrowser1.Url = new Uri(domain + "update");
            }
        }
    }
}
