using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScannerAndPicker.Models;
using System.Globalization;

namespace ScannerAndPicker.Controllers;

[Authorize]
public class CsvViewerController : Controller
{
    private readonly ILogger<CsvViewerController> _logger;

    public CsvViewerController(ILogger<CsvViewerController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Display CSV content that was downloaded client-side and passed via session storage
    /// </summary>
    [HttpGet]
    public IActionResult DisplayFromSession()
    {
        // This page will read CSV content from sessionStorage via JavaScript
        // and display it in a table
        return View();
    }

    /// <summary>
    /// API endpoint to parse CSV content posted from the client
    /// </summary>
    [HttpPost]
    public IActionResult ParseCsv([FromBody] CsvContentRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.CsvContent))
            {
                return BadRequest("No CSV content provided");
            }

            var csvData = ParseCsvContent(request.CsvContent);
            csvData.FileName = request.FileName ?? "CSV Data";

            return Json(csvData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV content");
            return StatusCode(500, "Error parsing CSV content");
        }
    }

    private CsvDataViewModel ParseCsvContent(string csvContent)
    {
        var result = new CsvDataViewModel();

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, config);

            // Read the header
            csv.Read();
            csv.ReadHeader();
            result.Headers = csv.HeaderRecord?.ToList() ?? new List<string>();

            // Read all rows
            while (csv.Read())
            {
                var row = new List<string>();
                for (int i = 0; i < result.Headers.Count; i++)
                {
                    row.Add(csv.GetField(i) ?? string.Empty);
                }
                result.Rows.Add(row);
            }
        }
        catch (Exception)
        {
            // If CSV parsing fails, try simple line-by-line parsing
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                result.Headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
                for (int i = 1; i < lines.Length; i++)
                {
                    var row = lines[i].Split(',').Select(c => c.Trim()).ToList();
                    result.Rows.Add(row);
                }
            }
        }

        return result;
    }
}
