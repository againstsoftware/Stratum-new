using System.Runtime.InteropServices;

public static class MobileDetection
{
    [DllImport("__Internal")]
    private static extern bool IsMobile();


    public static bool Mobile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobile();
#else
        return false;
#endif
    }
}