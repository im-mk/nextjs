using App.Api.Entities;

namespace App.Api.Repositories;

public interface IDocumentsRepository
{
    Task<Document?> GetById(int id);
    Task<IEnumerable<Document>> GetAll(int? contactId = null);
    Task<int> Create(Document document);
    Task<bool> Delete(int id);
}
