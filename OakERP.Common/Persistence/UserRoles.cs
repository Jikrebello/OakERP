namespace OakERP.Common.Persistence;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly List<string> All = [Admin, User];
}
