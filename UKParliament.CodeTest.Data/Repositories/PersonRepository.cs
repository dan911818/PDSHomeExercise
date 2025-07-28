using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKParliament.CodeTest.Data.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly PersonManagerContext _context;
        private readonly ILogger<PersonRepository> _logger;
        public PersonRepository(PersonManagerContext context, ILogger<PersonRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public Task<Person?> GetByNameAsync(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("First name and last name cannot be null or empty.");
            }
            return _context.People
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.FirstName == firstName && p.LastName == lastName);
        }

        public async Task<Person> CreateAsync(Person person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return false;
            }
            _context.People.Remove(person);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.People.Include(p => p.Department).ToListAsync();
        }

        public async Task<Person?> GetByIdAsync(int id)
        {
            return await _context.People.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Person> UpdateAsync(Person person)
        {
            var personToUpdate = _context.People.Find(person.Id);
            if (personToUpdate == null)
            {
                throw new ArgumentException($"Person with ID {person.Id} does not exist.");
            }
            personToUpdate.FirstName = person.FirstName;
            personToUpdate.LastName = person.LastName;
            personToUpdate.DateOfBirth = person.DateOfBirth;
            personToUpdate.Department = person.Department;
            _context.People.Update(personToUpdate);
            await _context.SaveChangesAsync();
            return personToUpdate;
        }
    }
}
