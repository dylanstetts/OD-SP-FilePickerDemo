namespace ScannerAndPicker.Models;

public class CsvDataViewModel
{
    public string FileName { get; set; } = "CSV Data";
    public List<string> Headers { get; set; } = new List<string>();
    public List<List<string>> Rows { get; set; } = new List<List<string>>();
}
