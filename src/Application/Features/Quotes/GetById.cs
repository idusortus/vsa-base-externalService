using Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedCore;

namespace Application.Features.Quotes;

public sealed record GetQuoteByIdQuery(int QuoteId) : IRequest<Result<QuoteResponse>>;

internal sealed class GetQuoteByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetQuoteByIdQuery, Result<QuoteResponse>>
{
    public async Task<Result<QuoteResponse>> Handle(GetQuoteByIdQuery query, CancellationToken cancellationToken)
    {
        QuoteResponse? quote = await context.Quotes
            .Where(quoteItem => quoteItem.Id == query.QuoteId)
            .Select(quoteItem => new QuoteResponse
            {
                QuoteId = quoteItem.Id,
                Author = quoteItem.Author,
                Content = quoteItem.Content
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (quote is null)
        {
            return Result.Failure<QuoteResponse>(new Error("NOT_Found",
                $"Quote with id '{query.QuoteId}' not found.", ErrorType.NotFound));
        }
        return quote;
    }
}

public sealed class QuoteResponse
{
    public int QuoteId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public sealed class GetQuoteByIdValidator : AbstractValidator<GetQuoteByIdQuery>
{
    public GetQuoteByIdValidator()
    {
        RuleFor(i => i.QuoteId)
            .GreaterThan(0)
            .WithMessage("Quote id must be greater than zero.");
    }
}