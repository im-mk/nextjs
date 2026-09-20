using App.Api.Dto.Contacts;

namespace App.Api.Services;

public interface IContactService
{
    Task<ContactResponse> CreateContact(CreateContactRequest request);
    Task<ContactResponse?> GetContact(int id);
    Task<IEnumerable<ContactResponse>> GetAllContacts();
    Task<bool> UpdateContact(int id, UpdateContactRequest request);
    Task<bool> DeleteContact(int id);
    Task<bool> CheckEmailExists(string email, int? excludeContactId = null);
}
