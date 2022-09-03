using System;
using System.Collections.Generic;
using System.Text;

namespace BDSUpdater
{
    class Configuration
    {
        public string ServerPath { get; set; } = @"C:\Games\Minecraft Server";
        public string ArchivePath { get; set; } = @"C:\Games\Minecraft Worlds\Archive\";
        public TimeSpan UpdateTime { get; set; } = new TimeSpan(0, 15, 0, 0, 0);


    }
}
