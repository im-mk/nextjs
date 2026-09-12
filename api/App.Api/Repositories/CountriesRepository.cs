using App.Api.Entities;
using Dapper;
using ZiggyCreatures.Caching.Fusion;

namespace App.Api.Repositories;

public class CountriesRepository(
    System.Data.IDbConnection dbConnection,
    IFusionCache fusionCache) : ICountriesRepository
{
    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        const string cacheKey = "countries:all";

        return await fusionCache.GetOrSetAsync(
            cacheKey,
            async _ => await LoadCountriesFromDatabaseAsync(),
            TimeSpan.FromDays(1)
        );
    }

    private async Task<List<Country>> LoadCountriesFromDatabaseAsync()
    {
        const string sql = @"SELECT id AS Id, name AS Name FROM public.countries ORDER BY name";
        var rows = await dbConnection.QueryAsync<Country>(sql);
        return rows.ToList();
    }
}
