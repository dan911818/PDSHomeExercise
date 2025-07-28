namespace UKParliament.CodeTest.Services;

using UKParliament.CodeTest.Services.DTOs;

public interface IPersonService
{
    Task<PersonDTO?> GetByIdAsync(int id);
    Task<PersonDTO> GetByNameAsync(string firstName, string lastName);
    Task<IEnumerable<PersonDTO>> GetAllPeopleAsync();
    Task<IEnumerable<PersonDTO>> GetPeopleByDepartmentAsync(int departmentId);
    Task<PersonDTO> AddPersonAsync(PersonDTO personDto);
    Task<PersonDTO> UpdatePersonAsync(int id, PersonDTO personDto);
    Task<bool> DeletePersonAsync(int id);
}