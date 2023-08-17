public static class ExtensionMethods
{
    public const float VIRTUAL_SCALE = 200;

    public static float VirtualToRealScale(this float size) 
    {
        return size * VIRTUAL_SCALE;
    }

    public static float RealToVirtualScale(this float size)
    {
        return size / VIRTUAL_SCALE;
    }
}
