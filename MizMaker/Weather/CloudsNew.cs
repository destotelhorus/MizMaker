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
            ["base"] = Base,
            ["iprecptns"] = 0
        };
    }
}