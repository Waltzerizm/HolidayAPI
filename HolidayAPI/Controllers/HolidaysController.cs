using Microsoft.AspNetCore.Mvc;
using HolidayAPI.Models;
using HolidayAPI.Services;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace HolidayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HolidaysController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetAllCountries")]
        public string GetAllCountries()
        {
            var databaseController = new DatabaseController(_configuration);
            string databaseResponse = databaseController.GetAllCountries();

            if(databaseResponse != "[]")
                return databaseResponse;

            var enricoApiService = new EnricoApiService();

            List<Country> countries = enricoApiService.GetSupportedCountries().Result;

            databaseController.PostAllCountriesAsync(countries);

            return JsonConvert.SerializeObject(countries);
        }

        [HttpGet("GetHolidaysByYearAndCountry/{year}/{country}")]
        public string GetHolidayByYearAndCountry(int year, string country, string? region = null)
        {
            var databaseController = new DatabaseController(_configuration);
            string databaseResponse = databaseController.GetHolidayByYearAndCountry(year, country, region);

            if (databaseResponse != "[]")
                return databaseResponse;

            var monthlyHolidays = new MonthlyHolidays[12].Select(x => new MonthlyHolidays { Month = 0, HolidayName = new List<string>() }).ToList();
            var enricoApiService = new EnricoApiService();

            List<Holiday> holidays = enricoApiService.GetHolidaysByYearAndCountry(year, country, region).Result;

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

            databaseController.PostHolidayByYearAndCountryAsync(monthlyHolidays, year, country, region);

            return JsonConvert.SerializeObject(monthlyHolidays);
        }

        [HttpGet("GetDayStatus/{date}/{country}")]
        public string GetDayStatus(DateTime date, string country, string? region = null)
        {
            var databaseController = new DatabaseController(_configuration);
            string databaseResponse = databaseController.GetDayStatus(date, country, region);

            if (databaseResponse != "[]")
                return databaseResponse;

            var enricoApiService = new EnricoApiService();
            var status = new DayState();

            bool isWorkday = enricoApiService.GetDayWorkdayStatus(date, country, region).Result.IsWorkday;
            bool isPublicHoliday = enricoApiService.GetDayHolidayStatus(date, country, region).Result.IsPublicHoliday;

            if (isWorkday)
            {
                status.DayStatus = "workday";

                databaseController.PostDayStatusAsync(status, date, country, region);

                return JsonConvert.SerializeObject(status);
            }

            if (isPublicHoliday)
            {
                status.DayStatus = "holiday";

                databaseController.PostDayStatusAsync(status, date, country, region);

                return JsonConvert.SerializeObject(status);
            }

            status.DayStatus = "free day";

            databaseController.PostDayStatusAsync(status, date, country, region);

            return JsonConvert.SerializeObject(status);
        }

        [HttpGet("GetMaxFreedays{year}/{country}")]
        public string GetMaxFreedays(int year, string country, string? region = null)
        {
            var databaseController = new DatabaseController(_configuration);
            string databaseResponse = databaseController.GetMaxFreedays(year, country, region);

            if (databaseResponse != "[]")
                return databaseResponse;

            var enricoApiService = new EnricoApiService();
            var yearlyData = new List<bool>(); // list of bools representing if the certain day of the year is a free day. yearlyData[n] = true - the n day is a free day and vice versa.
            var longestFreeday = new LongestFreeday();

            List<Holiday> yearOfHolidays = enricoApiService.GetHolidaysByYearAndCountry(year, country, region).Result;

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
            int longestCount = 2; // 2 since theres always saturday and sunday

            for (int i = 1; i < yearlyData.Count; i++) // Finds the longest free day
            {
                if (yearlyData[i - 1] == false || yearlyData[i] != yearlyData[i - 1])
                    count = 0;

                count++;

                if (count > longestCount)
                    longestCount = count;
            }

            longestFreeday.LongestFreedayCount = longestCount;

            databaseController.PostMaxFreedaysAsync(longestFreeday, year, country, region);

            return JsonConvert.SerializeObject(longestFreeday);
        }
    }
}
