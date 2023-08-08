using System;
using MizMaker.Lua;

namespace MizMaker.Weather;

public class CloudsNew : Clouds
{
    public string Preset;
    public int Base;

    public override LsonDict CreateLsonDict()
    {
        return new LsonDict
        {
            ["density"] = 0,
            ["thickness"] = 200,
            ["preset"] = Preset,
            ["base"] = Math.Round(Base * 0.3048 / 100) * 100,
            ["iprecptns"] = 0
        };
    }
}