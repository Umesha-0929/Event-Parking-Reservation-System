namespace SEVPMS.Application.Common.Paging;

public static class PagingRules
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public static (int Page, int PageSize, int Skip) Normalize(int page, int pageSize)
    {
        var safePage = page < 1 ? DefaultPage : page;
        var safePageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (safePage, safePageSize, checked((safePage - 1) * safePageSize));
    }
}
