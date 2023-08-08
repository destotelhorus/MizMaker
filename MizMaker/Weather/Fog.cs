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
                ["visibility"] = Visibility,
                ["thickness"] = Thickness
            };
        }
        return new LsonDict
        {
            ["thickness"] = 0,
            ["visibility"] = 0,
        };
    }
}