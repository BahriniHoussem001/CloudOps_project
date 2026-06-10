using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Users.Dtos;
using CloudOps.Api.Modules.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOps.Api.Modules.Users.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users", async (AppDbContext dbContext) =>
        {
            var users = await dbContext.Users
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<List<UserDto>>.Ok(
                users,
                "Users retrieved successfully"
            ));
        })
        .WithName("GetUsers")
        .WithTags("Users")
        .WithOpenApi();

        app.MapGet("/api/users/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .Where(user => user.Id == id)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("User not found"));
            }

            return Results.Ok(ApiResponse<UserDto>.Ok(
                user,
                "User retrieved successfully"
            ));
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .WithOpenApi();

        app.MapPost("/api/users", async (CreateUserRequest request, AppDbContext dbContext) =>
        {
            var emailExists = await dbContext.Users
                .AnyAsync(user => user.Email == request.Email);

            if (emailExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Email already exists"));
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Results.Created(
                $"/api/users/{user.Id}",
                ApiResponse<UserDto>.Ok(userDto, "User created successfully")
            );
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithOpenApi();

        app.MapPut("/api/users/{id:guid}", async (Guid id, UpdateUserRequest request, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("User not found"));
            }

            var emailExists = await dbContext.Users
                .AnyAsync(existingUser => existingUser.Email == request.Email && existingUser.Id != id);

            if (emailExists)
            {
                return Results.BadRequest(ApiResponse<object>.Fail("Email already exists"));
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Role = request.Role;

            await dbContext.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Results.Ok(ApiResponse<UserDto>.Ok(
                userDto,
                "User updated successfully"
            ));
        })
        .WithName("UpdateUser")
        .WithTags("Users")
        .WithOpenApi();

        app.MapDelete("/api/users/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("User not found"));
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(ApiResponse<object>.Ok(
                null,
                "User deleted successfully"
            ));
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithOpenApi();
    }
}