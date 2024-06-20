using MizMaker.Lua;

namespace MizMaker.Weather;

public class CloudsNew : Clouds
{
    public string Preset;
    public double Base;

    public override LsonDict CreateLsonDict()
    {
        return new LsonDict
        {
            ["density"] = 0,
            ["thickness"] = 200,
            ["preset"] = Preset,
            ["base"] = (int) (Base * 0.3048 / 100.0),
            ["iprecptns"] = 0
        };
    }
}