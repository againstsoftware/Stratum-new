
using System.Runtime.InteropServices;

public static class MobileDetection
{
    [DllImport("__Internal")]
    private static extern bool IsMobile();


    public static bool Mobile => IsMobile();
}
