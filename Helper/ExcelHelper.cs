using ClosedXML.Excel;

namespace MyFirstApplication
{

    public class ExcelHelper
    {
        public static bool ValidateRowDataTypes(IXLRow row, IEnumerable<UserProperties> userProperties)
        {
            var columnTypes = userProperties
                              .SelectMany(up => up.MandatoryColumns.Split(','))
                              .Distinct()
                              .ToList();

            for (int i = 0; i < columnTypes.Count; i++)
            {
                var cell = row.Cell(i + 1);
                if (!IsValidDataType(cell, columnTypes[i]))
                {
                    return false; // Data type mismatch
                }
            }

            return true;
        }
        public static bool IsValidDataType(IXLCell cell, string expectedType)
        {
            switch (expectedType.ToLower())
            {
                case "string":
                    return cell.DataType == XLDataType.Text;
                case "number":
                    return cell.DataType == XLDataType.Number;
                case "date":
                    return cell.DataType == XLDataType.DateTime;
                // Add more cases as needed
                default:
                    return false;
            }
        }
        public static bool CompareDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key) || !Equals(dict1[key], dict2[key]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}