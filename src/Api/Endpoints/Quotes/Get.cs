using Api.Extensions;
using Application.Features.Quotes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Quotes;

public class GetQuotesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("quotes", async (
            ISender sender,
            CancellationToken ct,
            [FromQuery] int pNumber = 1,
            [FromQuery] int pSize = 10) =>
        {
            var result = await sender.Send(new GetQuotesQuery(pNumber, pSize));
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Quotes); 
    }
}