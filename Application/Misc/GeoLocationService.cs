using System;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using SharedLibraryCore.Interfaces;

namespace IW4MAdmin.Application.Misc;

public class GeoLocationService : IGeoLocationService, IDisposable
{
    private readonly DatabaseReader _reader;
    
    public GeoLocationService(string sourceAddress)
    {
        try
        {
            _reader = new DatabaseReader(sourceAddress);
        }
        catch
        {
            // ignored
        }
    }
    
    public Task<IGeoLocationResult> Locate(string address)
    {
        CountryResponse country = null;
        
        if (_reader != null)
        {
            try
            {
                country = _reader.Country(address);
            }
            catch
            {
                // ignored
            }
        }

        var response = new GeoLocationResult
        {
            Country = country?.Country.Name ?? "Unknown",
            CountryCode = country?.Country.IsoCode ?? ""
        };

        return Task.FromResult((IGeoLocationResult)response);
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
