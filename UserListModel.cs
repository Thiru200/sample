namespace MyFirstApplication
{
    public class UserListModel
    {
        public string? EmpCode { get; set; }
        public string? EmpName { get; set; }
    }
    public class UserProperties
    {
        public string PageName { get; set; } = string.Empty;
        public string MandatoryColumns { get; set; } = null;
        public string MandatoryDataTypes { get; set; } = null;

    }
}