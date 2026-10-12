using Microsoft.EntityFrameworkCore;
using Transfer.API.Models;

namespace Transfer.API.Data;

public class TransferDbContext : DbContext
{
    public TransferDbContext(DbContextOptions<TransferDbContext> options) : base(options)
    {
    }

    public DbSet<TransferLog> TransferLogs { get; set; }
}