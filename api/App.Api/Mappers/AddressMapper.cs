using App.Api.Models;

namespace App.Api.Mappers;

public static class AddressMapper
{
    public static Address FromRequest(AddressRequest request)
    {
        return new Address
        {
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            AddressLine3 = request.AddressLine3,
            AddressLine4 = request.AddressLine4,
            Postcode = request.Postcode,
            Country = request.Country
        };
    }
}
