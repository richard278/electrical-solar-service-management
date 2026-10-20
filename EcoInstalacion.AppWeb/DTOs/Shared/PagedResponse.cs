using System;
using System.Collections.Generic;

namespace EcoInstalacion.AppWeb.DTOs.Shared
{
    public sealed class PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }

        public int TotalPages =>
            TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)(PageSize < 1 ? 25 : PageSize));

        public bool HasPreviousPage =>
            TotalCount > 0 && Page > 1;

        public bool HasNextPage =>
            Page < TotalPages;
    }
}
