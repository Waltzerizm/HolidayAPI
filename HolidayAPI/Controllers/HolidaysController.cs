using Microsoft.AspNetCore.Mvc;
using HolidayAPI.Models;
using HolidayAPI.Services;

namespace HolidayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly ILogger<HolidaysController> _logger;

        public HolidaysController(ILogger<HolidaysController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetAllCountries")]
        public IEnumerable<Country> GetAllCountries()
        {
            var enricoApiService = new EnricoApiService();

            List<Country> countries = enricoApiService.GetSupportedCountries().Result;

            return countries;
        }

        [HttpGet("GetHolidaysByYearAndCountry/{year}/{country}")]
        public IEnumerable<MonthlyHolidays> GetHolidayByYearAndCountry(int year, string country) // TODO: handle "lt" or "lithuania"
        {
            var monthlyHolidays = new MonthlyHolidays[12].Select(x => new MonthlyHolidays { Month = 0, HolidayName = new List<string>() }).ToArray();
            var enricoApiService = new EnricoApiService();

            List<Holiday> holidays = enricoApiService.GetHolidaysByYearAndCountry(year, country).Result;

            // could async find the longest freeday here and put to DB

            for (int i = 0; i < 12; i++)
            {
                monthlyHolidays[i].Month = i + 1;

                foreach (Holiday holiday in holidays)
                {
                    if (holiday.Date.month.Equals(i + 1))
                    {
                        monthlyHolidays[i].HolidayName.Add(holiday.Name.Last().Text);
                    }
                }
            }

            return monthlyHolidays;
        }

        [HttpGet("GetDayStatus/{date}/{country}")]
        public DayState GetDayStatus(DateTime date, string country)
        {
            var enricoApiService = new EnricoApiService();
            var status = new DayState();

            bool isWorkday = enricoApiService.GetDayWorkdayStatus(date, country).Result.IsWorkday;
            bool isPublicHoliday = enricoApiService.GetDayHolidayStatus(date, country).Result.IsPublicHoliday;

            if (isWorkday)
            {
                status.DayStatus = "workday";
                return status;
            }

            if (isPublicHoliday)
            {
                status.DayStatus = "holiday";
                return status;
            }

            status.DayStatus = "free day";
            return status;
        }
        
        [HttpGet("GetMaxFreedays{year}/{country}")]
        public LongestFreeday GetMaxFreedays(int year, string country)
        {
            var enricoApiService = new EnricoApiService();
            var yearlyData = new List<bool>();
            var longestFreeday = new LongestFreeday();

            List<Holiday> yearOfHolidays = enricoApiService.GetHolidaysByYearAndCountry(year, country).Result;

            for (int i = 0; i < 12; i++) // Initializes data and marks weekends as a free day
            {
                for (int j = 0; j < DateTime.DaysInMonth(year, i + 1); j++)
                {
                    DateTime date = new DateTime(year, i + 1, j + 1);

                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        yearlyData.Add(true);
                    else
                        yearlyData.Add(false);
                }
            }

            foreach (Holiday holiday in yearOfHolidays) // Marks the holidays as a free day
            {
                int monthSum = 0;

                for (int k = 0; k < holiday.Date.month - 1; k++)
                    monthSum += DateTime.DaysInMonth(year, k + 1);

                yearlyData[monthSum + holiday.Date.day - 1] = true;
            }

            int count = 1;
            int longestCount = 1;

            for (int i = 1; i < yearlyData.Count; i++) // Finds the longest free day
            {
                if (yearlyData[i - 1] == false || yearlyData[i] != yearlyData[i - 1])
                    count = 0;

                count++;

                if (count > longestCount)
                    longestCount = count;
            }

            longestFreeday.LongestFreedayCount = longestCount;

            return longestFreeday;
        }
    }
}
