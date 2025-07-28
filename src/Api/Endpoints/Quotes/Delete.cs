using Api.Extensions;
using Application.Features.Quotes;
using MediatR;

namespace Api.Endpoints.Quotes;

internal sealed class DeleteQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("quotes/{id:int}", async (ISender handler, int id, CancellationToken ct) =>
        {
            var result = await handler.Send(new DeleteQuoteCommand(id), ct);
            return result.Match(
                Results.NoContent,
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Quotes);
    }
}
