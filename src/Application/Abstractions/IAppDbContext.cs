using Domain.Quotes;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Quote> Quotes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}