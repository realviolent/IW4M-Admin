using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLibraryCore.Dtos;

namespace SharedLibraryCore.Interfaces
{
    public interface IConfigurationFileService
    {
        Task<IEnumerable<ConfigurationFileDto>> GetConfigurationFilesAsync();
        Task WriteConfigurationFileAsync(string fileName, string content);
    }
}
