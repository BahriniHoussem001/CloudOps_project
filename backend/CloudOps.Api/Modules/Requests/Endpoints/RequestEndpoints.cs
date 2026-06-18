using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Messaging;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Requests.Dtos;
using CloudOps.Api.Modules.Requests.Events;
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

        app.MapPost("/api/requests", async (
            CreateServiceRequestRequest request,
            AppDbContext dbContext,
            IMessagePublisher messagePublisher,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("RequestEndpoints");

            var clientExists = await dbContext.Users
                .AnyAsync(user => user.Id == request.ClientId);

            if (!clientExists)
            {
                logger.LogWarning(
                    "Failed to create service request because client {ClientId} was not found",
                    request.ClientId
                );

                return Results.BadRequest(ApiResponse<object>.Fail("Client not found"));
            }

            var serviceExists = await dbContext.Services
                .AnyAsync(service => service.Id == request.ServiceId);

            if (!serviceExists)
            {
                logger.LogWarning(
                    "Failed to create service request because service {ServiceId} was not found",
                    request.ServiceId
                );

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

            logger.LogInformation(
                "Service request {RequestId} created for client {ClientId} and service {ServiceId}",
                serviceRequest.Id,
                serviceRequest.ClientId,
                serviceRequest.ServiceId
            );

            var requestCreatedEvent = new RequestCreatedEvent
            {
                RequestId = serviceRequest.Id,
                ClientId = serviceRequest.ClientId,
                ServiceId = serviceRequest.ServiceId,
                Title = serviceRequest.Title,
                Status = serviceRequest.Status,
                CreatedAt = serviceRequest.CreatedAt
            };

            await messagePublisher.PublishAsync(
                "request-created-queue",
                requestCreatedEvent
            );

            logger.LogInformation(
                "RequestCreatedEvent published to RabbitMQ for request {RequestId}",
                serviceRequest.Id
            );

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

        app.MapPut("/api/requests/{id:guid}", async (
            Guid id,
            UpdateServiceRequestRequest request,
            AppDbContext dbContext,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("RequestEndpoints");

            var serviceRequest = await dbContext.ServiceRequests.FindAsync(id);

            if (serviceRequest is null)
            {
                logger.LogWarning(
                    "Failed to update service request {RequestId} because it was not found",
                    id
                );

                return Results.NotFound(ApiResponse<object>.Fail("Service request not found"));
            }

            serviceRequest.Title = request.Title;
            serviceRequest.Description = request.Description;
            serviceRequest.Status = request.Status;

            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Service request {RequestId} updated successfully with status {Status}",
                serviceRequest.Id,
                serviceRequest.Status
            );

            return Results.Ok(ApiResponse<object>.Ok(
                new { serviceRequest.Id },
                "Service request updated successfully"
            ));
        })
        .WithName("UpdateServiceRequest")
        .WithTags("Requests")
        .WithOpenApi();

        app.MapDelete("/api/requests/{id:guid}", async (
            Guid id,
            AppDbContext dbContext,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("RequestEndpoints");

            var serviceRequest = await dbContext.ServiceRequests.FindAsync(id);

            if (serviceRequest is null)
            {
                logger.LogWarning(
                    "Failed to delete service request {RequestId} because it was not found",
                    id
                );

                return Results.NotFound(ApiResponse<object>.Fail("Service request not found"));
            }

            dbContext.ServiceRequests.Remove(serviceRequest);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Service request {RequestId} deleted successfully",
                id
            );

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