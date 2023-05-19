namespace certman.Extensions;

public static class StringExtensions
{
    public static string ThrowIfFileExists(this string path) => File.Exists(path) ? throw new Exception($"File {path} already exists") : path;
    public static string ThrowIfFileNotExists(this string path) => !File.Exists(path) ? throw new Exception($"File {path} does not exist") : path;
    
    public static string ThrowIfDirectoryExists(this string path) => Directory.Exists(path) ? throw new Exception($"Directory {path} already exists") : path;
    public static string ThrowIfDirectoryNotExists(this string path) => !Directory.Exists(path) ? throw new Exception($"Directory {path} does not exist") : path;
    
    public static string SanitizeFileName(this string fileName)
    {
        fileName = fileName.Replace(' ', '-');
        
        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat(new[] { ' ', '&', ';', '|', '$', '`', '!', '\"', '\'', '(', ')', '*', '?', '[', ']', '#' })
            .ToArray();

        return new string(fileName
            .Where(x => !invalidChars.Contains(x))
            .ToArray());
    }
}