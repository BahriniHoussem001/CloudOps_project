using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Services.Dtos;
using CloudOps.Api.Modules.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOps.Api.Modules.Services.Endpoints;

public static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this WebApplication app)
    {
        app.MapGet("/api/services", async (AppDbContext dbContext) =>
        {
            var services = await dbContext.Services
                .Select(service => new ServiceItemDto
                {
                    Id = service.Id,
                    Name = service.Name,
                    Category = service.Category,
                    Description = service.Description
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<List<ServiceItemDto>>.Ok(
                services,
                "Services retrieved successfully"
            ));
        })
        .WithName("GetServices")
        .WithTags("Services")
        .WithOpenApi();

        app.MapGet("/api/services/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var service = await dbContext.Services
                .Where(service => service.Id == id)
                .Select(service => new ServiceItemDto
                {
                    Id = service.Id,
                    Name = service.Name,
                    Category = service.Category,
                    Description = service.Description
                })
                .FirstOrDefaultAsync();

            if (service is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service not found"));
            }

            return Results.Ok(ApiResponse<ServiceItemDto>.Ok(
                service,
                "Service retrieved successfully"
            ));
        })
        .WithName("GetServiceById")
        .WithTags("Services")
        .WithOpenApi();

        app.MapPost("/api/services", async (CreateServiceRequest request, AppDbContext dbContext) =>
        {
            var serviceExists = await dbContext.Services
                .AnyAsync(service => service.Name == request.Name);

            if (serviceExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Service already exists"));
            }

            var serviceItem = new ServiceItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Services.Add(serviceItem);
            await dbContext.SaveChangesAsync();

            var serviceDto = new ServiceItemDto
            {
                Id = serviceItem.Id,
                Name = serviceItem.Name,
                Category = serviceItem.Category,
                Description = serviceItem.Description
            };

            return Results.Created(
                $"/api/services/{serviceItem.Id}",
                ApiResponse<ServiceItemDto>.Ok(serviceDto, "Service created successfully")
            );
        })
        .WithName("CreateService")
        .WithTags("Services")
        .WithOpenApi();

        app.MapPut("/api/services/{id:guid}", async (Guid id, UpdateServiceRequest request, AppDbContext dbContext) =>
        {
            var serviceItem = await dbContext.Services.FindAsync(id);

            if (serviceItem is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service not found"));
            }

            var serviceExists = await dbContext.Services
                .AnyAsync(existingService => existingService.Name == request.Name && existingService.Id != id);

            if (serviceExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Service name already exists"));
            }

            serviceItem.Name = request.Name;
            serviceItem.Category = request.Category;
            serviceItem.Description = request.Description;
            serviceItem.IsActive = request.IsActive;

            await dbContext.SaveChangesAsync();

            var serviceDto = new ServiceItemDto
            {
                Id = serviceItem.Id,
                Name = serviceItem.Name,
                Category = serviceItem.Category,
                Description = serviceItem.Description
            };

            return Results.Ok(ApiResponse<ServiceItemDto>.Ok(
                serviceDto,
                "Service updated successfully"
            ));
        })
        .WithName("UpdateService")
        .WithTags("Services")
        .WithOpenApi();

        app.MapDelete("/api/services/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var serviceItem = await dbContext.Services.FindAsync(id);

            if (serviceItem is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Service not found"));
            }

            dbContext.Services.Remove(serviceItem);
            await dbContext.SaveChangesAsync();

            return Results.Ok(ApiResponse<object>.Ok(
                null,
                "Service deleted successfully"
            ));
        })
        .WithName("DeleteService")
        .WithTags("Services")
        .WithOpenApi();
    }
}