using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using UKParliament.CodeTest.Data.Repositories;
using UKParliament.CodeTest.Data;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.DTOs;

namespace UKParliament.CodeTest.Tests
{
    public class PersonValidatorTests
    {
        private readonly Mock<IPersonRepository> _mockRepository;
        private readonly PersonValidator _validator;

        public PersonValidatorTests()
        {
            _mockRepository = new Mock<IPersonRepository>();
            _validator = new PersonValidator(_mockRepository.Object);
        }

        [Fact]
        public async Task ValidateForCreateAsync_ValidPerson_DoesNotThrow()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            _mockRepository.Setup(r => r.GetByNameAsync("John", "Doe"))
                          .ReturnsAsync((Person?)null);

            // Act & Assert
            await _validator.ValidateForCreateAsync(personDTO);
        }

        [Fact]
        public async Task ValidateForCreateAsync_NullPerson_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _validator.ValidateForCreateAsync(null!));
        }

        [Fact]
        public async Task ValidateForCreateAsync_EmptyFirstName_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                FirstName = "",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForCreateAsync(personDTO));
            
            Assert.Contains("First name is required", exception.Message);
        }

        [Fact]
        public async Task ValidateForCreateAsync_ShortFirstName_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                FirstName = "A",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForCreateAsync(personDTO));
            
            Assert.Contains("must be at least 2 characters", exception.Message);
        }

        [Fact]
        public async Task ValidateForCreateAsync_FutureDateOfBirth_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForCreateAsync(personDTO));
            
            Assert.Contains("cannot be in the future", exception.Message);
        }

        [Fact]
        public async Task ValidateForCreateAsync_InvalidDepartment_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "InvalidDepartment" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForCreateAsync(personDTO));
            
            Assert.Contains("Invalid department", exception.Message);
        }

        [Fact]
        public async Task ValidateForUpdateAsync_ValidPerson_DoesNotThrow()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            var existingPerson = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25))
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                          .ReturnsAsync(existingPerson);
            _mockRepository.Setup(r => r.GetByNameAsync("John", "Doe"))
                          .ReturnsAsync((Person?)null);

            // Act & Assert
            await _validator.ValidateForUpdateAsync(personDTO);
        }

        [Fact]
        public async Task ValidateForUpdateAsync_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForUpdateAsync(personDTO));
            
            Assert.Contains("Valid Person ID is required", exception.Message);
        }

        [Fact]
        public async Task ValidateForUpdateAsync_PersonNotFound_ThrowsArgumentException()
        {
            // Arrange
            var personDTO = new PersonDTO
            {
                Id = 999,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                Department = new DepartmentDTO { Name = "IT" }
            };

            _mockRepository.Setup(r => r.GetByIdAsync(999))
                          .ReturnsAsync((Person?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _validator.ValidateForUpdateAsync(personDTO));
            
            Assert.Contains("does not exist", exception.Message);
        }
    }
}
