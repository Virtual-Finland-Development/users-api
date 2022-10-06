using System;

namespace VirtualFinland.UserAPI.Models;

public interface IEntity
{
    Guid Id { get; }
}