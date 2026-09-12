using System.Data;
using Dapper;

namespace App.Api.Repositories;

public class AddressesRepository(
    IDbConnection dbConnection) : IAddressesRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;

    public Task<Address?> Get(int addressId)
    {
        const string sql = @"
            SELECT id, address_line1, address_line2, address_line3, address_line4, postcode, country
            FROM public.addresses
            WHERE id = @AddressId;";

        return _dbConnection.QueryFirstOrDefaultAsync<Address?>(sql, new { AddressId = addressId });
    }

    public Task<int> Insert(Address address, IDbTransaction? transaction = null)
    {
        const string sql = @"
            INSERT INTO public.addresses (address_line1, address_line2, address_line3, address_line4, postcode, country)
            VALUES (@AddressLine1, @AddressLine2, @AddressLine3, @AddressLine4, @Postcode, @Country)
            RETURNING id;";

        return _dbConnection.QuerySingleAsync<int>(sql, address, transaction);
    }

    public Task<int> DeleteByIds(IEnumerable<int> ids, IDbTransaction? transaction = null)
    {
        const string sql = @"
            DELETE FROM public.addresses
            WHERE id = ANY(@Ids);";

        return _dbConnection.ExecuteAsync(sql, new { Ids = ids }, transaction);
    }
}
