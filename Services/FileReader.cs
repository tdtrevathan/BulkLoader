using CsvHelper;
using System.Globalization;
using System.Threading.Tasks;

public static class FileReader<T>
{
    public static async Task<FileReaderResult<T>> ReadFile(string path)
    {
        var successfullRecords = new List<T>();
        var unsuccessfulRecords = new List<string>();

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        while (await csv.ReadAsync())
        {
            try
            {
                var record = csv.GetRecord<T>();
                successfullRecords.Add(record);
            }
            catch (Exception ex)
            {
                unsuccessfulRecords.Add(csv.Context.Parser.RawRecord ?? "NA");
            }
        }

        return new FileReaderResult<T>()
        {
            SuccessfullRecords = successfullRecords,
            UnprocessableRecords = unsuccessfulRecords
        };
    }
}