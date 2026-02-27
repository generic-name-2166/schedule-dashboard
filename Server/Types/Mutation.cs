namespace Server.Types;

using HotChocolate.Types;

[MutationType]
public static class Mutation
{
    public static async Task<bool> UploadFileAsync(IFile file)
    {
        var fileName = file.Name;
        var fileSize = file.Length;

        await using Stream stream = file.OpenReadStream();

        // We can now work with standard stream functionality of .NET
        // to handle the file.
        return true;
    }
}
