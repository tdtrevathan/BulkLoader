public class FileReaderResult<T>
{
    public List<T> SuccessfullRecords {get; set;} = new List<T>();
    public List<string> UnprocessableRecords {get; set;} = new List<string>();
}