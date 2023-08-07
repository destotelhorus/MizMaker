using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MizMaker.Lua;

namespace MizMaker
{
    internal class Program
    {
        private static string _outFolder;
        private static string _watchFolder;
        private static string _templateOutFolder;
        private static readonly object _lockObject = new object();
        
        private static Profile[] LoadProfiles(string category)
        {
            try
            {
                return File.ReadAllLines($"wx/{category}.csv").Select(Profile.FromString).ToArray();
            }
            catch (FileNotFoundException)
            {
                return new Profile[] { };
            }
        }

        public static void Main(string[] args)
        {
            _outFolder = args.FirstOrDefault() ?? "out";
            _watchFolder = args.Skip(1).FirstOrDefault();
            _templateOutFolder = args.Skip(2).FirstOrDefault();

            _watchFolder = _watchFolder == null ? "." : Path.Combine(_outFolder, _watchFolder);
            
            if (_templateOutFolder != null)
                _templateOutFolder = Path.Combine(_outFolder, _templateOutFolder);
            
            LsonDict.SerializeImplicitIndex = true;

            if (_templateOutFolder != null)
            {
                foreach (var watchPath in MizFile.KnownTheatres
                    .Select(x => _watchFolder.Replace("__MAP__", x))
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
                foreach (var watchPath in MizFile.KnownTheatres
                    .Select(x => _watchFolder.Replace("__MAP__", x))
                    .Where(Directory.Exists))
                {
                    RunOnce(watchPath);
                }
        }

        static void RunOnce(string watchFolder)
        {
            lock (_lockObject)
            {
                var mizTemplates = Directory.GetFiles(watchFolder, "*.miz", SearchOption.TopDirectoryOnly);
                var sb = new StringBuilder();

                foreach (var mizTemplate in mizTemplates)
                {
                    if (_templateOutFolder != null && File.Exists(Path.Combine(_templateOutFolder, Path.GetFileName(mizTemplate))))
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
                        var miz = new MizFile(mizTemplate, _outFolder);
                        foreach (var profile in LoadProfiles(miz.FilePrefix))
                            miz.ApplyProfile(profile);
                        miz.Finish(_templateOutFolder);
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