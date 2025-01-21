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
                return File.ReadAllLines(Path.Combine(Settings.Instance.WxFolder, $"{category}.csv")).Skip(1).Select(Profile.FromString).ToArray();
            }
            catch (FileNotFoundException)
            {
                return new Profile[] { };
            }
        }

        public static void Main()
        {
            LsonDict.SerializeImplicitIndex = true;

            foreach (var theatre in Settings.Instance.Theatres.Values)
            {
                var watchPath = Settings.Instance.WatchFolder.Replace("__MAP__", theatre);
                if (!Directory.Exists(watchPath))
                {
                    Console.WriteLine($"Cannot watch directory: Watch folder does not exist - {watchPath}");
                    continue;
                }

                var outPath = Settings.Instance.OutFolder.Replace("__MAP__", theatre);
                if (!Directory.Exists(outPath))
                {
                    Console.WriteLine($"Cannot watch directory: Out folder does not exist - {outPath}");
                    continue;
                }

                var processedPath = Settings.Instance.ProcessedFolder?.Replace("__MAP__", theatre);

                Console.WriteLine($"Now watching: {watchPath}");
                
                var w = new FileSystemWatcher(watchPath);
                w.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;
                w.Filter = "*.miz";
                w.EnableRaisingEvents = true;
                w.IncludeSubdirectories = false;

                w.Changed += (_,_) =>
                {
                    Thread.Sleep(10000);
                    RunOnce(watchPath, outPath, processedPath);
                };
                w.Created += (_,_) =>
                {
                    Thread.Sleep(10000);
                    RunOnce(watchPath, outPath, processedPath);
                };
                w.Renamed += (_,_) =>
                {
                    Thread.Sleep(10000);
                    RunOnce(watchPath, outPath, processedPath);
                };
            }

            while (Console.ReadLine() != "exit")
            {
            }
        }

        static void RunOnce(string watchFolder, string outFolder, string processedFolder)
        {
            lock (LockObject)
            {
                var mizTemplates = Directory.GetFiles(watchFolder, "*.miz", SearchOption.TopDirectoryOnly);
                var sb = new StringBuilder();

                foreach (var mizTemplate in mizTemplates)
                {
                    if (processedFolder != null && File.Exists(Path.Combine(processedFolder, Path.GetFileName(mizTemplate))))
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
                        var miz = new MizFile(mizTemplate, outFolder);
                        foreach (var profile in LoadProfiles(miz.FilePrefix))
                            miz.ApplyProfile(profile);
                        miz.Finish(processedFolder);
                    }
                    catch (ApplicationException x)
                    {
                        sb.Append(DateTime.UtcNow.ToString("u"));
                        sb.Append("] Exception while processing: ");
                        sb.AppendLine(Path.GetFileName(mizTemplate));
                        sb.AppendLine(x.Message);
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