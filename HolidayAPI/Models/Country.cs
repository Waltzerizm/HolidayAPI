namespace HolidayAPI.Models
{
    public class Country
    {
        public string? FullName { get; set; }

        public string? CountryCode { get; set; }

        public List<string>? Regions { get; set; }
    }
}
