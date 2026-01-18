using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedLibraryCore.Interfaces
{
    /// <summary>
    ///     defines the capabilities of the plugin importer
    /// </summary>
    public interface IPluginImporter
    {
        /// <summary>
        ///     discovers C# assembly plugin and command types
        /// </summary>
        /// <returns>tuple of IPlugin implementation type definitions, and IManagerCommand type definitions</returns>
        Task<(IEnumerable<Type>, IEnumerable<Type>, IEnumerable<Type>)> DiscoverAssemblyPluginImplementationsAsync();

        /// <summary>
        ///     discovers the script plugins
        /// </summary>
        /// <returns>initialized script plugin collection</returns>
        Task<IEnumerable<(Type, string)>> DiscoverScriptPluginsAsync();
    }
}
