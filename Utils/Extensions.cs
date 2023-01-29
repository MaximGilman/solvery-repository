namespace Utils
{
    public static class Extensions
    {
        public static string CropUpToLength(this string value, int length)
        =>    value.Length >= length ? value[..length] : value;

    }
}