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
            ["thickness"] = Thickness,
            ["base"] = Base,
            ["iprecptns"] = Precipitation,
        };
    }
}