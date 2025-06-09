namespace ContestCAE;

using System.Xml;

public class Course
{
    public string Name = string.Empty;
    public string CourseNumber = string.Empty;
    public string Description = string.Empty;
	public int Credits = 0;
}

public static class XmlParser
{
    public static List<Course> LoadCourses(string path)
    {
        var list = new List<Course>();
        Course? current = null;

        try
        {
            using var reader = XmlReader.Create(path);
			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "CourseDto":
							current = new Course();
							break;
						case "CourseNumber":
							if (current != null) current.CourseNumber = reader.ReadElementContentAsString();
							break;
						case "Name":
							if (current != null) current.Name = reader.ReadElementContentAsString();
							break;
						case "Description":
							if (current != null) current.Description = reader.ReadElementContentAsString();
							break;
						case "Credits":
							if (current != null && int.TryParse(reader.ReadElementContentAsString(), out var credits))
								current.Credits = credits;
							break;
					}
				}
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "CourseDto")
				{
					if (current != null && !string.IsNullOrEmpty(current.Name))
					{
						list.Add(current);
						current = null;
					}
				}
			}
		}
		catch (Exception ex)
        {
            Console.WriteLine($"Error parsing XML: {ex.Message}");
        }

        return list;
    }
}
