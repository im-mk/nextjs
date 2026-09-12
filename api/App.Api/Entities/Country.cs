namespace App.Api.Entities;

public class Country
{
    public string Id { get; set; } = default!; // ISO 3166-1 alpha-2
    public string Name { get; set; } = default!;
}
