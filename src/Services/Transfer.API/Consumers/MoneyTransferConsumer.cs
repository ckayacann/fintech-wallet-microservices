using MassTransit;
using Microsoft.EntityFrameworkCore;
using Transfer.API.Data;
using Transfer.API.Events;

namespace Transfer.API.Consumers;

public class MoneyTransferConsumer : IConsumer<MoneyTransferSubmittedEvent>
{
    private readonly TransferDbContext _context;
    private readonly ILogger<MoneyTransferConsumer> _logger;

    public MoneyTransferConsumer(TransferDbContext context, ILogger<MoneyTransferConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MoneyTransferSubmittedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing transaction {TransactionId} for amount {Amount}", message.TransactionId, message.Amount);

        var log = await _context.TransferLogs.FirstOrDefaultAsync(t => t.Id == message.TransactionId);
        if (log != null)
        {
            // Gerçek senaryoda bakiye kontrolü & bakiye düşme logic'i burada tetiklenir
            log.Status = "Completed";
            await _context.SaveChangesAsync();
        }
    }
}