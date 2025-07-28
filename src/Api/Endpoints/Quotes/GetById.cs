using Api.Extensions;
using Application.Features.Quotes;
using MediatR;

namespace Api.Endpoints.Quotes;

internal sealed class GetQuoteByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("quotes/{id:int}", async (
            ISender sender,
            int id,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetQuoteByIdQuery(id), ct);
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .WithName("GetQuoteById")
        .WithTags(Tags.Quotes); 
    }
}