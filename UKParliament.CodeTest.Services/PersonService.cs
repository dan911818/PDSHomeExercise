namespace UKParliament.CodeTest.Services;

using Microsoft.Extensions.Logging;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Repositories;
using UKParliament.CodeTest.Services.DTOs;
using UKParliament.CodeTest.Services.Validators;
using Microsoft.EntityFrameworkCore;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonValidator _personValidator;
    private readonly ILogger<PersonService> _logger;
    private readonly PersonManagerContext _context;
    
    public PersonService(IPersonRepository personRepository, ILogger<PersonService> logger, IPersonValidator personValidator, PersonManagerContext context)
    {
        _personRepository = personRepository;
        _logger = logger;
        _personValidator = personValidator ?? throw new ArgumentNullException(nameof(personValidator));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PersonDTO> AddPersonAsync(PersonDTO personDto)
    {
        await _personValidator.ValidateForCreateAsync(personDto); // Exception will be thrown if validation fails

        // Get or create the department
        Department? department = null;
        if (personDto.Department != null && !string.IsNullOrEmpty(personDto.Department.Name))
        {
            department = await GetOrCreateDepartmentAsync(personDto.Department.Name);
        }

        Person personToCreate = new()
        {
            FirstName = personDto.FirstName,
            LastName = personDto.LastName,
            DateOfBirth = personDto.DateOfBirth,
            Department = department!
        };

        var createdPerson = await _personRepository.CreateAsync(personToCreate);
        
        return new PersonDTO
        {
            Id = createdPerson.Id,
            FirstName = createdPerson.FirstName,
            LastName = createdPerson.LastName,
            DateOfBirth = createdPerson.DateOfBirth,
            Department = new DepartmentDTO
            {
                Name = createdPerson.Department?.Name ?? "No Department"
            }
        };
    }

    private async Task<Department> GetOrCreateDepartmentAsync(string departmentName)
    {
        // First try to find existing department
        var existingDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == departmentName);
        
        if (existingDepartment != null)
        {
            return existingDepartment;
        }

        // Create new department if it doesn't exist
        var newDepartment = new Department { Name = departmentName };
        _context.Departments.Add(newDepartment);
        await _context.SaveChangesAsync();
        return newDepartment;
    }

    public async Task<bool> DeletePersonAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Valid Person ID is required for deletion.");
        }

        var person = await _personRepository.GetByIdAsync(id);
        if (person == null)
        {
            _logger.LogWarning($"Person with ID {id} not found for deletion.");
            return false;
        }
        return await _personRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PersonDTO>> GetAllPeopleAsync()
    {
        var people = await _personRepository.GetAllAsync();
        if (people == null || !people.Any())
        {
            _logger.LogInformation("No people found in the database.");
            return Enumerable.Empty<PersonDTO>();
        }

        return people.Select(p => new PersonDTO
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Department = new DepartmentDTO { Name = p.Department?.Name ?? "No Department" }
        });
    }

    public Task<IEnumerable<PersonDTO>> GetPeopleByDepartmentAsync(int departmentId)
    {
        throw new NotImplementedException();
    }

    public async Task<PersonDTO?> GetByIdAsync(int id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        if (person == null)
        {
            _logger.LogWarning($"Person with ID {id} not found.");
            return null;
        }
        return new PersonDTO
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            Department = new DepartmentDTO
            {
                Name = person?.Department?.Name ?? "No Department"
            }
        };
    }

    public Task<PersonDTO> GetByNameAsync(string firstName, string lastName)
    {
        throw new NotImplementedException();
    }

    public async Task<PersonDTO> UpdatePersonAsync(int id, PersonDTO personDto)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Valid Person ID is required for update.");
        }

        if (personDto == null)
        {
            throw new ArgumentNullException(nameof(personDto), "Person data is required for update.");
        }

        await _personValidator.ValidateForUpdateAsync(personDto); // Exception will be thrown if validation fails

        var existingPerson = await _personRepository.GetByIdAsync(id);
        if (existingPerson == null)
        {
            _logger.LogWarning($"Person with ID {id} not found for update.");
            throw new ArgumentException($"Person with ID {id} not found.");
        }

        // Update the person properties
        existingPerson.FirstName = personDto.FirstName;
        existingPerson.LastName = personDto.LastName;
        existingPerson.DateOfBirth = personDto.DateOfBirth;
        
        // Handle department update
        if (personDto.Department != null && !string.IsNullOrEmpty(personDto.Department.Name))
        {
            existingPerson.Department = await GetOrCreateDepartmentAsync(personDto.Department.Name);
        }
        else
        {
            existingPerson.Department = null!;
        }

        var updatedPerson = await _personRepository.UpdateAsync(existingPerson);
        
        return new PersonDTO
        {
            Id = updatedPerson.Id,
            FirstName = updatedPerson.FirstName,
            LastName = updatedPerson.LastName,
            DateOfBirth = updatedPerson.DateOfBirth,
            Department = new DepartmentDTO
            {
                Name = updatedPerson.Department?.Name ?? "No Department"
            }
        };
    }
}