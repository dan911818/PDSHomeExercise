using Microsoft.AspNetCore.Mvc;
using UKParliament.CodeTest.Web.ViewModels;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace UKParliament.CodeTest.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonController> _logger;
    
    public PersonController(IPersonService personService, ILogger<PersonController> logger)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/person/all
    [Route("all")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonDTO>>> GetAllPeople()
    {
        try
        {
            var people = await _personService.GetAllPeopleAsync();
            if (people == null || !people.Any())
            {
                _logger.LogInformation("No people found.");
                return Ok(Enumerable.Empty<PersonDTO>());
            }
            return Ok(people);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all people.");
            return StatusCode(500, "An error occurred while retrieving people.");
        }
    }

    // GET: api/person/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonDTO>> GetById(int id)
    {
        try
        {
            var person = await _personService.GetByIdAsync(id);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {Id} not found.", id);
                return NotFound($"Person with ID {id} not found.");
            }
            return Ok(person);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving person with ID {Id}.", id);
            return StatusCode(500, "An error occurred while retrieving the person.");
        }
    }

    // POST: api/person
    [HttpPost]
    public async Task<ActionResult<PersonDTO>> CreatePerson([FromBody] PersonDTO personDto)
    {
        if (personDto == null)
        {
            _logger.LogError("Received null person DTO for creation.");
            return BadRequest(new { message = "Person data is required." });
        }

        try
        {
            var createdPerson = await _personService.AddPersonAsync(personDto);
            return CreatedAtAction(nameof(GetById), new { id = createdPerson.Id }, createdPerson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed for person creation.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person.");
            return StatusCode(500, new { message = "An error occurred while creating the person." });
        }
    }

    // PUT: api/person/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PersonDTO>> UpdatePerson(int id, [FromBody] PersonDTO personDto)
    {
        if (personDto == null)
        {
            _logger.LogError("Received null person DTO for update.");
            return BadRequest(new { message = "Person data is required." });
        }

        if (id != personDto.Id)
        {
            _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match DTO ID {DtoId}.", id, personDto.Id);
            return BadRequest(new { message = "ID in URL does not match ID in request body." });
        }

        try
        {
            var updatedPerson = await _personService.UpdatePersonAsync(id, personDto);
            return Ok(updatedPerson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed for person update with ID {Id}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person with ID {Id}.", id);
            return StatusCode(500, new { message = "An error occurred while updating the person." });
        }
    }

    // DELETE: api/person/{id}
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePerson(int id)
    {
        try
        {
            var result = await _personService.DeletePersonAsync(id);
            if (!result)
            {
                _logger.LogWarning("Person with ID {Id} not found for deletion.", id);
                return NotFound($"Person with ID {id} not found.");
            }
            
            _logger.LogInformation("Successfully deleted person with ID {Id}.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting person with ID {Id}.", id);
            return StatusCode(500, "An error occurred while deleting the person.");
        }
    }
}