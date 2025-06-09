
public class Employee
{
    private string _employeeNumber;
    private string _title;
    private string _firstName;
    private string _lastName;
    private string _email;
    private string _phone;
    private string _fax;
    private string _branch;
    private string _branchCode;

    public string EmployeeNumber
    {
        get => _employeeNumber;
        set
        {
            _employeeNumber = SafeTruncate(value.Trim());
        }
    }
    public string Title
    {
        get => _title;
        set
        {
            _title = SafeTruncate(value.Trim());
        }
    }
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = SafeTruncate(value.Trim());
        }
    }
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = SafeTruncate(value.Trim());
        }
    }
    public string Email
    {
        get => _email;
        set
        {
            _email = SafeTruncate(value.Trim());
        }
    }
    public string Phone
    {
        get => _phone;
        set
        {
            _phone = SafeTruncate(value.Trim());
        }
    }
    public string Fax
    {
        get => _fax;
        set
        {
            _fax = SafeTruncate(value.Trim());
        }
    }
    public string Branch
    {
        get => _branch;
        set
        {
            _branch = SafeTruncate(value.Trim());
        }
    }
    public string BranchCode
    {
        get => _branchCode;
        set
        {
            _branchCode = SafeTruncate(value.Trim());
        }
    }

    public string Age { get; set; }

    public static string SafeTruncate(string input, int maxLength = 4000)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input.Length <= maxLength ? input : input.Substring(0, maxLength);
    }
}