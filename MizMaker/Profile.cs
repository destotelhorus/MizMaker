using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MizMaker.Weather;

namespace MizMaker
{
    public class Profile
    {
        public string Name;
        public DateTime StartTime;
        public Clouds Clouds;
        public Fog Fog;
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

            if (!String.IsNullOrWhiteSpace(cells[2]))
            {
                try
                {
                    profile.Clouds = new CloudsNew
                    {
                        Preset = cells[2],
                        Base = Int32.Parse(cells[3])
                    };
                }
                catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid new Cloud settings: {cells[2]} {cells[3]}\n{x.Message}"); }
            }

            if (!String.IsNullOrWhiteSpace(cells[4]))
            {
                try
                {
                    profile.Clouds = new CloudsOld
                    {
                        Base = Int32.Parse(cells[4]),
                        Thickness = Int32.Parse(cells[5]),
                        Density = Int32.Parse(cells[6]),
                        Precipitation = Int32.Parse(cells[7])
                    };
                }
                catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Old cloud settings: {cells[4]} {cells[5]} {cells[6]} {cells[7]}\n{x.Message}"); }
            }

            if (profile.Clouds == null)
            {
                throw new ApplicationException($"{profile.Name} cloud settings are mission\n");
            }

            if (!String.IsNullOrWhiteSpace(cells[8]))
            {
                try
                {
                    profile.Fog = new Fog
                    {
                        Visibility = Int32.Parse(cells[8]),
                        Thickness = Int32.Parse(cells[9]),
                        Enabled = true
                    };
                }
                catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid fog settings: {cells[8]} {cells[9]}\n{x.Message}"); }
            }
            else
            {
                profile.Fog = new Fog
                {
                    Enabled = false
                };
            }

            try
            {
                profile.QNH = Int32.Parse(cells[10]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid QNH: {cells[10]}\n{x.Message}"); }

            try
            {
                profile.Temp = Int32.Parse(cells[11]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Temp: {cells[11]}\n{x.Message}"); }

            try
            {
                profile.DirKtsGnd = DirKts.FromString(cells[12]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at GND: {cells[12]}\n{x.Message}"); }
            
            try
            {
                profile.Wind066 = DirKts.FromString(cells[13]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at 066: {cells[13]}\n{x.Message}"); }
            
            try
            {
                profile.Wind260 = DirKts.FromString(cells[14]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Wind at GND: {cells[14]}\n{x.Message}"); }

            try
            {
                profile.Turb = Int32.Parse(cells[15]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid Turb: {cells[15]}\n{x.Message}"); }

            profile.LhaSpawn = cells[16];
            
            if (!String.IsNullOrWhiteSpace(profile.LhaSpawn))
                try
                {
                    profile.LhaDir = DirKts.FromString(cells[17]);
                }
                catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid LHA/To: {cells[17]}\n{x.Message}"); }

            profile.CvnSpawn = cells[18];
            try
            {
                profile.CvnDir = DirKts.FromString(cells[19]);
            }
            catch (Exception x) { throw new ApplicationException($"{profile.Name} has invalid CVN/To: {cells[19]}\n{x.Message}"); }

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