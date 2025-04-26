using CsvHelper;
using System.Globalization;

public static class FileReader<T>{
    public static List<T> ReadFile(string path){
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<T>().ToList();
    }
}