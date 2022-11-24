namespace VirtualFinland.UserAPI.Models.Shared;

public record Address(
    string? StreetAddress,
    string? ZipCode,
    string? City,
    string? Country
);
