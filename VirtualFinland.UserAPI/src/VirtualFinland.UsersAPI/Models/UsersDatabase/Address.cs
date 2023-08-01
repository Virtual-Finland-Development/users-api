using System.ComponentModel.DataAnnotations;

namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public class Address : Encryptable
{
    //[MaxLength(512)]
    [Encrypted]
    public string? StreetAddress { get; set; }

    //[MaxLength(5)]
    [Encrypted]
    public string? ZipCode { get; set; }

    //[MaxLength(512)]
    [Encrypted]
    public string? City { get; set; }

    //[MaxLength(512)]
    [Encrypted]
    public string? Country { get; set; }

    public Guid Id { get; set; }
}
