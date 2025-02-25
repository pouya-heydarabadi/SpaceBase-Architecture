using Identity.Api.Application.Interfaces;
using Identity.Api.Infrastructure.Redis;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Dtos;
using Identity.Api.Models.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRedisService>(config =>
{
    string? connectionString = builder.Configuration.GetSection("RedisConnectionString").Value;
    if (!connectionString.IsNullOrEmpty())
        return new RedisService(connectionString);
    throw new ApplicationException("Redis service not configured");
});

builder.Services.AddScoped(typeof(IRedisRepository<>), typeof(RedisRepository<>));


builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapScalarApiReference();
    app.MapOpenApi();
}

#region Endpoints

app.MapGet("/users", async (AppDbContext _dbContext) =>
    await _dbContext.Users.ToListAsync());

app.MapGet("/users/{id}", async (int id, AppDbContext _dbContext, IRedisRepository<User> _redisRepository) =>
{
    var findUserFromCache = await _redisRepository.GetAsync(id.ToString());
    if (findUserFromCache is null)
    {
        var findUserFromDatabase = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id)
                                   ?? throw new Exception("User not found");
        await _redisRepository.SetAsync(id.ToString(), findUserFromDatabase);
        return TypedResults.Ok(findUserFromDatabase);
    }
    return TypedResults.Ok(findUserFromCache);

});

app.MapPost("/users", async (CreateUserDto user, AppDbContext _dbContext, IRedisRepository<User> _redisRepository) =>
{

    User newUser = new User()
    {
        Address = user.Address,
        Email = user.Email,
        Name = user.Name,
        Age = user.Age,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
    };

    #region Redis

    await _redisRepository.SetAsync(user.PhoneNumber, newUser);

    #endregion

    _dbContext.Users.Add(newUser);
    await _dbContext.SaveChangesAsync();

    User? findUser = await _dbContext.Users
        .FirstOrDefaultAsync(x => x.PhoneNumber == user.PhoneNumber);

    return Results.Created($"/users/{findUser?.Id}", findUser);
});

app.MapPut("/users/{id}", async (int id, User inputUser, AppDbContext _dbContext) =>
{
    var user = await _dbContext.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Name = inputUser.Name;
    user.Email = inputUser.Email;
    user.Age = inputUser.Age;
    user.Address = inputUser.Address;
    user.PhoneNumber = inputUser.PhoneNumber;
    await _dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (int id, AppDbContext _dbContext) =>
{
    var user = await _dbContext.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    _dbContext.Users.Remove(user);
    await _dbContext.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

app.UseHttpsRedirection();

app.Run();

