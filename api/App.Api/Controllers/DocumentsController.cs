using Microsoft.AspNetCore.Mvc;
using App.Api.Dto.Documents;
using App.Api.Services;

namespace App.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(
        IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    [EndpointName("GetAllDocuments")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAll([FromQuery] int? contactId)
    {
        try
        {
            var documents = await _documentService.GetAllDocuments(contactId);
            return Ok(documents);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving documents." });
        }
    }

    [HttpGet("{id}")]
    [EndpointName("GetDocumentById")]
    public async Task<ActionResult<DocumentResponse>> Get(int id)
    {
        try
        {
            var document = await _documentService.GetDocument(id);
            return document != null ? Ok(document) : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the document." });
        }
    }

    [HttpPost("upload-url")]
    [EndpointName("CreateDocumentUploadUrl")]
    public async Task<ActionResult<CreateUploadUrlResponse>> CreateUploadUrl([FromBody] CreateUploadUrlRequest request)
    {
        try
        {
            var result = await _documentService.CreateUploadUrl(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating the upload URL." });
        }
    }

    [HttpPost]
    [EndpointName("CompleteDocumentUpload")]
    public async Task<ActionResult<DocumentResponse>> Create([FromBody] CompleteUploadRequest request)
    {
        try
        {
            var result = await _documentService.CompleteUpload(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while saving the document." });
        }
    }

    [HttpGet("{id}/download-url")]
    [EndpointName("GetDocumentDownloadUrl")]
    public async Task<ActionResult<DownloadUrlResponse>> GetDownloadUrl(int id)
    {
        try
        {
            var result = await _documentService.GetDownloadUrl(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating the download URL." });
        }
    }

    [HttpDelete("{id}")]
    [EndpointName("DeleteDocument")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var ok = await _documentService.DeleteDocument(id);
            return ok ? Ok(new { message = "Document deleted successfully" }) : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the document." });
        }
    }
}
