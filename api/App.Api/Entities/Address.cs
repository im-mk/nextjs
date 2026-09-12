public class Address
{
    public int Id { get; set; }
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string Postcode { get; set; } = default!;
    public string Country { get; set; } = "GB"; // ISO 3166-1 alpha-2
}