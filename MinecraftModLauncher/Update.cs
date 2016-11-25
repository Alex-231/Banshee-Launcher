using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace MinecraftModLauncher
{
    public partial class Update : Form
    {
        string domain;
        string versionNumber;

        //In this rediculous constructor, I take values that should be accessible anyway from another form that just closed.
        public Update(string versionIn, string domainIn)
        {
            InitializeComponent();
            this.version.Text = "Banshee " + versionIn;
            versionNumber = versionIn;
            domain = domainIn;
        }

        //Again, this method should be broken down and refactored.
        private void Update_Load(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            //Cant remember how these work exactly, had to research them at the time.
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            // Starts the download
            //If there's no Banshee folder in the temp directory, make one.
            if(!Directory.Exists(Path.GetTempPath() + "\\Banshee"))
                Directory.CreateDirectory(Path.GetTempPath() + "\\Banshee");
            //If there's already a zip downloaded, just not installed, abord download.
            if(File.Exists(Path.GetTempPath() + "Banshee\\" + versionNumber + ".zip"))
                client_DownloadFileCompleted(new object(), EventArgs.Empty);

            //Not sure why I left this in, I had no idea I left so much debug stuff.
#if DEBUG
            var debug = Path.GetTempPath() + "Banshee";
#endif

            //Download the latest zip...
            client.DownloadFileAsync(new Uri(domain + versionNumber + ".zip"), Path.GetTempPath() + "Banshee\\" + versionNumber + ".zip");
            this.process.Text = "Downloading...";
        }

        //When the download progress changes, update the zip.
        //Not sure exactly if this works, can't remember.
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        //Once the file is downloaded, begin installation.
        //I say begin, because I only install half while the old exe is running.
        //Then the rest when the new exe is launched.
        //It works I guess.
        void client_DownloadFileCompleted(object sender, EventArgs e)
        {
            this.process.Text = "Installing...";

            progressBar1.Style = ProgressBarStyle.Marquee;

            //If the downloaded file cannot be found, error!
            if(!File.Exists(Path.GetTempPath() + "\\Banshee\\" + versionNumber + ".zip"))
                MessageBox.Show("Something went wrong!\nCouldn't locate the downloaded file.", "Banshee - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //Otherwise, grab the zip, and extract.
            ZipArchive zip = new ZipArchive(File.OpenRead(Path.GetTempPath() + "\\Banshee\\" + versionNumber + ".zip"));
            ZipArchiveExtensions.ExtractToDirectory(zip, Environment.CurrentDirectory.Replace("\\Client", ""), true);

            //This is dumb, I don't think it's possible to see it.
            //I probably left this in from an old design.
            this.process.Text = "Complete";

            this.Close();
            //Extract
        }

    }
}