using Application.Abstractions;
using Application.Extensions;
using Domain.Quotes;
using FluentValidation;
using MediatR;
using SharedCore;

namespace Application.Features.Quotes;

public record GetQuotesQuery(int PageNumber, int PageSize) : IRequest<Result<PaginatedResult<Quote>>>;

public sealed class GetQuotesValidator : AbstractValidator<GetQuotesQuery>
{
    public GetQuotesValidator()
    {
        RuleFor(n => n.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page Number must be a positive integer");
        RuleFor(s => s.PageSize)
            .GreaterThan(0)
            .WithMessage("Page Size must be a positive integer.");
    }
}

public sealed class Handler(IAppDbContext context) : IRequestHandler<GetQuotesQuery, Result<PaginatedResult<Quote>>>
{
    public async Task<Result<PaginatedResult<Quote>>> Handle(GetQuotesQuery request, CancellationToken ct)
    {
        var pagination = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var query = context.Quotes.OrderBy(q => q.Id);
        var result = await query.ToPaginatedResultAsync(pagination, ct);
        return Result.Success(result);
    }
}
