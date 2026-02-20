using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        public CreatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public async Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            var person = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name);

            if (person is not null) throw new BadHttpRequestException("Bad Request");
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger _logger;

        public CreatePersonHandler(StargateContext context, ILogger<CreatePersonHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            //check to make sure name is not in db
            var existingPerson =
                await _context.Connection.QueryFirstOrDefaultAsync<Person>("select * from person where name = @name",
                    new {name = request.Name});
            if (existingPerson != null)
                return new CreatePersonResult
                {
                    Id = existingPerson.Id,
                    Message = "This person alread exists.",
                    ResponseCode = -1,
                    Success = false
                };
            try
            {
                var newPerson = new Person()
                {
                    Name = request.Name
                };

                await _context.People.AddAsync(newPerson);

                await _context.SaveChangesAsync();

                return new CreatePersonResult()
                {
                    Id = newPerson.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create person");
                return new CreatePersonResult {Success = false, Message = "Failed to create person"};
            }

        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
