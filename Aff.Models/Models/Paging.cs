namespace Aff.Models.Models
{
    public class Paging
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public int Pages { get; set; }

        public int TotalRecords { get; set; }
        public string Sort { get; set; }
        public string Field { get; set; }
        public string Query { get; set; }
    }
}
