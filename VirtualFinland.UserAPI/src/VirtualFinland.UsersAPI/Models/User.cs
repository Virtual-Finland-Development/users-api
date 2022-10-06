using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualFinland.UserAPI.Models;

public class User : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? TestbedId { get; set; }
}