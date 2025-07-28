

using UKParliament.CodeTest.Services.DTOs;
using UKParliament.CodeTest.Services.Validators;
using UKParliament.CodeTest.Data.Repositories;

namespace UKParliament.CodeTest.Services;

public class PersonValidator : IPersonValidator
{
    private readonly IPersonRepository _personRepository;
    public PersonValidator(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task ValidateForCreateAsync(PersonDTO personDTO)
    {
        // Check for null DTO
        if (personDTO == null)
        {
            throw new ArgumentNullException(nameof(personDTO), "Person data is required.");
        }

        // Validate common fields
        ValidateCommonFields(personDTO);

        // Check for duplicate person (same first name, last name, and date of birth)
        var existingPerson = await _personRepository.GetByNameAsync(personDTO.FirstName, personDTO.LastName);
        if (existingPerson != null && existingPerson.DateOfBirth == personDTO.DateOfBirth)
        {
            throw new ArgumentException($"A person with the name '{personDTO.FirstName} {personDTO.LastName}' and date of birth '{personDTO.DateOfBirth}' already exists.");
        }
    }

    public async Task ValidateForUpdateAsync(PersonDTO personDTO)
    {
        // Check for null DTO
        if (personDTO == null)
        {
            throw new ArgumentNullException(nameof(personDTO), "Person data is required.");
        }

        // Validate ID for update
        if (personDTO.Id <= 0)
        {
            throw new ArgumentException("Valid Person ID is required for update.");
        }

        // Check if person exists
        var existingPerson = await _personRepository.GetByIdAsync(personDTO.Id);
        if (existingPerson == null)
        {
            throw new ArgumentException($"Person with ID {personDTO.Id} does not exist.");
        }

        // Validate common fields
        ValidateCommonFields(personDTO);

        // Check for duplicate person (excluding the current person being updated)
        var duplicatePerson = await _personRepository.GetByNameAsync(personDTO.FirstName, personDTO.LastName);
        if (duplicatePerson != null && duplicatePerson.Id != personDTO.Id && duplicatePerson.DateOfBirth == personDTO.DateOfBirth)
        {
            throw new ArgumentException($"Another person with the name '{personDTO.FirstName} {personDTO.LastName}' and date of birth '{personDTO.DateOfBirth}' already exists.");
        }
    }

    private static void ValidateCommonFields(PersonDTO personDTO)
    {
        // Validate First Name
        if (string.IsNullOrWhiteSpace(personDTO.FirstName))
        {
            throw new ArgumentException("First name is required and cannot be empty.");
        }

        if (personDTO.FirstName.Length < 2)
        {
            throw new ArgumentException("First name must be at least 2 characters long.");
        }

        if (personDTO.FirstName.Length > 50)
        {
            throw new ArgumentException("First name cannot exceed 50 characters.");
        }

        if (!IsValidName(personDTO.FirstName))
        {
            throw new ArgumentException("First name contains invalid characters. Only letters, spaces, hyphens, and apostrophes are allowed.");
        }

        // Validate Last Name
        if (string.IsNullOrWhiteSpace(personDTO.LastName))
        {
            throw new ArgumentException("Last name is required and cannot be empty.");
        }

        if (personDTO.LastName.Length < 2)
        {
            throw new ArgumentException("Last name must be at least 2 characters long.");
        }

        if (personDTO.LastName.Length > 50)
        {
            throw new ArgumentException("Last name cannot exceed 50 characters.");
        }

        if (!IsValidName(personDTO.LastName))
        {
            throw new ArgumentException("Last name contains invalid characters. Only letters, spaces, hyphens, and apostrophes are allowed.");
        }

        // Validate Date of Birth
        var today = DateOnly.FromDateTime(DateTime.Now);
        var minimumAge = today.AddYears(-120); // Assuming no one lives beyond 120 years
        var maximumAge = today.AddYears(-16);  // Minimum working age

        if (personDTO.DateOfBirth > today)
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }

        if (personDTO.DateOfBirth < minimumAge)
        {
            throw new ArgumentException("Date of birth is too far in the past. Please verify the date.");
        }

        if (personDTO.DateOfBirth > maximumAge)
        {
            throw new ArgumentException("Person must be at least 16 years old.");
        }

        // Validate Department
        if (personDTO.Department == null)
        {
            throw new ArgumentException("Department is required.");
        }

        if (string.IsNullOrWhiteSpace(personDTO.Department.Name))
        {
            throw new ArgumentException("Department name is required and cannot be empty.");
        }

        // Validate department against allowed values
        var allowedDepartments = new[] { "IT", "HR", "Finance", "Operations", "Legal" };
        if (!allowedDepartments.Contains(personDTO.Department.Name, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid department '{personDTO.Department.Name}'. Allowed departments are: {string.Join(", ", allowedDepartments)}.");
        }
    }

    private static bool IsValidName(string name)
    {
        // Allow letters, spaces, hyphens, and apostrophes
        // This regex allows for names like "Mary-Jane", "O'Connor", "Van Der Berg", etc.
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z\s'-]+$");
    }
}
