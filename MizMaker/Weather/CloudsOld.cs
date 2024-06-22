using MizMaker.Lua;

namespace MizMaker.Weather;

public class CloudsOld : Clouds
{
    public double Density;
    public double Precipitation;
    public double Thickness;
    public double Base;

    public override LsonDict CreateLsonDict()
    {
        return new LsonDict
        {
            ["density"] = Density,
            ["thickness"] = (int) (Thickness * 0.3048),
            ["base"] = (int) (Base * 0.3048),
            ["iprecptns"] = Precipitation
        };
    }
}