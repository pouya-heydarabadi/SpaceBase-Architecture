using Identity.Api.Application.Interfaces;
using Identity.Api.Infrastructure.Redis;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Dtos;
using Identity.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRedisService>(_ =>
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

app.MapGet("/users", async (AppDbContext dbContext) =>
    await dbContext.Users.ToListAsync());

app.MapGet("/users/{id}", async (int id, AppDbContext dbContext, IRedisRepository<User> redisRepository) =>
{
    var findUserFromCache = await redisRepository.GetAsync(id.ToString());
    if (findUserFromCache is null)
    {
        var findUserFromDatabase = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id)
                                   ?? throw new Exception("User not found");
        await redisRepository.SetAsync(id.ToString(), findUserFromDatabase);
        return TypedResults.Ok(findUserFromDatabase);
    }
    return TypedResults.Ok(findUserFromCache);

});

app.MapPost("/users", async (CreateUserDto user, AppDbContext dbContext, IRedisRepository<User> redisRepository) =>
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

    await redisRepository.SetAsync(user.PhoneNumber, newUser);

    #endregion

    dbContext.Users.Add(newUser);
    await dbContext.SaveChangesAsync();

    User? findUser = await dbContext.Users
        .FirstOrDefaultAsync(x => x.PhoneNumber == user.PhoneNumber);

    return Results.Created($"/users/{findUser?.Id}", findUser);
});

app.MapPut("/users/{id}", async (int id, User inputUser, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Name = inputUser.Name;
    user.Email = inputUser.Email;
    user.Age = inputUser.Age;
    user.Address = inputUser.Address;
    user.PhoneNumber = inputUser.PhoneNumber;
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (int id, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

app.UseHttpsRedirection();

app.Run();

