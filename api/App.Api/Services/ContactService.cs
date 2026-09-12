using App.Api.Entities;
using App.Api.Mappers;
using App.Api.Models;
using App.Api.Repositories;

namespace App.Api.Services;

public class ContactService(
    IContactsRepository contactsRepository) : IContactService
{
    private readonly IContactsRepository _contactsRepository = contactsRepository;

    public async Task<ContactResponse> CreateContact(CreateContactRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("FirstName is required");
        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("LastName is required");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        if (request.Address != null)
        {
            if (string.IsNullOrWhiteSpace(request.Address.AddressLine1))
                throw new ArgumentException("AddressLine1 is required when providing an address");
            if (string.IsNullOrWhiteSpace(request.Address.Postcode))
                throw new ArgumentException("Postcode is required when providing an address");
            if (string.IsNullOrWhiteSpace(request.Address.Country))
                throw new ArgumentException("Country is required when providing an address");
        }

        // Check for duplicate email
        if (await _contactsRepository.EmailExists(request.Email))
            throw new InvalidOperationException("Email already exists");

        var contact = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Address = request.Address == null ? null : AddressMapper.FromRequest(request.Address),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = await _contactsRepository.Create(contact);

        return new ContactResponse
        {
            Id = id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Company = contact.Company,
            Address = request.Address,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }

    public async Task<ContactResponse?> GetContact(int id)
    {
        var contact = await _contactsRepository.GetById(id);
        if (contact == null)
            return null;

        return MapToResponse(contact);
    }

    public async Task<IEnumerable<ContactResponse>> GetAllContacts()
    {
        var contacts = await _contactsRepository.GetAll();
        return contacts.Select(MapToResponse);
    }

    public async Task<bool> UpdateContact(int id, UpdateContactRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("FirstName is required");
        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("LastName is required");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        // Get existing contact
        var contact = await _contactsRepository.GetById(id);
        if (contact == null)
            return false;

        // Check for duplicate email (exclude current contact)
        if (contact.Email != request.Email && await _contactsRepository.EmailExists(request.Email, id))
            throw new InvalidOperationException("Email already exists");

        if (request.Address != null)
        {
            if (string.IsNullOrWhiteSpace(request.Address.AddressLine1))
                throw new ArgumentException("AddressLine1 is required when providing an address");
            if (string.IsNullOrWhiteSpace(request.Address.Postcode))
                throw new ArgumentException("Postcode is required when providing an address");
            if (string.IsNullOrWhiteSpace(request.Address.Country))
                throw new ArgumentException("Country is required when providing an address");
        }

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Company = request.Company;
        contact.Address = request.Address == null ? null : AddressMapper.FromRequest(request.Address);

        return await _contactsRepository.Update(contact);
    }

    public async Task<bool> DeleteContact(int id)
    {
        return await _contactsRepository.Delete(id);
    }

    public async Task<bool> CheckEmailExists(string email, int? excludeContactId = null)
    {
        var exists = await _contactsRepository.EmailExists(email);
        if (!exists)
            return false;

        // If we're excluding a specific contact, check if the existing email belongs to that contact
        if (excludeContactId.HasValue)
        {
            var contact = await _contactsRepository.GetById(excludeContactId.Value);
            if (contact?.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true)
                return false; // Email exists but belongs to the same contact
        }

        return true;
    }

    private static ContactResponse MapToResponse(Contact contact)
    {
        return new ContactResponse
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Company = contact.Company,
            Address = contact.Address == null ? null : new AddressRequest
            {
                AddressLine1 = contact.Address.AddressLine1,
                AddressLine2 = contact.Address.AddressLine2,
                AddressLine3 = contact.Address.AddressLine3,
                AddressLine4 = contact.Address.AddressLine4,
                Postcode = contact.Address.Postcode,
                Country = contact.Address.Country
            },
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}
