using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace BDSUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new Configuration();
            var nextCheck = DateTime.Now.Date + config.UpdateTime;
            var p = new Program();//yes i am lazy
            await p.EndOfDayProcess(config);
            while (true)
            {
                if (DateTime.Now > nextCheck)
                {
                    await p.EndOfDayProcess(config);
                    nextCheck = DateTime.Now.Date + config.UpdateTime;
                }
                System.Threading.Thread.Sleep(10000);
            }
        }

        private async Task EndOfDayProcess(Configuration config)
        {
            var serverProcess = Process.GetProcessesByName("bedrock_server");
            foreach (var p in serverProcess)
            {                
                p.Kill();
            }
            ArchiveWorld(config);
            await CheckUpdate(config);
            StartService(config);

        }

        private void StartService(Configuration config)
        {
            var exe = Path.Combine(config.ServerPath, "bedrock_server.exe");
            System.Diagnostics.Process.Start(exe);
        }

        private async Task CheckUpdate(Configuration config)
        {
            //TODO how to know current version?
            var current = "1.19.22.0";
            var latest = await GetLatest();
            if (IsOlder(current, latest.Version))
            {
                await DownloadAndUnpack(latest.Url,config);
            }
        }

        private async Task DownloadAndUnpack(string version,Configuration config)
        {
            var temp = Path.Combine(
          System.IO.Path.GetTempPath(),
          version.Substring(version.LastIndexOf('/')+1));
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(version);

                    using (var fs = new FileStream(temp, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    System.IO.Compression.ZipFile.ExtractToDirectory(temp, config.ServerPath, true);
             
                }
            }
            finally
            {
                if (System.IO.File.Exists(temp))
                    System.IO.File.Delete(temp);
            }
          
        }

        private bool IsOlder(string current, string version)
        {
            var cFields = current.Split('.');
            var vFields = version.Split('.');
            for (int i = 0; i < Math.Min(cFields.Length, vFields.Length); i++)
            {
                if (int.Parse(cFields[i]) < int.Parse(vFields[i]))
                    return true;
            }
            return false;
        }

        private async Task<DownloadVersion> GetLatest()
        {
            //TODO
            //currently https://minecraft.fandom.com/wiki/Bedrock_Dedicated_Server will list all versions and the direct link
            // this is the same direct link you get from https://www.minecraft.net/en-us/download/server/bedrock after you click the accept
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://minecraft.fandom.com/wiki/Bedrock_Dedicated_Server");
                var doc = new HtmlDocument();
                doc.LoadHtml(response);
                var links = doc.DocumentNode.SelectNodes("//tr");//I suck with xpath, want to select all links with href that starts  https://minecraft.azureedge.net/bin-win
                if (links!=null)
                {
                    
                }
                //then extract the version number
            }
            return new DownloadVersion { Version = "1.19.22.01", Url = "https://minecraft.azureedge.net/bin-win/bedrock-server-1.19.22.01.zip" };
        }

        private static void ArchiveWorld(Configuration config)
        {
            var worldPath = System.IO.Path.Combine(config.ServerPath, "worlds");

            foreach (var world in System.IO.Directory.EnumerateDirectories(worldPath))
            {
                var archivePath = world.Replace(worldPath, config.ArchivePath);
                if (!System.IO.Directory.Exists(archivePath))
                    System.IO.Directory.CreateDirectory(archivePath);
                var archiveWorld = System.IO.Path.Combine(archivePath, $"{DateTime.Now.ToString("yyyyMMdd")}.zip");
                if (System.IO.File.Exists(archiveWorld))
                    System.IO.File.Delete(archiveWorld);
                System.IO.Compression.ZipFile.CreateFromDirectory(world, archiveWorld);
            }
        }
    }
}
