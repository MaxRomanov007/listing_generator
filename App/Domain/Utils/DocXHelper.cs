namespace App.Domain.Utils;

public static class DocXHelper
{
    private const float Inch = 2.54f;
    private const byte PointsInInch = 72;
    
    public static float CmToPt(float cm)
    {
        return cm / Inch * PointsInInch;
    } 
}