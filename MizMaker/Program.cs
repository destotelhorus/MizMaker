using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MizMaker.Lua;

namespace MizMaker
{
    internal class Program
    {
        private static readonly object LockObject = new();
        
        private static Profile[] LoadProfiles(string category)
        {
            try
            {
                return File.ReadAllLines(Path.Combine(Settings.Instance.WxFolder, $"{category}.csv")).Select(Profile.FromString).ToArray();
            }
            catch (FileNotFoundException)
            {
                return new Profile[] { };
            }
        }

        public static void Main()
        {
            LsonDict.SerializeImplicitIndex = true;

            if (Settings.Instance.ProcessedFolder != null)
            {
                foreach (var watchPath in Settings.Instance.Theatres.Values
                    .Select(x => Settings.Instance.WatchFolder.Replace("__MAP__", x))
                    .Where(Directory.Exists))
                {
                    Console.WriteLine($"Now watching: {watchPath}");
                    
                    var w = new FileSystemWatcher(watchPath);
                    w.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;
                    w.Filter = "*.miz";
                    w.EnableRaisingEvents = true;
                    w.IncludeSubdirectories = false;

                    w.Changed += (_,_) =>
                    {
                        Thread.Sleep(10000);
                        RunOnce(watchPath);
                    };
                    w.Created += (_,_) =>
                    {
                        Thread.Sleep(10000);
                        RunOnce(watchPath);
                    };
                    w.Renamed += (_,_) =>
                    {
                        Thread.Sleep(10000);
                        RunOnce(watchPath);
                    };
                }

                while (Console.ReadLine() != "exit")
                {
                }
            }
            else
                foreach (var watchPath in Settings.Instance.Theatres.Values
                    .Select(x => Settings.Instance.WatchFolder.Replace("__MAP__", x))
                    .Where(Directory.Exists))
                {
                    RunOnce(watchPath);
                }
        }

        static void RunOnce(string watchFolder)
        {
            lock (LockObject)
            {
                var mizTemplates = Directory.GetFiles(watchFolder, "*.miz", SearchOption.TopDirectoryOnly);
                var sb = new StringBuilder();

                foreach (var mizTemplate in mizTemplates)
                {
                    if (Settings.Instance.ProcessedFolder != null && File.Exists(Path.Combine(Settings.Instance.ProcessedFolder, Path.GetFileName(mizTemplate))))
                    {
                        sb.Append(DateTime.UtcNow.ToString("u"));
                        sb.Append("] ERR Template of name '");
                        sb.Append(Path.GetFileName(mizTemplate));
                        sb.AppendLine("' already processed!");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"Found: {mizTemplate}");
                        var miz = new MizFile(mizTemplate, Settings.Instance.OutFolder);
                        foreach (var profile in LoadProfiles(miz.FilePrefix))
                            miz.ApplyProfile(profile);
                        miz.Finish(Settings.Instance.ProcessedFolder);
                    }
                    catch (Exception x)
                    {
                        sb.Append(DateTime.UtcNow.ToString("u"));
                        sb.Append("] Exception while processing: ");
                        sb.AppendLine(Path.GetFileName(mizTemplate));
                        sb.AppendLine(x.ToString());
                    }
                }

                if (sb.Length > 0)
                {
                    File.WriteAllText(Path.Combine(watchFolder, "Readme.md"), sb.ToString());
                }
            }
        }
    }
}