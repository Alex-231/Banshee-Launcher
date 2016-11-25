using System.IO.Compression;
using System.IO;

namespace MinecraftModLauncher
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
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue; //Not sure why this is here.
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
