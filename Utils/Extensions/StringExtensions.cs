using System.Text.RegularExpressions;

namespace Utils.Extensions;

/// <summary>
/// Расширения для строк.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Обрезать строку до указанной длины.
    /// </summary>
    /// <param name="str">Строка.</param>
    /// <param name="length">Максимальная длина.</param>
    public static string CropUpToLength(this string str, int length)
        => str.Length >= length ? str[..length] : str;


    public static Guid SubstringGuid(this string str)
    {
        return Guid.Parse(Regex.Replace(str,
            @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$",
            "'$0'"));
    }
}
