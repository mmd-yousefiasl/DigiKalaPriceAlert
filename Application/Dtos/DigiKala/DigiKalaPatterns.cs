using System.Text.RegularExpressions;

namespace Application.Dtos.DigiKala
{
    public static partial class DigiKalaPatterns
    {
        [GeneratedRegex(@"dkp-(\d+)", RegexOptions.IgnoreCase)]
        public static partial Regex ProductIdRegex();
    }
}
