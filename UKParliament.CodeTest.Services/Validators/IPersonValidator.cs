using UKParliament.CodeTest.Services.DTOs;

namespace UKParliament.CodeTest.Services.Validators;

public interface IPersonValidator
{
    Task ValidateForCreateAsync(PersonDTO personDTO);
    Task ValidateForUpdateAsync(PersonDTO personDTO);
}
