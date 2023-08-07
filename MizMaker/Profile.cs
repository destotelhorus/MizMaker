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
            var i = 0;

            return new Profile
            {
                Name = cells[i++],
                StartTime = DateTime.ParseExact(cells[i++].TrimEnd('L'), "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                CloudPreset = cells[i++],
                CloudBase = Int32.Parse(cells[i++]),
                QNH = Int32.Parse(cells[i++]),
                Temp = Int32.Parse(cells[i++]),
                DirKtsGnd = DirKts.FromString(cells[i++]),
                Wind066 = DirKts.FromString(cells[i++]),
                Wind260 = DirKts.FromString(cells[i++]),
                Turb = Int32.Parse(cells[i++]),
                LhaSpawn = cells[i++],
                LhaDir = DirKts.FromString(cells[i++]),
                CvnSpawn = cells[i++],
                CvnDir = DirKts.FromString(cells[i++]),
            };
        }
    }

    public class DirKts
    {
        public int Dir { get; set; }
        public int Knots { get; set; }

        public double Meters => Knots * 0.5144444;
        public int Reciprocal => Dir > 180 ? Dir - 180 : Dir + 180;

        private static readonly Regex WindRx = new Regex(@"(^[0-9]{3})([0-9]{2})kt$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
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