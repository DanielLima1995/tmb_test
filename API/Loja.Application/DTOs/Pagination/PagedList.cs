namespace Loja.Application.DTOs.Pagination;

public class PagedList<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PerPage);

    public PagedList(List<T> items, int totalCount, int page, int perPage)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PerPage = perPage;
    }
}
