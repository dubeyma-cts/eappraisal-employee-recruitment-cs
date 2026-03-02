namespace eAppraisal.Domain.Common;

/// <summary>
/// Utility for masking PAN (Permanent Account Number) values.
/// Only HR and Admin roles may view the full PAN.
/// </summary>
public static class PanMaskingService
{
    /// <summary>
    /// Masks all characters except the last 4.
    /// Returns null/empty as-is.
    /// Example: "ABCDE1234F" -> "******234F"
    /// </summary>
    public static string? Mask(string? pan)
    {
        if (string.IsNullOrWhiteSpace(pan))
            return pan;

        if (pan.Length <= 4)
            return pan;

        return new string('*', pan.Length - 4) + pan[^4..];
    }

    /// <summary>
    /// Returns true if the given role is allowed to see the full (unmasked) PAN.
    /// Only HR and Admin roles have this privilege.
    /// </summary>
    public static bool CanViewFullPan(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return role.Equals("HR", StringComparison.OrdinalIgnoreCase)
            || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the full PAN for HR/Admin roles, or a masked version for everyone else.
    /// </summary>
    public static string? ForRole(string? pan, string? role)
    {
        return CanViewFullPan(role) ? pan : Mask(pan);
    }
}
