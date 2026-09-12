using App.Api.Entities;

namespace App.Api.Services;

public interface ICountriesService
{
    Task<IEnumerable<Country>> GetAllCountriesAsync();
}
