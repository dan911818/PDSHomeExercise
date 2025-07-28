using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Data.Repositories;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.DTOs;
using UKParliament.CodeTest.Services.Validators;

namespace UKParliament.CodeTest.Tests
{
    public class PersonServiceTests : IDisposable
    {
        private readonly Mock<IPersonRepository> _mockRepository;
        private readonly Mock<IPersonValidator> _mockValidator;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly PersonManagerContext _context;
        private readonly PersonService _personService;

        public PersonServiceTests()
        {
            _mockRepository = new Mock<IPersonRepository>();
            _mockValidator = new Mock<IPersonValidator>();
            _mockLogger = new Mock<ILogger<PersonService>>();

            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<PersonManagerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new PersonManagerContext(options);

            // Seed test departments
            _context.Departments.AddRange(
                new Department { Id = 1, Name = "IT" },
                new Department { Id = 2, Name = "HR" },
                new Department { Id = 3, Name = "Finance" }
            );
            _context.SaveChanges();

            _personService = new PersonService(_mockRepository.Object, _mockLogger.Object, _mockValidator.Object, _context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region AddPersonAsync Tests

        [Fact]
        public async Task AddPersonAsync_ValidPerson_ReturnsCreatedPerson()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            var createdPerson = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = personDto.DateOfBirth,
                Department = new Department { Id = 1, Name = "IT" }
            };

            _mockValidator.Setup(v => v.ValidateForCreateAsync(personDto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Person>())).ReturnsAsync(createdPerson);

            // Act
            var result = await _personService.AddPersonAsync(personDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("IT", result.Department.Name);
            _mockValidator.Verify(v => v.ValidateForCreateAsync(personDto), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Person>()), Times.Once);
        }

        [Fact]
        public async Task AddPersonAsync_ValidPersonWithNewDepartment_CreatesNewDepartment()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                Department = new DepartmentDTO { Name = "Operations" } // New department not in seed data
            };

            var createdPerson = new Person
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = personDto.DateOfBirth,
                Department = new Department { Id = 4, Name = "Operations" }
            };

            _mockValidator.Setup(v => v.ValidateForCreateAsync(personDto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Person>())).ReturnsAsync(createdPerson);

            // Act
            var result = await _personService.AddPersonAsync(personDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Operations", result.Department.Name);
            
            // Verify the new department was created in the context
            var newDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Operations");
            Assert.NotNull(newDepartment);
            Assert.Equal("Operations", newDepartment.Name);
        }

        [Fact]
        public async Task AddPersonAsync_ValidationFails_ThrowsException()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                FirstName = "Invalid",
                LastName = "Person",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), // Too young
                Department = new DepartmentDTO { Name = "IT" }
            };

            _mockValidator.Setup(v => v.ValidateForCreateAsync(personDto))
                         .ThrowsAsync(new ArgumentException("Person must be at least 16 years old."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _personService.AddPersonAsync(personDto));
            Assert.Equal("Person must be at least 16 years old.", exception.Message);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Person>()), Times.Never);
        }

        #endregion

        #region GetAllPeopleAsync Tests

        [Fact]
        public async Task GetAllPeopleAsync_PeopleExist_ReturnsAllPeople()
        {
            // Arrange
            var people = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)), Department = new Department { Name = "IT" } },
                new Person { Id = 2, FirstName = "Jane", LastName = "Smith", DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)), Department = new Department { Name = "HR" } }
            };

            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(people);

            // Act
            var result = await _personService.GetAllPeopleAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.FirstName == "John" && p.Department.Name == "IT");
            Assert.Contains(result, p => p.FirstName == "Jane" && p.Department.Name == "HR");
        }

        [Fact]
        public async Task GetAllPeopleAsync_NoPeople_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Person>());

            // Act
            var result = await _personService.GetAllPeopleAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPeopleAsync_RepositoryReturnsNull_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((IEnumerable<Person>)null!);

            // Act
            var result = await _personService.GetAllPeopleAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_PersonExists_ReturnsPerson()
        {
            // Arrange
            var person = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new Department { Name = "IT" }
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);

            // Act
            var result = await _personService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("IT", result.Department.Name);
        }

        [Fact]
        public async Task GetByIdAsync_PersonDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Person?)null);

            // Act
            var result = await _personService.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdatePersonAsync Tests

        [Fact]
        public async Task UpdatePersonAsync_ValidUpdate_ReturnsUpdatedPerson()
        {
            // Arrange
            var existingPerson = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new Department { Name = "IT" }
            };

            var updatedPersonDto = new PersonDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith", // Changed last name
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "HR" } // Changed department
            };

            var updatedPerson = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new Department { Name = "HR" }
            };

            _mockValidator.Setup(v => v.ValidateForUpdateAsync(updatedPersonDto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPerson);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Person>())).ReturnsAsync(updatedPerson);

            // Act
            var result = await _personService.UpdatePersonAsync(1, updatedPersonDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal("HR", result.Department.Name);
            _mockValidator.Verify(v => v.ValidateForUpdateAsync(updatedPersonDto), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePersonAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _personService.UpdatePersonAsync(0, personDto));
            Assert.Equal("Valid Person ID is required for update.", exception.Message);
        }

        [Fact]
        public async Task UpdatePersonAsync_NullPersonDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _personService.UpdatePersonAsync(1, null!));
        }

        [Fact]
        public async Task UpdatePersonAsync_PersonNotFound_ThrowsArgumentException()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                Id = 999,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            _mockValidator.Setup(v => v.ValidateForUpdateAsync(personDto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Person?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _personService.UpdatePersonAsync(999, personDto));
            Assert.Equal("Person with ID 999 not found.", exception.Message);
        }

        #endregion

        #region DeletePersonAsync Tests

        [Fact]
        public async Task DeletePersonAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            var person = new Person { Id = 1, FirstName = "John", LastName = "Doe" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _personService.DeletePersonAsync(1);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeletePersonAsync_PersonNotFound_ReturnsFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Person?)null);

            // Act
            var result = await _personService.DeletePersonAsync(999);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeletePersonAsync_InvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _personService.DeletePersonAsync(0));
            Assert.Equal("Valid Person ID is required for deletion.", exception.Message);

            var exception2 = await Assert.ThrowsAsync<ArgumentException>(
                () => _personService.DeletePersonAsync(-1));
            Assert.Equal("Valid Person ID is required for deletion.", exception2.Message);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_NullValidator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PersonService(_mockRepository.Object, _mockLogger.Object, null!, _context));
        }

        [Fact]
        public void Constructor_NullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new PersonService(_mockRepository.Object, _mockLogger.Object, _mockValidator.Object, null!));
        }

        #endregion

        #region Integration-style Tests for Department Handling

        [Fact]
        public async Task AddPersonAsync_WithExistingDepartment_ReusesExistingDepartment()
        {
            // Arrange
            var personDto = new PersonDTO
            {
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" } // Existing department
            };

            var createdPerson = new Person
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = personDto.DateOfBirth,
                Department = new Department { Id = 1, Name = "IT" }
            };

            _mockValidator.Setup(v => v.ValidateForCreateAsync(personDto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Person>())).ReturnsAsync(createdPerson);

            var departmentCountBefore = await _context.Departments.CountAsync();

            // Act
            await _personService.AddPersonAsync(personDto);

            // Assert
            var departmentCountAfter = await _context.Departments.CountAsync();
            Assert.Equal(departmentCountBefore, departmentCountAfter); // Should not create new department
        }

        #endregion
    }
}
