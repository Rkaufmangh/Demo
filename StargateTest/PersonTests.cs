using StargateAPI.Business.Queries;

namespace StargateTest
{
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using StargateAPI.Business.Commands;
    using StargateAPI.Business.Data;

    [TestFixture]
    public class PersonTests
    {
        private SqliteConnection _connection;
        private StargateContext _context;
        private string _name = "Jack Beanstock";

        [SetUp]
        public void Setup()
        {
            // 1. Create and open connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // 2. Configure Context
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new StargateContext(options);
            _context.Database.EnsureCreated(); // Creates schema
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
            _connection.Dispose(); // Memory DB is wiped here
        }

        [Test]
        public async Task CreatePerson_Test()
        {
            // Arrange
            var handler = new CreatePersonHandler(_context, new Mock<ILogger<CreatePersonHandler>>().Object);
            var command = new CreatePerson { Name = _name };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == result.Id);
            Assert.That(person, Is.Not.Null);
            Assert.That(person.Name, Is.EqualTo(_name));
        }

        [Test]
        public async Task GetPersonByName_Test()
        {
            var handler = new GetPersonHandler(_context, new Mock<ILogger<GetPersonHandler>>().Object);
            var createHandler = new CreatePersonHandler(_context, new Mock<ILogger<CreatePersonHandler>>().Object);
            //add person
            var command = new CreatePerson {Name = _name};

            _ = await createHandler.Handle(command, CancellationToken.None);
            var person = _context.People.FirstOrDefault(p => p.Name == _name);
            Assert.That(person, Is.Not.Null);
            Assert.That(person.Name, Is.EqualTo(_name));
        }

        [Test]
        public async Task GetAllPeople_Test()
        {
            var handler = new GetPeopleHandler(_context, new Mock<ILogger<GetPersonHandler>>().Object);
            var createHandler = new CreatePersonHandler(_context, new Mock<ILogger<CreatePersonHandler>>().Object);
            for (int i = 0; i < 5; i++)
            {
                
            }
        }
    }

}
