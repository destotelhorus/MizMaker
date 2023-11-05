using System;
using MizMaker.Lua;

namespace MizMaker.Weather;

public class CloudsOld : Clouds
{
    public int Density;
    public int Precipitation;
    public int Thickness;
    public int Base;

    public override LsonDict CreateLsonDict()
    {
        return new LsonDict
        {
            ["density"] = Density,
            ["thickness"] = Math.Round(Thickness * 0.3048 / 100) * 100,
            ["base"] = Math.Round(Base * 0.3048 / 100) * 100,
            ["iprecptns"] = Precipitation
        };
    }
}