namespace UKParliament.CodeTest.Data;

public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Department Department { get; set; }
}