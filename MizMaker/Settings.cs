using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MizMaker
{
    public class Settings
    {   
        public static readonly IDeserializer YamlDeserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
        
        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    var configPaths = new List<string>{".", ".."};
                    foreach (var configPath in configPaths)
                    {
                        var fpath = Path.Combine(configPath, "config.yaml");
                        if (File.Exists(fpath))
                        {
                            string configFile;
                            using (var conf = File.OpenText(fpath))
                                configFile = conf.ReadToEnd();
                            
                            _instance = YamlDeserializer.Deserialize<Settings>(configFile);
                            return _instance;
                        }
                    }
                    throw new FileNotFoundException("No config file available.");
                }
                return _instance;
           }
        }

        public Dictionary<string, string> Theatres { get; set; } = new();
        
        public string MizRegex { get; set; }
        public string WatchFolder { get; set; }
        public string OutFolder { get; set; }
        public string ProcessedFolder { get; set; }
        
        public string WxFolder { get; set; }
        
    }
}
