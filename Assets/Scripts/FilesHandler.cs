using System.IO;
using System;

public static class FilesHandler
{
    /// <param name="originFileFullName">Full file path before moving</param>
    /// <param name="destinationFileFullName">Full file path after moving</param>
    public static void MoveFile(string originFileFullName, string destinationFileFullName)
    {
        if (String.IsNullOrEmpty(originFileFullName))
        {
            throw new Exception("'originFileFullName' is null or empty");
        }

        if (String.IsNullOrEmpty(destinationFileFullName))
        {
            throw new Exception("'destinationFileFullName' is null or empty");
        }

        if (File.Exists(destinationFileFullName))
        {
            throw new Exception("'destinationFileFullName' already exists");
        }


        try
        {
            var dirPath = Path.GetDirectoryName(destinationFileFullName);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);


            File.Move(originFileFullName, destinationFileFullName);
        }
        catch (IOException ex)
        {
            throw new Exception($"Error while moving file: {ex}");
        }
    }
}
