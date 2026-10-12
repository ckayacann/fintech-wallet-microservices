using Account.API.Data;
using Account.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Account.API.Features.Accounts;

public record GetAccountsByUserIdQuery(int UserId) : IRequest<List<AccountModel>>;

public class GetAccountsByUserIdQueryHandler : IRequestHandler<GetAccountsByUserIdQuery, List<AccountModel>>
{
    private readonly AccountDbContext _context;

    public GetAccountsByUserIdQueryHandler(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountModel>> Handle(GetAccountsByUserIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .Where(a => a.UserId == request.UserId)
            .ToListAsync(cancellationToken);
    }
}