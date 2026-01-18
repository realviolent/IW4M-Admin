using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharedLibraryCore.Dtos;
using SharedLibraryCore.Interfaces;

namespace SharedLibraryCore.Services
{
    public class ConfigurationFileService : IConfigurationFileService
    {
        public async Task<IEnumerable<ConfigurationFileDto>> GetConfigurationFilesAsync()
        {
            var configDir = Path.Join(Utilities.OperatingDirectory, "Configuration");
            if (!Directory.Exists(configDir))
            {
                return Enumerable.Empty<ConfigurationFileDto>();
            }

            var files = Directory.GetFiles(configDir)
                .Where(file => file.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase));

            return await Task.WhenAll(files.Select(async fileName => new ConfigurationFileDto
            {
                FileName = Path.GetFileName(fileName),
                Content = await File.ReadAllTextAsync(fileName)
            }));
        }

        public async Task WriteConfigurationFileAsync(string fileName, string content)
        {
            // Sanitize filename to prevent directory traversal
            var safeFileName = Path.GetFileName(fileName);
            var path = Path.Join(Utilities.OperatingDirectory, "Configuration", safeFileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Configuration file {safeFileName} does not exist.");
            }

            await File.WriteAllTextAsync(path, content);
        }
    }
}
