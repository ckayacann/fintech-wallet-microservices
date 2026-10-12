using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public TransfersController(IPublishEndpoint publishEndpoint, TransferDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTransfer([FromBody] CreateTransferDto dto)
    {
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

        // RabbitMQ kuyruğuna asenkron event fırlatıyoruz (Non-blocking)
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