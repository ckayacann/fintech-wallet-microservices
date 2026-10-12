using System.Text.Json;
using Account.API.Data;
using Account.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Account.API.Features.Accounts;

public record GetAccountsByUserIdQuery(int UserId) : IRequest<List<AccountModel>>;

public class GetAccountsByUserIdQueryHandler : IRequestHandler<GetAccountsByUserIdQuery, List<AccountModel>>
{
    private readonly AccountDbContext _context;
    private readonly IDistributedCache _cache;

    public GetAccountsByUserIdQueryHandler(AccountDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<AccountModel>> Handle(GetAccountsByUserIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"user_accounts_{request.UserId}";

        // 1. Önce Redis Cache kontrol edilir
        var cachedAccounts = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedAccounts))
        {
            return JsonSerializer.Deserialize<List<AccountModel>>(cachedAccounts)!;
        }

        var accounts = await _context.Accounts
            .Where(a => a.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(accounts),
            cacheOptions,
            cancellationToken
        );

        return accounts;
    }
}