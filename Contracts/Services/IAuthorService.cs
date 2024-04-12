using Astra.Models;

namespace Astra.Contracts.Services;

public interface IAuthorService
{
    Task<Author?> GetAuthorAsync();
    Task SaveAuthorAsync(Author author);
}