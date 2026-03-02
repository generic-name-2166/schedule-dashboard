using System.Text;
using nietras.SeparatedValues;

namespace Server.Types;

[MutationType]
public static class Mutation
{
    public static async Task<bool> UploadFileAsync(IFile file)
    {
        var fileName = file.Name;
        var fileSize = file.Length;

        Console.WriteLine($"{fileName} {fileSize}");

        try
        {
            await using Stream stream = file.OpenReadStream();
            string text = await new StreamReader(stream, Encoding.UTF8).ReadToEndAsync();
            using var reader = Sep.Reader().FromText(text);

            foreach (var row in reader)
            {
                var id = row["Ид"].Span;
                Console.WriteLine(id.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.GetType()} {ex.Message}");
        }

        // We can now work with standard stream functionality of .NET
        // to handle the file.
        return true;
    }
}
