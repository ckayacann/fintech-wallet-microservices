namespace Transfer.API.Events;

public record MoneyTransferSubmittedEvent(
    Guid TransactionId,
    string FromAccount,
    string ToAccount,
    decimal Amount,
    DateTime Timestamp
);