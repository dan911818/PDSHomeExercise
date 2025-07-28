using UKParliament.CodeTest.Services.DTOs;

namespace UKParliament.CodeTest.Web.ViewModels;

public class PersonViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DepartmentDTO Department { get; set; }
}