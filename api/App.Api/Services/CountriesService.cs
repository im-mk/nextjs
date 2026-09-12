using App.Api.Entities;
using App.Api.Repositories;

namespace App.Api.Services;

public class CountriesService : ICountriesService
{
    private readonly ICountriesRepository _countriesRepository;

    public CountriesService(ICountriesRepository countriesRepository)
    {
        _countriesRepository = countriesRepository;
    }

    public Task<IEnumerable<Country>> GetAllCountriesAsync()
    {
        return _countriesRepository.GetAllAsync();
    }
}
