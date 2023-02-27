using System.Text.RegularExpressions;
using Utils.Guards;

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

    private const string GUID_REGEX = @"(?i)[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?";

    public static Guid SubstringGuid(this string str)
    {
        var match = Regex.Match(str, GUID_REGEX);

        Guard.IsMatch(match.Value, GUID_REGEX);
        return Guid.Parse(match.Value);

    }
}
