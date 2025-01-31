namespace CommentsApp.DTO;

public class PaginationMetadata
{
    public int TotalItemCount { get; set; }
    public int TotalPageCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    string? SearchQuery { get; set; }

    public PaginationMetadata(int totalItemCount, int pageSize, int currentPage, string? searchQuery = null)
    {
        TotalItemCount = totalItemCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        SearchQuery = searchQuery;
        TotalPageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
    }
}
