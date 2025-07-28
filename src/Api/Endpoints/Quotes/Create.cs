using Api.Extensions;
using Application.Features.Quotes;
using MediatR;
using SharedCore;

namespace Api.Endpoints.Quotes;

internal sealed class CreateQuoteEndpoint : IEndpoint
{
    public sealed class Request
    {
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("quotes", async (ISender sender, Request request, CancellationToken ct) =>
        {
            var command = new CreateQuoteCommand(request.Author, request.Content);
            Result<int> result = await sender.Send(command, ct);
            return result.Match(
                value => Results.CreatedAtRoute("GetQuoteById", new {id=value}),
                CustomResults.Problem);
        })
        .WithTags(Tags.Quotes);
    }
}
