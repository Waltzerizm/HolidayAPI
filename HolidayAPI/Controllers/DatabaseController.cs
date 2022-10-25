using Microsoft.AspNetCore.Mvc;
using HolidayAPI.Models;
using HolidayAPI.Services;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;

namespace HolidayAPI.Controllers
{
    public class DatabaseController
    {
        private readonly IConfiguration _configuration;

        public DatabaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetAllCountries()
        {
            string query = @"SELECT * FROM dbo.countries";

            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");

            var table = GetDataTable(query);

            var result = table.AsEnumerable().GroupBy(country => new
            {
                FullName = country.Field<string>("fullName"),
                Code = country.Field<string>("countryCode")
            }).Select(group => new
            {
                FullName = group.Key.FullName,
                Code = group.Key.Code,
                Regions = new List<string>(group.Where(country => country.Field<string>("regions") != null).Select(country => country.Field<string>("regions"))) // magic
            }).ToList();

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(result);
        }

        public string GetHolidayByYearAndCountry(int year, string country, string? region = null)
        {
            string query = "SELECT * FROM dbo.holidays WHERE country = @country AND year = @year";

            var table = GetDataTable(query, country: country, year: year, region: region);

            var result = table.AsEnumerable().GroupBy(holiday => new
            {
                Month = holiday.Field<int>("month")
            }).Select(group => new
            {
                Month = group.Key.Month,
                HolidayName = new List<string>(group.Where(country => country.Field<string>("holidayName") != null)
                .Select(country => country.Field<string>("holidayName")))
            }).ToList();

            return JsonConvert.SerializeObject(result);
        }

        public string GetDayStatus(DateTime date, string country, string? region = null)
        {
            string query = "SELECT day_status FROM dbo.day_status WHERE country = @country AND status_date = @date";

            var table = GetDataTable(query, country: country, date: date, region: region);

            return JsonConvert.SerializeObject(table);
        }

        public string GetMaxFreedays(int year, string country, string? region = null)
        {
            string query = "SELECT longest_count FROM dbo.longest_freeday WHERE country = @country AND year = @year";

            var table = GetDataTable(query, country: country, year: year, region: region);

            return JsonConvert.SerializeObject(table);
        }

        public DataTable GetDataTable(string query, DateTime? date = null, int? year = null, string? country = null, string? region = null)
        {
            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");
            var sql = new StringBuilder(query);
            var table = new DataTable();
            SqlDataReader myReader;

            if (region != null) sql.Append(" AND region = @region");

            using (var connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (var command = new SqlCommand(sql.ToString(), connection))
                {
                    if (date != null) command.Parameters.AddWithValue("@date", date?.ToString("yyyy-MM-dd"));
                    if (year != null) command.Parameters.AddWithValue("@year", year);
                    if (country != null) command.Parameters.AddWithValue("@country", country);
                    if (region != null) command.Parameters.AddWithValue("@region", region);

                    myReader = command.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    connection.Close();
                }
            }

            return table;
        }

        public void PostAllCountries(List<Country> countries)
        {
            string query = "INSERT INTO dbo.countries(fullName, countryCode, regions) VALUES(@fullName, @countryCode, @regions)";

            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");
            var sql = new StringBuilder(query);
            SqlDataReader myReader;

            using (var connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (var command = new SqlCommand(sql.ToString(), connection))
                {
                    command.Parameters.Add("@fullName", SqlDbType.NVarChar);
                    command.Parameters.Add("@countryCode", SqlDbType.NVarChar);
                    command.Parameters.Add("@regions", SqlDbType.NVarChar);

                    foreach (Country country in countries)
                    {
                        command.Parameters["@fullName"].Value = country.FullName;
                        command.Parameters["@countryCode"].Value = country.CountryCode;

                        foreach (string region in country.Regions)
                        {
                            command.Parameters["@regions"].Value = region ?? Convert.DBNull;
                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }

        public void PostHolidayByYearAndCountry(List<MonthlyHolidays> holidays, int year, string country, string? region = null)
        {
            string query = "INSERT INTO dbo.holidays(country, region, year, month, holidayName) VALUES(@country, @region, @year, @month, @holidayName)";

            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");

            SqlDataReader myReader;

            using (var connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@country", country);
                    command.Parameters.AddWithValue("@region", region ?? Convert.DBNull);
                    command.Parameters.AddWithValue("@year", year);
                    command.Parameters.Add("@month", SqlDbType.Int);
                    command.Parameters.Add("@holidayName", SqlDbType.NVarChar);

                    foreach (MonthlyHolidays month in holidays)
                    {
                        command.Parameters["@month"].Value = month.Month;

                        foreach (string holiday in month.HolidayName)
                        {
                            command.Parameters["@holidayName"].Value = holiday ?? Convert.DBNull;
                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
        }

        public void PostDayStatus(DayState state, DateTime date, string country, string? region = null)
        {
            string query = "INSERT INTO dbo.day_status(country, region, status_date, day_status) VALUES(@country, @region, @statusDate, @dayStatus)";

            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");

            SqlDataReader myReader;

            using (var connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@country", country);
                    command.Parameters.AddWithValue("@region", region ?? Convert.DBNull);
                    command.Parameters.AddWithValue("@statusDate", date.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@dayStatus", state.DayStatus);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public void PostMaxFreedays(LongestFreeday longestFreeday, int year, string country, string? region = null)
        {
            string query = "INSERT INTO dbo.longest_freeday(country, region, year, longest_count) VALUES(@country, @region, @year, @longest_count)";

            string sqlDataSource = _configuration.GetConnectionString("HolidayDbCon");

            SqlDataReader myReader;

            using (var connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@country", country);
                    command.Parameters.AddWithValue("@region", region ?? Convert.DBNull);
                    command.Parameters.AddWithValue("@year", year);
                    command.Parameters.AddWithValue("@longest_count", longestFreeday.LongestFreedayCount);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}
