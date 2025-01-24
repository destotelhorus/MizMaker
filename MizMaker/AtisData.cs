using System;

namespace MizMaker
{
    public class AtisData
    {
        public string Name;
        public int Altitude;
        public int HeadingEast;
        public string ArrivalsEast;
        public string DeparturesEast;
        public int HeadingWest;
        public string ArrivalsWest;
        public string DeparturesWest;

        public static AtisData FromString(string s)
        {
            var cells = s.Split(',');
            var data = new AtisData();

            data.Name = cells[0];
            data.Altitude = Int32.Parse(cells[1]);
            data.HeadingEast = Int32.Parse(cells[2]);
            data.ArrivalsEast = cells[3];
            data.DeparturesEast = cells[4];
            data.HeadingWest = Int32.Parse(cells[5]);
            data.ArrivalsWest = cells[6];
            data.DeparturesWest = cells[7];
            
            return data;
        }
    }
}