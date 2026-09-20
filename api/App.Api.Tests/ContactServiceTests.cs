using App.Api.Entities;
using App.Api.Dto.Addresses;
using App.Api.Dto.Contacts;
using App.Api.Repositories;
using App.Api.Services;
using Moq;

namespace App.Api.Tests;

public class ContactServiceTests
{
    [Fact]
    public async Task CreateContact_IncludesAddress_WhenProvided()
    {
        var contactsRepo = new Mock<IContactsRepository>();
        contactsRepo
            .Setup(r => r.EmailExists("test@example.com", null))
            .ReturnsAsync(false);

        contactsRepo
            .Setup(r => r.Create(It.IsAny<Contact>()))
            .ReturnsAsync(42)
            .Callback<Contact>(contact =>
            {
                Assert.NotNull(contact.Address);
                Assert.Equal("123 Main St", contact.Address!.AddressLine1);
                Assert.Equal("SW1A 1AA", contact.Address.Postcode);
            });

        var service = new ContactService(contactsRepo.Object);

        var request = new CreateContactRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "test@example.com",
            Phone = "123",
            Company = "Analytical Engines",
            Address = new AddressRequest
            {
                AddressLine1 = "123 Main St",
                Postcode = "SW1A 1AA",
                Country = "GB"
            }
        };

        var result = await service.CreateContact(request);

        Assert.Equal(42, result.Id);
        Assert.NotNull(result.Address);
        Assert.Equal("123 Main St", result.Address!.AddressLine1);
        Assert.Equal("SW1A 1AA", result.Address.Postcode);
    }
}
