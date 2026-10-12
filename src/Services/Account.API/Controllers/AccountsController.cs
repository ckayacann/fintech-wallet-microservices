using System.Security.Claims;
using Account.API.Features.Accounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] string currency)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var account = await _mediator.Send(new CreateAccountCommand(userId, currency));
        return Ok(account);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var accounts = await _mediator.Send(new GetAccountsByUserIdQuery(userId));
        return Ok(accounts);
    }
}