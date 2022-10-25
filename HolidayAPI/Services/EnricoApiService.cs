using HolidayAPI.Models;
using Newtonsoft.Json;
using System.Text;

namespace HolidayAPI.Services
{
    public class EnricoApiService
    {

        private HttpClient _client;

        public EnricoApiService()
        {
            _client = new HttpClient();
        }

        public async Task<List<Country>> GetSupportedCountries() // TOOO: input handling
        {
            string url = "https://kayaposoft.com/enrico/json/v2.0/?action=getSupportedCountries";

            string response = await _client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<List<Country>>(response);

            return data;
        }

        public async Task<List<Holiday>> GetHolidaysByYearAndCountry(int? year, string country, string? region = null)
        {
            if (region != null)
                region = string.Format("&region={0}", region);

            string url = string.Format("https://kayaposoft.com/enrico/json/v2.0/?action=getHolidaysForYear&year={0}&country={1}&holidayType=public_holiday{2}", year, country, region);

            string apiResponse = await _client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<List<Holiday>>(apiResponse);

            return data;
        }

        public async Task<PublicHolidayState> GetDayHolidayStatus(DateTime date, string country, string? region = null)
        {
            if (region != null)
                region = string.Format("&region={0}", region);

            string url = string.Format("https://kayaposoft.com/enrico/json/v2.0/?action=isPublicHoliday&date={0}&country={1}{2}", date.ToString("dd-MM-yyyy"), country, region);

            string holidayApiResponse = await _client.GetStringAsync(url);

            var holidayData = JsonConvert.DeserializeObject<PublicHolidayState>(holidayApiResponse);

            return holidayData;
        }

        public async Task<WorkdayState> GetDayWorkdayStatus(DateTime date, string country, string? region = null)
        {
            if (region != null)
                region = string.Format("&region={0}", region);

            string url = string.Format("https://kayaposoft.com/enrico/json/v2.0/?action=isWorkDay&date={0}&country={1}{2}", date.ToString("dd-MM-yyyy"), country, region);

            string workdayApiResponse = await _client.GetStringAsync(url);

            var workdayData = JsonConvert.DeserializeObject<WorkdayState>(workdayApiResponse);

            return workdayData;
        }

    }
}
