using App.Api.Entities;

namespace App.Api.Repositories;

public interface IContactsRepository
{
    Task<Contact?> GetById(int id);
    Task<IEnumerable<Contact>> GetAll();
    Task<int> Create(Contact contact);
    Task<bool> Update(Contact contact);
    Task<bool> Delete(int id);
    Task<bool> EmailExists(string email, int? excludeId = null);
}
