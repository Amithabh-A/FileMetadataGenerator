using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dotnet run <directory-path>");
            return;
        }

        string directoryPath = args[0];

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Directory does not exist: {directoryPath}");
            return;
        }

        var metadata = new List<FileMetadata>();
        string metadataFilePath = Path.Combine(directoryPath, "metadata.json");

        foreach (var filePath in Directory.GetFiles(directoryPath))
        {
            // Skip the metadata file itself
            if (Path.GetFileName(filePath).Equals("metadata.json", StringComparison.OrdinalIgnoreCase))
                continue;

            var fileHash = await ComputeFileHashAsync(filePath);
            metadata.Add(new FileMetadata
            {
                FileName = Path.GetFileName(filePath),
                FileHash = fileHash
            });
        }

        // Write (or overwrite) the metadata.json file
        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(metadataFilePath, JsonSerializer.Serialize(metadata, options));
        Console.WriteLine($"Metadata file created/overwritten at: {metadataFilePath}");
    }

    private static async Task<string> ComputeFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

class FileMetadata
{
    public string FileName { get; set; }
    public string FileHash { get; set; }
}

