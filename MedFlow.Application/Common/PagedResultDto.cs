namespace MedFlow.Application.Common;

/// <summary>Sahifalangan ro'yxat: joriy sahifa elementlari va filtrga mos jami yozuvlar soni.</summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
