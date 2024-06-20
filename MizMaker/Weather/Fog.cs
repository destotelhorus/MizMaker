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
                ["visibility"] = (int) (Visibility * 0.3048 / 100.0),
                ["thickness"] = (int) (Thickness * 0.3048 / 100.0)
            };
        }
        return new LsonDict
        {
            ["thickness"] = 0,
            ["visibility"] = 0
        };
    }
}