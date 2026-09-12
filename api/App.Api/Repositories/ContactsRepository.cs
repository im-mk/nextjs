using System.Data;
using App.Api.Entities;
using Dapper;

namespace App.Api.Repositories;

public class ContactsRepository(
    IDbConnection dbConnection) : IContactsRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;

    public async Task<Contact?> GetById(int id)
    {
        var sql = @"SELECT c.id, c.first_name as FirstName, c.last_name as LastName, c.email, c.phone, c.company,
                     c.created_at as CreatedAt, c.updated_at as UpdatedAt,
                     a.id as AddressId, a.address_line1 as AddressLine1, a.address_line2 as AddressLine2,
                     a.address_line3 as AddressLine3, a.address_line4 as AddressLine4,
                     a.postcode as Postcode, a.country as Country
              FROM public.contacts c
              LEFT JOIN public.contact_addresses ca ON ca.contact_id = c.id
              LEFT JOIN public.addresses a ON a.id = ca.address_id
              WHERE c.id = @id
              ORDER BY ca.id DESC
              LIMIT 1";

        var contacts = await _dbConnection.QueryAsync<Contact, Address, Contact>(
            sql,
            (contact, address) =>
            {
                contact.Address = address;
                return contact;
            },
            new { id },
            splitOn: "AddressId");

        return contacts.FirstOrDefault();
    }

    public async Task<IEnumerable<Contact>> GetAll()
    {
        var sql = @"SELECT c.id, c.first_name as FirstName, c.last_name as LastName, c.email, c.phone, c.company,
                     c.created_at as CreatedAt, c.updated_at as UpdatedAt,
                     a.id as AddressId, a.address_line1 as AddressLine1, a.address_line2 as AddressLine2,
                     a.address_line3 as AddressLine3, a.address_line4 as AddressLine4,
                     a.postcode as Postcode, a.country as Country
                     FROM public.contacts c
                     LEFT JOIN (
                         SELECT DISTINCT ON (contact_id) id, contact_id, address_id
                         FROM public.contact_addresses
                         ORDER BY contact_id, id DESC
                     ) ca ON ca.contact_id = c.id
              LEFT JOIN public.addresses a ON a.id = ca.address_id
              ORDER BY c.created_at DESC";

        return await _dbConnection.QueryAsync<Contact, Address, Contact>(
            sql,
            (contact, address) =>
            {
                contact.Address = address;
                return contact;
            },
            splitOn: "AddressId");
    }

    public async Task<int> Create(Contact contact)
    {
        var id = await _dbConnection.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(id), 0) + 1 FROM public.contacts");
        var now = DateTime.UtcNow;

        await _dbConnection.ExecuteAsync(
            @"INSERT INTO public.contacts (id, first_name, last_name, email, phone, company, created_at, updated_at)
              VALUES (@id, @firstName, @lastName, @email, @phone, @company, @createdAt, @updatedAt)",
            new
            {
                id,
                firstName = contact.FirstName,
                lastName = contact.LastName,
                email = contact.Email,
                phone = contact.Phone,
                company = contact.Company,
                createdAt = now,
                updatedAt = now
            });

        if (contact.Address != null)
        {
            var addressId = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(MAX(id), 0) + 1 FROM public.addresses");

            await _dbConnection.ExecuteAsync(
                @"INSERT INTO public.addresses (id, address_line1, address_line2, address_line3, address_line4, postcode, country)
                  VALUES (@id, @addressLine1, @addressLine2, @addressLine3, @addressLine4, @postcode, @country)",
                new
                {
                    id = addressId,
                    addressLine1 = contact.Address.AddressLine1,
                    addressLine2 = contact.Address.AddressLine2,
                    addressLine3 = contact.Address.AddressLine3,
                    addressLine4 = contact.Address.AddressLine4,
                    postcode = contact.Address.Postcode,
                    country = contact.Address.Country
                });

            var contactAddressId = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(MAX(id), 0) + 1 FROM public.contact_addresses");

            await _dbConnection.ExecuteAsync(
                @"INSERT INTO public.contact_addresses (id, contact_id, address_id, address_type)
                  VALUES (@id, @contactId, @addressId, @addressType)",
                new
                {
                    id = contactAddressId,
                    contactId = id,
                    addressId,
                    addressType = "Billing"
                });
        }

        return id;
    }

    public async Task<bool> Update(Contact contact)
    {
        contact.UpdatedAt = DateTime.UtcNow;

        var result = await _dbConnection.ExecuteAsync(
            @"UPDATE public.contacts 
              SET first_name = @firstName, last_name = @lastName, email = @email, 
                  phone = @phone, company = @company, updated_at = @updatedAt
              WHERE id = @id",
            new
            {
                firstName = contact.FirstName,
                lastName = contact.LastName,
                email = contact.Email,
                phone = contact.Phone,
                company = contact.Company,
                updatedAt = contact.UpdatedAt,
                id = contact.Id
            });

        if (contact.Address == null)
        {
            await _dbConnection.ExecuteAsync(
                "DELETE FROM public.contact_addresses WHERE contact_id = @contactId",
                new { contactId = contact.Id });
            return result > 0;
        }

        var existingAddressId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(
            "SELECT address_id FROM public.contact_addresses WHERE contact_id = @contactId ORDER BY id DESC LIMIT 1",
            new { contactId = contact.Id });

        if (existingAddressId.HasValue)
        {
            await _dbConnection.ExecuteAsync(
                @"UPDATE public.addresses
                  SET address_line1 = @addressLine1, address_line2 = @addressLine2, address_line3 = @addressLine3,
                      address_line4 = @addressLine4, postcode = @postcode, country = @country
                  WHERE id = @id",
                new
                {
                    id = existingAddressId.Value,
                    addressLine1 = contact.Address.AddressLine1,
                    addressLine2 = contact.Address.AddressLine2,
                    addressLine3 = contact.Address.AddressLine3,
                    addressLine4 = contact.Address.AddressLine4,
                    postcode = contact.Address.Postcode,
                    country = contact.Address.Country
                });
        }
        else
        {
            var addressId = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(MAX(id), 0) + 1 FROM public.addresses");

            await _dbConnection.ExecuteAsync(
                @"INSERT INTO public.addresses (id, address_line1, address_line2, address_line3, address_line4, postcode, country)
                  VALUES (@id, @addressLine1, @addressLine2, @addressLine3, @addressLine4, @postcode, @country)",
                new
                {
                    id = addressId,
                    addressLine1 = contact.Address.AddressLine1,
                    addressLine2 = contact.Address.AddressLine2,
                    addressLine3 = contact.Address.AddressLine3,
                    addressLine4 = contact.Address.AddressLine4,
                    postcode = contact.Address.Postcode,
                    country = contact.Address.Country
                });

            var contactAddressId = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(MAX(id), 0) + 1 FROM public.contact_addresses");

            await _dbConnection.ExecuteAsync(
                @"INSERT INTO public.contact_addresses (id, contact_id, address_id, address_type)
                  VALUES (@id, @contactId, @addressId, @addressType)",
                new
                {
                    id = contactAddressId,
                    contactId = contact.Id,
                    addressId,
                    addressType = "Billing"
                });
        }

        return result > 0;
    }

    public async Task<bool> Delete(int id)
    {
        await _dbConnection.ExecuteAsync(
            "DELETE FROM public.contact_addresses WHERE contact_id = @id",
            new { id });

        var result = await _dbConnection.ExecuteAsync(
            "DELETE FROM public.contacts WHERE id = @id",
            new { id });

        return result > 0;
    }

    public async Task<bool> EmailExists(string email, int? excludeId = null)
    {
        var sql = "SELECT COUNT(1) FROM public.contacts WHERE email = @email";
        object parameters = new { email };

        if (excludeId.HasValue)
        {
            sql += " AND id != @excludeId";
            parameters = new { email, excludeId };
        }

        var count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }
}
