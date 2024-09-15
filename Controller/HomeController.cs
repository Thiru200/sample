
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyFirstApplication;
using ExcelDataReader;
using ClosedXML.Excel;
public class HomeController : Controller
{
    private static List<UserListModel> users = new List<UserListModel>();
    private static List<UserProperties> userProperties = new List<UserProperties>();
    public HomeController()
    {
        users.AddRange(new List<UserListModel>
        {
            new UserListModel() { EmpCode = "M0846715", EmpName = "thirumurugan.m" },
            new UserListModel() { EmpCode = "M0846714", EmpName = "karthi.m" },
            new UserListModel() { EmpCode = "M0846713", EmpName = "babu.j" }
        });
        userProperties.AddRange(new List<UserProperties>{
            new UserProperties(){PageName="Trainer",MandatoryColumns="EmpName,EmpCode",MandatoryDataTypes="string,string"}
        });
    }
    [HttpGet]
    public IActionResult Index(List<UserListModel> users)
    {
        var employees = users == null ? new List<UserListModel>() : users;
        return View(employees);
    }
    [HttpGet("DownloadTemplate")]
    public IActionResult DownloadTemplate()
    {
        var columnHeaders = userProperties
           .SelectMany(up => up.MandatoryColumns.Split(','))
           .Distinct()
           .ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sheet1");
            // Populate headers dynamically
            for (int i = 0; i < columnHeaders.Count; i++)
            {
                // worksheet.Cell(1, i + 1).Value = columnHeaders[i];
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = columnHeaders[i];
                // Set background color for the header row
                cell.Style.Fill.BackgroundColor = XLColor.LightGray; // You can use any color you prefer
                cell.Style.Font.Bold = true; // Optional: make the header text bold
            }
            // Save to memory stream
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0; // Reset stream position to the beginning
            // Return file as a download
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "example.xlsx");
        }
    }
    [HttpPost]
    [Obsolete]
    public JsonResult Index(IFormFile file)
    {
        var result = ValidateUsers(file);
        return Json(result);
    }
    public static List<Dictionary<string, object>> GetSampleUserData()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "EmpCode", "M0846712" },
                { "EmpName", "Alice" }
            },
            new Dictionary<string, object>
            {
                { "EmpCode", "M0846713" },
                { "EmpName", "Babu" }
            },
            new Dictionary<string, object>
            {
                { "EmpCode", "M0846714" },
                { "EmpName", "Charan" }
            }
            // Add more sample data as needed
        };
    }
    private bool ValidateUsers(IFormFile file)
    {
        return ValidateExcelTemplateAsync(file).Result;
    }
    public async Task<bool> ValidateExcelTemplateAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false; // No file uploaded
        }
        try
        {
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1); // Assuming template is in the first sheet
                                                       // Extract column headers from the worksheet
                var headersInSheet = Enumerable.Range(1, worksheet.ColumnCount())
                                               .Select(col => worksheet.Cell(1, col).GetString())
                                               .ToList();

                // Define the expected headers
                var expectedHeaders = userProperties
                                      .SelectMany(up => up.MandatoryColumns.Split(','))
                                      .Distinct()
                                      .ToList();

                // Check if headers match
                if (headersInSheet.Count != expectedHeaders.Count)
                {
                    return false; // Number of headers mismatch
                }
                // Sort and compare headers
                headersInSheet.Sort();
                expectedHeaders.Sort();

                if (!headersInSheet.SequenceEqual(expectedHeaders))
                {
                    return false;
                }
                // Validate rows
                int startRow = 2; // Assuming data starts from row 2
                var maxRow = worksheet.LastRowUsed().RowNumber();

                for (int rowNum = startRow; rowNum <= maxRow; rowNum++)
                {
                    var row = worksheet.Row(rowNum);
                    var rowData = new Dictionary<string, object>();
                    if (!ExcelHelper.ValidateRowDataTypes(row, userProperties))
                    {
                        return false; // Data type mismatch
                    }
                    // Populate rowData with column names as keys
                    foreach (var header in headersInSheet)
                    {
                        rowData[header] = row.Cell(headersInSheet.IndexOf(header) + 1).GetValue<object>();
                    }
                    var user = GetSampleUserData();
                    // Check if rowData exists in userData
                    if (!user.Any(user => ExcelHelper.CompareDictionaries(rowData, user)))
                    {
                        return false; // Data mismatch
                    }
                }
            }
        }
        catch
        {
            return false; // Error processing file
        }
        return true; // File matches expected template
    }
    private List<UserListModel> GetEmployees(string fName)
    {
        List<UserListModel> users = new List<UserListModel>();

        // Construct the file path correctly
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", fName);

        // Check if file exists
        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file was not found.", filePath);
        }

        // Register encoding provider
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // Open the file and read its contents
        using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            // Use a library-specific reader for Excel files
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                while (reader.Read())
                {
                    // Ensure that there are at least two columns
                    if (reader.FieldCount >= 2)
                    {
                        users.Add(new UserListModel()
                        {
                            EmpCode = reader.GetValue(0)?.ToString() ?? string.Empty,
                            EmpName = reader.GetValue(1)?.ToString() ?? string.Empty
                        });
                    }
                }
            }
        }

        return users;
    }

}

