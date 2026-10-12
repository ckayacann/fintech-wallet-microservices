using Account.API.Data;
using Account.API.Models;
using MediatR;

namespace Account.API.Features.Accounts;

public record CreateAccountCommand(int UserId, string Currency) : IRequest<AccountModel>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountModel>
{
    private readonly AccountDbContext _context;

    public CreateAccountCommandHandler(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<AccountModel> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountNumber = "TR" + new Random().Next(100000000, 999999999).ToString();

        var account = new AccountModel
        {
            UserId = request.UserId,
            AccountNumber = accountNumber,
            Balance = 1000, // Deneme için başlangıç bakiyesi
            Currency = request.Currency
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return account;
    }
}