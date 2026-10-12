using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedLockNet;
using Transfer.API.Data;
using Transfer.API.Events;
using Transfer.API.Models;

namespace Transfer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TransferDbContext _context;
    private readonly IDistributedLockFactory _lockFactory;

    public TransfersController(
        IPublishEndpoint publishEndpoint, 
        TransferDbContext context, 
        IDistributedLockFactory lockFactory)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _lockFactory = lockFactory;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTransfer([FromBody] CreateTransferDto dto)
    {
        string resource = $"lock:account:{dto.FromAccount}";
        TimeSpan expiryTime = TimeSpan.FromSeconds(10);
        TimeSpan waitTime = TimeSpan.FromSeconds(2);
        TimeSpan retryTime = TimeSpan.FromMilliseconds(200);

        await using (var redLock = await _lockFactory.CreateLockAsync(resource, expiryTime, waitTime, retryTime))
        {
            if (!redLock.IsAcquired)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new 
                { 
                    message = "Hesabınızda şu anda işlenen bir transfer bulunuyor. Lütfen birkaç saniye sonra tekrar deneyin." 
                });
            }

            var transactionId = Guid.NewGuid();

            var transferLog = new TransferLog
            {
                Id = transactionId,
                FromAccount = dto.FromAccount,
                ToAccount = dto.ToAccount,
                Amount = dto.Amount,
                Status = "Pending"
            };

            _context.TransferLogs.Add(transferLog);
            await _context.SaveChangesAsync();

            // RabbitMQ event fırlatma
            await _publishEndpoint.Publish(new MoneyTransferSubmittedEvent(
                transactionId,
                dto.FromAccount,
                dto.ToAccount,
                dto.Amount,
                DateTime.UtcNow
            ));

            return Accepted(new { transactionId, status = "Processing" });
        } 
    }

    public record CreateTransferDto(string FromAccount, string ToAccount, decimal Amount);
}