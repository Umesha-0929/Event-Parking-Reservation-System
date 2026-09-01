namespace SEVPMS.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string CustomerOnly = "CustomerOnly";

    public const string EventOrganizerOnly =
        "EventOrganizerOnly";

    public const string VenueOwnerOnly =
        "VenueOwnerOnly";

    public const string AdminOnly =
        "AdminOnly";
}