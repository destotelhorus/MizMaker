using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Instrumentation;
using System.Text.RegularExpressions;

namespace MizMaker
{
    public class Profile
    {
        public string Name;
        public DateTime StartTime;
        public string CloudPreset;
        public int CloudBase;
        public int QNH;
        public int Temp;
        public DirKts DirKtsGnd;
        public DirKts Wind066;
        public DirKts Wind260;
        public int Turb;
        public string LhaSpawn;
        public DirKts LhaDir;
        public string CvnSpawn;
        public DirKts CvnDir;

        public static Profile FromString(string s)
        {
            var cells = s.Split('\t');

            var profile = new Profile();
            profile.Name = cells[0];

            try
            {
                profile.StartTime = DateTime.ParseExact(cells[1].TrimEnd('L'), "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Start Time: {cells[1]}\n{x.Message}"); }

            profile.CloudPreset = cells[2];
            try
            {
                profile.CloudBase = Int32.Parse(cells[3]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Cloud Base: {cells[3]}\n{x.Message}"); }

            try
            {
                profile.QNH = Int32.Parse(cells[4]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid QNH: {cells[4]}\n{x.Message}"); }

            try
            {
                profile.Temp = Int32.Parse(cells[5]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Temp: {cells[5]}\n{x.Message}"); }

            try
            {
                profile.DirKtsGnd = DirKts.FromString(cells[6]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at GND: {cells[6]}\n{x.Message}"); }
            
            try
            {
                profile.Wind066 = DirKts.FromString(cells[7]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at 066: {cells[7]}\n{x.Message}"); }
            
            try
            {
                profile.Wind260 = DirKts.FromString(cells[8]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at GND: {cells[8]}\n{x.Message}"); }

            try
            {
                profile.Turb = Int32.Parse(cells[9]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Turb: {cells[9]}\n{x.Message}"); }

            profile.LhaSpawn = cells[10];
            
            if (!String.IsNullOrWhiteSpace(profile.LhaSpawn))
                try
                {
                    profile.LhaDir = DirKts.FromString(cells[11]);
                }
                catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid LHA/To: {cells[11]}\n{x.Message}"); }

            profile.CvnSpawn = cells[12];
            try
            {
                profile.CvnDir = DirKts.FromString(cells[13]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid CVN/To: {cells[13]}\n{x.Message}"); }
            profile.CvnDir = DirKts.FromString(cells[13]);
            return profile;
        }
    }

    public class DirKts
    {
        public int Dir { get; set; }
        public int Knots { get; set; }

        public double Meters => Knots * 0.5144444;
        public int Reciprocal => Dir > 180 ? Dir - 180 : Dir + 180;

        private static readonly Regex WindRx = new(@"(^[0-9]{3})([0-9]{2})kt$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static DirKts FromString(string s)
        {
            var m = WindRx.Match(s);
            return new DirKts
            {
                Dir = Int32.Parse(m.Groups[1].Value),
                Knots = Int32.Parse(m.Groups[2].Value)
            };
        }
    }
}