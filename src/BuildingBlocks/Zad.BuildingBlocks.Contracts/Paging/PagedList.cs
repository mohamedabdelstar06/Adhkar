namespace Zad.BuildingBlocks.Contracts.Paging;

public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
