using App.Api.Dto.Addresses;

namespace App.Api.Dto.Contacts;

public class CreateContactRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public AddressRequest? Address { get; set; }
}