using System.Data.Common;
using Application.Abstractions;
using Domain.Quotes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedCore;

namespace Application.Features.Quotes;

public sealed record DeleteQuoteCommand(int id):IRequest<Result>;

public sealed class DeleteQuoteHandler(IAppDbContext context)
    : IRequestHandler<DeleteQuoteCommand, Result>
{
    public async Task<Result> Handle(DeleteQuoteCommand command, CancellationToken cancellationToken)
    {
        // Quote? quote = await context.Quotes.SingleOrDefaultAsync(t => t.Id == request.id); // better for multi-key lookups
        Quote? quote = await context.Quotes.FindAsync(command.id);
        if (quote is null)
        {
            return Result.Failure<Result>(new Error("NOT_FOUND",
                $"Quote with id '{command.id}' not found.", ErrorType.NotFound));
        }
        return Result.Success();
    }
}

public sealed class DeleteQuoteValidator : AbstractValidator<DeleteQuoteCommand>
{
    public DeleteQuoteValidator()
    {
        RuleFor(i => i.id)
            .GreaterThan(0)
            .WithMessage("The quote id must be a positive integer");
    }
}