namespace eAppraisal.Shared.Auth;

public static class AppRoles
{
    public const string HR        = "HR";
    public const string Manager   = "Manager";
    public const string Employee  = "Employee";
    public const string ITAdmin   = "ITAdmin";

    public static readonly IReadOnlyList<string> All =
        [HR, Manager, Employee, ITAdmin];
}
