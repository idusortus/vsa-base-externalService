using Application.Abstractions;
using Domain.Quotes;
using FluentValidation;
using MediatR;
using SharedCore;

namespace Application.Features.Quotes;

public sealed record CreateQuoteCommand(string Author, string Content):IRequest<Result<int>>;

internal sealed class CreateQuoteHandler(IAppDbContext context)
    : IRequestHandler<CreateQuoteCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = new Quote { Author = request.Author, Content = request.Content };
        context.Quotes.Add(quote);
        await context.SaveChangesAsync(cancellationToken);

        return quote.Id;
    }
}

public sealed class CreateQuoteValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteValidator()
    {
        RuleFor(a => a.Author)
            .MinimumLength(5)
            .WithMessage("Author must contain at least 5 characters.");
        RuleFor(c => c.Content)
            .MinimumLength(5)
            .WithMessage("Conetent must contain at least 5 characters.");
    }
}