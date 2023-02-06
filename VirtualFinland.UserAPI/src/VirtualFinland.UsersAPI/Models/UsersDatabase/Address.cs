using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Address
{
    [MaxLength(512)]
    public string? StreetAddress { get; set; }

    [MaxLength(5)]
    public string? ZipCode { get; set; }

    [MaxLength(512)]
    public string? City { get; set; }

    [MaxLength(512)]
    public string? Country { get; set; }
    
    public Guid Id { get; set; }
}
