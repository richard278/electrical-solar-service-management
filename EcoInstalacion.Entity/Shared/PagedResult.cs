using System;
using System.Collections.Generic;

namespace EcoInstalacion.Entity.Shared
{
    public sealed class PagedResult<T>
    {
        public PagedResult(
            IReadOnlyList<T> items,
            int page,
            int pageSize,
            int totalCount)
        {
            Items = items ?? Array.Empty<T>();
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
    }
}
