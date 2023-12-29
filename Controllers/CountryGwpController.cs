using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Galytix.WebApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CountryGwpController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CountryGwpController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("avg")]
        public async Task<IActionResult> CalculateAverage([FromBody] GwpRequestModel requestModel)
        {
            try
            {
                Console.WriteLine($"Received Request: {JsonConvert.SerializeObject(requestModel)}");

                var projectRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", ".."); // Adjust the number of ".." based on your project structure
                var filePath = Path.Combine(projectRoot, "Data", "gwpByCountry.csv");

                Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");


                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest("CSV file not found.");
                }

                var data = await ReadCsv(filePath);

                var result = new Dictionary<string, double>();

                foreach (var lob in requestModel.Lob)
                {
                    var total = data
                        .Where(d => d.Country == requestModel.Country && d.LineOfBusiness == lob)
                        .SelectMany(d => d.YearData)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
                        .Sum();

                    result.Add(lob, total);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        private async Task<List<GwpDataModel>> ReadCsv(string filePath)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(filePath);

            var data = new List<GwpDataModel>();

            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');

                var model = new GwpDataModel
                {
                    Country = values[0].Trim(),
                    VariableId = values[1].Trim(),
                    VariableName = values[2].Trim(),
                    LineOfBusiness = values[3].Trim(),
                    YearData = values.Skip(4).Select(d => d.Trim()).ToList()
                };

                data.Add(model);
            }

            return data;
        }
    }

    public class GwpDataModel
    {
        public string Country { get; set; }
        public string VariableId { get; set; }
        public string VariableName { get; set; }
        public string LineOfBusiness { get; set; }
        public List<string> YearData { get; set; }
    }

    public class GwpRequestModel
    {
        public string Country { get; set; }
        public List<string> Lob { get; set; }
    }
}
