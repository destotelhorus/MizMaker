using System;
using MizMaker.Lua;

namespace MizMaker.Weather;

public class Fog
{
    public int Visibility;
    public int Thickness;
    public bool Enabled;

    public LsonDict CreateLsonDict()
    {
        if (Enabled)
        {
            return new LsonDict
            {
                ["visibility"] = Math.Round(Visibility * 0.3048 / 100) * 100,
                ["thickness"] = Math.Round(Thickness * 0.3048 / 100) * 100
            };
        }
        return new LsonDict
        {
            ["thickness"] = 0,
            ["visibility"] = 0
        };
    }
}