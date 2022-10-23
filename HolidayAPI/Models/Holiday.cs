namespace HolidayAPI.Models
{
    public class Holiday
    {
        public HolidayDate? Date { get; set; }
        public List<HolidayName>? Name { get; set; }
        public string? HolidayType { get; set; }
    }
}
