
using System.ComponentModel.DataAnnotations;

namespace App.Api.Models;

public class AddressRequest
{
    [Required]
    [StringLength(255)]
    public string AddressLine1 { get; set; } = default!;

    [StringLength(255)]
    public string? AddressLine2 { get; set; }

    [StringLength(255)]
    public string? AddressLine3 { get; set; }

    [StringLength(255)]
    public string? AddressLine4 { get; set; }

    [Required]
    [StringLength(10)]
    public string Postcode { get; set; } = default!;

    [Required]
    [StringLength(2)]
    public string Country { get; set; } = "GB";
}


