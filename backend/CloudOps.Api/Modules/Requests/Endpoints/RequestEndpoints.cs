using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Requests.Dtos;
using CloudOps.Api.Modules.Requests.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOps.Api.Modules.Requests.Endpoints;

public static class RequestEndpoints
{
    public static void MapRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/api/requests", async (AppDbContext dbContext) =>
        {
            var requests = await dbContext.ServiceRequests
                .Join(
                    dbContext.Users,
                    request => request.ClientId,
                    user => user.Id,
                    (request, user) => new { request, user }
                )
                .Join(
                    dbContext.Services,
                    combined => combined.request.ServiceId,
                    service => service.Id,
                    (combined, service) => new ServiceRequestDto
                    {
                        Id = combined.request.Id,
                        ClientName = combined.user.FullName,
                        ServiceName = service.Name,
                        Title = combined.request.Title,
                        Status = combined.request.Status
                    }
                )
                .ToListAsync();

            return Results.Ok(ApiResponse<List<ServiceRequestDto>>.Ok(
                requests,
                "Service requests retrieved successfully"
            ));
        })
        .WithName("GetServiceRequests")
        .WithTags("Requests")
        .WithOpenApi();

        app.MapGet("/api/requests/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var requestDto = await dbContext.ServiceRequests
                .Where(request => request.Id == id)
                .Join(
                    dbContext.Users,
                    request => request.ClientId,
                    user => user.Id,
                    (request, user) => new { request, user }
                )
                .Join(
                    dbContext.Services,
                    combined => combined.request.ServiceId,
                    service => service.Id,
                    (combined, service) => new ServiceRequestDto
                    {
                        Id = combined.request.Id,
                        ClientName = combined.user.FullName,
                        ServiceName = service.Name,
                        Title = combined.request.Title,
                        Status = combined.request.Status
                    }
                )
                .FirstOrDefaultAsync();

            if (requestDto is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service request not found"));
            }

            return Results.Ok(ApiResponse<ServiceRequestDto>.Ok(
                requestDto,
                "Service request retrieved successfully"
            ));
        })
        .WithName("GetServiceRequestById")
        .WithTags("Requests")
        .WithOpenApi();

        app.MapPost("/api/requests", async (CreateServiceRequestRequest request, AppDbContext dbContext) =>
        {
            var clientExists = await dbContext.Users.AnyAsync(user => user.Id == request.ClientId);

            if (!clientExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Client not found"));
            }

            var serviceExists = await dbContext.Services.AnyAsync(service => service.Id == request.ServiceId);

            if (!serviceExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Service not found"));
            }

            var serviceRequest = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ServiceId = request.ServiceId,
                Title = request.Title,
                Description = request.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.ServiceRequests.Add(serviceRequest);
            await dbContext.SaveChangesAsync();

            return Results.Created(
                $"/api/requests/{serviceRequest.Id}",
                ApiResponse<object>.Ok(
                    new { serviceRequest.Id },
                    "Service request created successfully"
                )
            );
        })
        .WithName("CreateServiceRequest")
        .WithTags("Requests")
        .WithOpenApi();

        app.MapPut("/api/requests/{id:guid}", async (Guid id, UpdateServiceRequestRequest request, AppDbContext dbContext) =>
        {
            var serviceRequest = await dbContext.ServiceRequests.FindAsync(id);

            if (serviceRequest is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service request not found"));
            }

            serviceRequest.Title = request.Title;
            serviceRequest.Description = request.Description;
            serviceRequest.Status = request.Status;

            await dbContext.SaveChangesAsync();

            return Results.Ok(ApiResponse<object>.Ok(
                new { serviceRequest.Id },
                "Service request updated successfully"
            ));
        })
        .WithName("UpdateServiceRequest")
        .WithTags("Requests")
        .WithOpenApi();

        app.MapDelete("/api/requests/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var serviceRequest = await dbContext.ServiceRequests.FindAsync(id);

            if (serviceRequest is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service request not found"));
            }

            dbContext.ServiceRequests.Remove(serviceRequest);
            await dbContext.SaveChangesAsync();

            return Results.Ok(ApiResponse<object>.Ok(
                null,
                "Service request deleted successfully"
            ));
        })
        .WithName("DeleteServiceRequest")
        .WithTags("Requests")
        .WithOpenApi();
    }
}