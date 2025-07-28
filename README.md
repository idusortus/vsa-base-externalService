
# Simple Clean/Vertical Slice Template for .Net 9 Webapi
```bash
└───src
    ├───Api
    │   ├───Endpoints
    │   │   └───Quotes
    │   ├───Extensions
    │   ├───Logs
    │   ├───Middleware
    │   └───Properties
 ├───Application
    │   ├───Abstractions
    │   │   ├───Behaviors
    │   │   ├───External
    │   │   └───Messaging
    │   ├───Features
    │   │   └───Quotes
    ├───Domain
    │   └───Quotes
    ├───Infrastructure
    │   ├───Database
    │   ├───External (e.g., HttpClients)
    │   ├───Migrations
    └───SharedCore
```


## Proper syntax for creating a new migration:
> From Root of the solution
```bash
dotnet ef migrations add Init \
  --project src/Infrastructure \
  --startup-project src/Api
```
> Apply DB Update (From solution root)
```bash
dotnet ef database update --project src/Infrastructure/ --startup-project src/Api/
```

### Condensed Feature Command/Query Files
- Delete
  - public sealed record DeleteXXXCommand(id/guid) : IRequest<Result<optional:TResult>>
  - public sealed class DeleteXXXHandler(context) : IRequestHandler<DeleteXXXCommand, Result<optional:TResult>
    - Handle()
      - return Result.Success(), Result.Failure()
  - public class DeleteXXXValidator : AbstractValidator<DeleteXXXCommand>
    - public DeleteXXXValidator() { Rules }

  - internal sealed class DeleteXXXEndpoint : IEndpoint
    - MapEndpoint()
      - app.MapDelete(...) (ISender handler, int/guid id, cancellationtoken ct)
        - var result = await Handler.Send(new DeleteXXXCommand(id), ct)
        - return result.Match(Results.NoContent{-other options exist-}, CustomResults.Problem)

- Create
  - public sealed record CreateXXXCommand(details) : IRequest<Result<id/guid>>
  - public sealed class CreateXXXCommand(context) : IRequestHandler<CreateXXXCommand, Result<int/guid>>
    - Handle() 
      - return Result.Success(), Result.Failure
  - public sealed class CreateXXXValidator : AbstractValidator<CreateXXXCommand>
    - public CreateXXXValidator() { Rules }

---
### API Endpoint Response Conventions

| Endpoint Type | Possible Response Codes | Code Short Description | Response Headers & Response Body |
| :--- | :--- | :--- | :--- |
| **`GET /collection`** <br/> (e.g., `GET /api/users`) | **200 OK** | **Success.** The request was successful, and the collection of resources is in the body. | **Headers:** `Content-Type: application/json` <br/> **Body:** A JSON array of the resources. **This should be an empty array `[]` if no items are found, not a 404.** |
| | **400 Bad Request** | The request included invalid parameters (e.g., an invalid filter or sort key). | **Headers:** `Content-Type: application/problem+json` <br/> **Body:** A JSON object detailing the error. |
| **`GET /collection/{id}`** <br/> (e.g., `GET /api/users/123`) | **200 OK** | **Success.** The specific resource was found and is in the body. | **Headers:** `Content-Type: application/json` <br/> **Body:** A single JSON object representing the resource. |
| | **404 Not Found** | **Resource not found.** The server could not find a resource matching the provided ID. | **Headers:** - <br/> **Body:** Typically empty, or a standard error object. |
| **`POST /collection`** <br/> (e.g., `POST /api/users`) | **201 Created** | **Success (Best Practice).** The resource was successfully created. | **Headers:** **`Location: /api/users/124`** (URL to the new resource). <br/> **Body:** (Optional but recommended) A JSON representation of the newly created resource, including its server-generated ID. |
| | **400 Bad Request** | **Validation failed.** The request body contained invalid or missing data. `[ApiController]` does this automatically. | **Headers:** `Content-Type: application/problem+json` <br/> **Body:** A JSON object detailing the model validation errors. |
| | **409 Conflict** | The resource could not be created because it would create a conflict (e.g., a user with that email already exists). | **Headers:** - <br/> **Body:** A JSON object explaining the nature of the conflict. |
| **`PUT /collection/{id}`** <br/> (e.g., `PUT /api/users/123`) | **204 No Content** | **Success (Best Practice).** The resource was fully updated. No body is returned as the client already has the new state. | **Headers:** - <br/> **Body:** **Empty.** |
| | **200 OK** | **Success (Alternative).** The resource was updated, and the server is returning the updated representation. | **Headers:** `Content-Type: application/json` <br/> **Body:** The full, updated JSON object. |
| | **404 Not Found** | The resource to be updated could not be found. | **Headers:** - <br/> **Body:** Typically empty. |
| | **400 Bad Request** | The request body for the update was invalid. | **Headers:** `Content-Type: application/problem+json` <br/> **Body:** A JSON object detailing the validation errors. |
| **`DELETE /collection/{id}`** <br/> (e.g., `DELETE /api/users/123`) | **204 No Content** | **Success.** The resource was successfully deleted. | **Headers:** - <br/> **Body:** **Empty.** |
| | **404 Not Found** | The resource to be deleted could not be found. | **Headers:** - <br/> **Body:** Typically empty. |

---

### Common/General Response Codes

These codes can be returned by almost any endpoint.

| Response Code | Code Short Description | When It's Used |
| :--- | :--- | :--- |
| **401 Unauthorized** | The client has not authenticated. | The request requires authentication, but no valid token (e.g., JWT Bearer token) was provided. The client should log in first. |
| **403 Forbidden** | The client is not allowed to perform this action. | The client is authenticated (logged in), but their role or permissions do not grant them access to this specific resource or action. |
| **500 Internal Server Error** | A generic, unhandled exception occurred on the server. | This indicates a **bug** in your API code. The response body should not expose sensitive details like stack traces in a production environment. |

### How to Implement This in .NET

*   `Ok(object)` -> **200 OK** with a body.
*   `CreatedAtAction("ActionName", routeValues, object)` -> **201 Created** with a `Location` header and a body.
*   `NoContent()` -> **204 No Content**.
*   `BadRequest(object)` -> **400 Bad Request** with a body detailing errors.
*   `NotFound()` -> **404 Not Found**.
*   `Conflict(object)` -> **409 Conflict** with a body.
*   `Forbid()` -> **403 Forbidden**.
*   `Unauthorized()` -> **401 Unauthorized**.