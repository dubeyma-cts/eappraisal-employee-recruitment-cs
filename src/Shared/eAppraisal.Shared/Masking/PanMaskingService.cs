using System.Text.RegularExpressions;

namespace eAppraisal.Shared.Masking;

/// <summary>
/// PAN masking: ABCDE1234F → ABCDE****F
/// Rule: keep first 5 chars, mask chars 6-9 with *, keep last char.
/// Guards against null / empty / already-masked inputs.
/// </summary>
public static class PanMaskingService
{
    private static readonly Regex PanPattern =
        new(@"^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled);

    public static string MaskPan(string? rawPan)
    {
        if (string.IsNullOrWhiteSpace(rawPan)) return string.Empty;

        var pan = rawPan.Trim().ToUpperInvariant();

        if (pan.Contains('*'))
            return pan; // already masked — do not double-mask

        if (pan.Length != 10)
            return MaskArbitrary(pan); // non-standard length — mask middle

        return $"{pan[..5]}****{pan[9]}";
    }

    public static bool IsValidPan(string? pan)
    {
        if (string.IsNullOrWhiteSpace(pan)) return false;
        return PanPattern.IsMatch(pan.Trim().ToUpperInvariant());
    }

    private static string MaskArbitrary(string value)
    {
        if (value.Length <= 2) return new string('*', value.Length);
        int keep = Math.Max(1, value.Length / 4);
        return value[..keep] + new string('*', value.Length - keep * 2) + value[^keep..];
    }
}
