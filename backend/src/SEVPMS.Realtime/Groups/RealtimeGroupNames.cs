namespace SEVPMS.Realtime.Groups;

public static class RealtimeGroupNames
{
    public static string User(Guid userId) => $"user:{userId:N}";
    public static string Event(Guid eventId) => $"event:{eventId:N}";
    public const string Admins = "role:admins";
    public const string Organizers = "role:organizers";
}
