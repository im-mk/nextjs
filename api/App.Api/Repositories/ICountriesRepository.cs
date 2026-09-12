using App.Api.Entities;

namespace App.Api.Repositories;

public interface ICountriesRepository
{
    Task<IEnumerable<Country>> GetAllAsync();
}
