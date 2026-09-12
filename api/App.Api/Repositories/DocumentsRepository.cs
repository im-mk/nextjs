using System.Data;
using App.Api.Entities;
using Dapper;

namespace App.Api.Repositories;

public class DocumentsRepository(
    IDbConnection dbConnection) : IDocumentsRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;

    public async Task<Document?> GetById(int id)
    {
        return await _dbConnection.QueryFirstOrDefaultAsync<Document>(
            @"SELECT id, contact_id as ContactId, file_name as FileName, content_type as ContentType,
                     size_bytes as SizeBytes, storage_key as StorageKey,
                     created_at as CreatedAt, updated_at as UpdatedAt
              FROM public.documents WHERE id = @id",
            new { id });
    }

    public async Task<IEnumerable<Document>> GetAll(int? contactId = null)
    {
        var sql = @"SELECT id, contact_id as ContactId, file_name as FileName, content_type as ContentType,
                            size_bytes as SizeBytes, storage_key as StorageKey,
                            created_at as CreatedAt, updated_at as UpdatedAt
                     FROM public.documents";

        if (contactId.HasValue)
            sql += " WHERE contact_id = @contactId";

        sql += " ORDER BY created_at DESC";

        return await _dbConnection.QueryAsync<Document>(sql, new { contactId });
    }

    public async Task<int> Create(Document document)
    {
        var id = await _dbConnection.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(id), 0) + 1 FROM public.documents");
        var now = DateTime.UtcNow;

        await _dbConnection.ExecuteAsync(
            @"INSERT INTO public.documents (id, contact_id, file_name, content_type, size_bytes, storage_key, created_at, updated_at)
              VALUES (@id, @contactId, @fileName, @contentType, @sizeBytes, @storageKey, @createdAt, @updatedAt)",
            new
            {
                id,
                contactId = document.ContactId,
                fileName = document.FileName,
                contentType = document.ContentType,
                sizeBytes = document.SizeBytes,
                storageKey = document.StorageKey,
                createdAt = now,
                updatedAt = now
            });

        return id;
    }

    public async Task<bool> Delete(int id)
    {
        var result = await _dbConnection.ExecuteAsync(
            "DELETE FROM public.documents WHERE id = @id",
            new { id });

        return result > 0;
    }
}
