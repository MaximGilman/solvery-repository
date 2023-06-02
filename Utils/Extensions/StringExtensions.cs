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


    /// <summary>
    /// Достать из строки первый GUID, являющийся подстрокой.
    /// </summary>
    /// <param name="str">входная строка.</param>
    /// <returns>GUID из строки.</returns>
    /// <exception cref="ArgumentException">Если GUID'ов не обнаружено.</exception>
    public static Guid SubstringGuidFromWords(this string str)
    {
        var stringGuid = str.Split().FirstOrDefault(x => Guid.TryParse(x, out _));
        if (stringGuid != default)
        {
            return Guid.Parse(stringGuid);
        }
        throw new ArgumentException($" {str} должно соответстовать GUID или его строковому представлению");
    }
}
