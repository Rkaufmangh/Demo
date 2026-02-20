using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Dapper;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using static StargateAPI.Business.Commands.GetPersonHandler;

namespace StargateAPI.Business.Commands
{
    public class GetPerson : IRequest<GetPersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonHandler : IRequestHandler<GetPerson, GetPersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetPersonHandler> _logger;

        public GetPersonHandler(StargateContext context, ILogger<GetPersonHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetPersonResult> Handle(GetPerson request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting person with name: {Name}", request.Name);
            // Use AsNoTracking for read-only operations
            var person = await _context.People
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null)
            {
                _logger.LogError("Person with name: {name} was null", request.Name);

                throw new BadHttpRequestException("Person not found");
            }
            _logger.LogInformation("Person: {@person}", person);
            return new GetPersonResult
            {
                Person = person
            };
        }

        public class GetPersonResult : BaseResponse
        {
            public Person? Person { get; set; } = null;
        }
    }
}