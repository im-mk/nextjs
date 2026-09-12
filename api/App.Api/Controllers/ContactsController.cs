using Microsoft.AspNetCore.Mvc;
using App.Api.Models;
using App.Api.Services;

namespace App.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(
        IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    [EndpointName("GetAllContacts")]
    public async Task<ActionResult<IEnumerable<ContactResponse>>> GetAll()
    {
        try
        {
            var contacts = await _contactService.GetAllContacts();
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving contacts." });
        }
    }

    [HttpGet("{id}")]
    [EndpointName("GetContactById")]
    public async Task<ActionResult<ContactResponse>> Get(int id)
    {
        try
        {
            var contact = await _contactService.GetContact(id);
            return contact != null ? Ok(contact) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the contact." });
        }
    }

    [HttpPost]
    [EndpointName("CreateContact")]
    public async Task<ActionResult<ContactResponse>> Create([FromBody] CreateContactRequest request)
    {
        try
        {
            var result = await _contactService.CreateContact(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the contact." });
        }
    }

    [HttpPatch("{id}")]
    // [EndpointName("UpdateContact")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactRequest request)
    {
        try
        {
            var ok = await _contactService.UpdateContact(id, request);
            return ok ? Ok(new { message = "Contact updated successfully" }) : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the contact." });
        }
    }

    [HttpDelete("{id}")]
    [EndpointName("DeleteContact")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var ok = await _contactService.DeleteContact(id);
            return ok ? Ok(new { message = "Contact deleted successfully" }) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the contact." });
        }
    }

    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmail([FromQuery] string email, [FromQuery] int? excludeContactId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var exists = await _contactService.CheckEmailExists(email, excludeContactId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking email." });
        }
    }
}
