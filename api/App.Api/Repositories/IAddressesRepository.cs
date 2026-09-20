using App.Api.Entities;

namespace App.Api.Repositories;

public interface IAddressesRepository
{
    Task<Address?> Get(int addressId);
    Task<int> Insert(Address address, System.Data.IDbTransaction? transaction = null);
    Task<int> DeleteByIds(IEnumerable<int> ids, System.Data.IDbTransaction? transaction = null);
}
