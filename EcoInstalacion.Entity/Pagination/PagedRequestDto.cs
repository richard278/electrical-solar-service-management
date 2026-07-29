namespace EcoInstalacion.Entity.Pagination
{
    public class PagedRequestDto
    {
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 25;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 100) ? 100 : (value < 1) ? 1 : value;
        }
    }
}
