using System.IO.Compression;
using System.IO;

namespace Banshee
{
    public static class ZipArchiveExtensions
    {
        //I think I grabbed this method from somewhere online.......
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, entry.FullName);
                completeFileName = completeFileName.Replace("/", "\\");
                string directory = completeFileName.Remove(completeFileName.LastIndexOf("\\"));

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (File.Exists(completeFileName))
                    File.Delete(completeFileName);

                entry.ExtractToFile(completeFileName, true);
            }
        }
    }
}
