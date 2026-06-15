using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Notifications.Dtos;
using CloudOps.Api.Modules.Notifications.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOps.Api.Modules.Notifications.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/notifications", async (AppDbContext dbContext) =>
        {
            var notifications = await dbContext.Notifications
                .Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<List<NotificationDto>>.Ok(
                notifications,
                "Notifications retrieved successfully"
            ));
        })
        .WithName("GetNotifications")
        .WithTags("Notifications")
        .WithOpenApi();

        app.MapGet("/api/notifications/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var notification = await dbContext.Notifications
                .Where(notification => notification.Id == id)
                .Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead
                })
                .FirstOrDefaultAsync();

            if (notification is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Notification not found"));
            }

            return Results.Ok(ApiResponse<NotificationDto>.Ok(
                notification,
                "Notification retrieved successfully"
            ));
        })
        .WithName("GetNotificationById")
        .WithTags("Notifications")
        .WithOpenApi();

        app.MapPost("/api/notifications", async (
            CreateNotificationRequest request,
            AppDbContext dbContext) =>
        {
            var userExists = await dbContext.Users
                .AnyAsync(user => user.Id == request.UserId);

            if (!userExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("User not found"));
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead
            };

            return Results.Created(
                $"/api/notifications/{notification.Id}",
                ApiResponse<NotificationDto>.Ok(
                    notificationDto,
                    "Notification created successfully"
                )
            );
        })
        .WithName("CreateNotification")
        .WithTags("Notifications")
        .WithOpenApi();

        app.MapPut("/api/notifications/{id:guid}", async (
            Guid id,
            UpdateNotificationRequest request,
            AppDbContext dbContext) =>
        {
            var notification = await dbContext.Notifications.FindAsync(id);

            if (notification is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Notification not found"));
            }

            notification.Title = request.Title;
            notification.Message = request.Message;
            notification.IsRead = request.IsRead;

            await dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead
            };

            return Results.Ok(ApiResponse<NotificationDto>.Ok(
                notificationDto,
                "Notification updated successfully"
            ));
        })
        .WithName("UpdateNotification")
        .WithTags("Notifications")
        .WithOpenApi();
        app.MapGet("/api/users/{userId:guid}/notifications", async (
    Guid userId,
    AppDbContext dbContext) =>
        {
            var userExists = await dbContext.Users
                .AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                return Results.NotFound(ApiResponse<object>.Fail("User not found"));
            }

            var notifications = await dbContext.Notifications
                .Where(notification => notification.UserId == userId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<List<NotificationDto>>.Ok(
                notifications,
                "User notifications retrieved successfully"
            ));
        })
        .WithName("GetUserNotifications")
        .WithTags("Notifications")
        .WithOpenApi();

        app.MapGet("/api/users/{userId:guid}/notifications/unread-count", async (
           Guid userId,
           AppDbContext dbContext) =>
        {
            var userExists = await dbContext.Users
                .AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                return Results.NotFound(ApiResponse<object>.Fail("User not found"));
            }

            var unreadCount = await dbContext.Notifications
                .CountAsync(notification =>
                    notification.UserId == userId &&
                    notification.IsRead == false
                );

            return Results.Ok(ApiResponse<object>.Ok(
                new { UnreadCount = unreadCount },
                "Unread notifications count retrieved successfully"
            ));
        })
        .WithName("GetUnreadNotificationsCount")
        .WithTags("Notifications")
        .WithOpenApi();
        app.MapPut("/api/notifications/{id:guid}/mark-as-read", async (
            Guid id,
            AppDbContext dbContext) =>
        {
            var notification = await dbContext.Notifications.FindAsync(id);

            if (notification is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Notification not found"));
            }

            notification.IsRead = true;

            await dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead
            };

            return Results.Ok(ApiResponse<NotificationDto>.Ok(
                notificationDto,
                "Notification marked as read successfully"
            ));
        })
        .WithName("MarkNotificationAsRead")
        .WithTags("Notifications")
        .WithOpenApi();

        app.MapDelete("/api/notifications/{id:guid}", async (
            Guid id,
            AppDbContext dbContext) =>
        {
            var notification = await dbContext.Notifications.FindAsync(id);

            if (notification is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("Notification not found"));
            }

            dbContext.Notifications.Remove(notification);
            await dbContext.SaveChangesAsync();

            return Results.Ok(ApiResponse<object>.Ok(
                null,
                "Notification deleted successfully"
            ));
        })
        .WithName("DeleteNotification")
        .WithTags("Notifications")
        .WithOpenApi();
    }
}