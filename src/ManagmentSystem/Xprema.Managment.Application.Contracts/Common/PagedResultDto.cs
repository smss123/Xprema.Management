using System.Collections.Generic;

namespace Xprema.Managment.Application.Contracts.Common;

/// <summary>
/// Represents a paged list of items
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class PagedResultDto<T>
{
    /// <summary>
    /// Total count of items
    /// </summary>
    public long TotalCount { get; set; }
    
    /// <summary>
    /// List of items in current page
    /// </summary>
    public IReadOnlyList<T> Items { get; set; }
    
    /// <summary>
    /// Creates a new instance of PagedResultDto
    /// </summary>
    public PagedResultDto()
    {
        Items = new List<T>();
    }
    
    /// <summary>
    /// Creates a new instance of PagedResultDto
    /// </summary>
    /// <param name="totalCount">Total count of items</param>
    /// <param name="items">List of items in current page</param>
    public PagedResultDto(long totalCount, IReadOnlyList<T> items)
    {
        TotalCount = totalCount;
        Items = items;
    }
} 