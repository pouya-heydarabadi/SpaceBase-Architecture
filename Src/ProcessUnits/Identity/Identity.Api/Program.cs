using System.Text.Json;
using Confluent.Kafka;
using Identity.Api.Application;
using Identity.Api.Application.Interfaces;
using Identity.Api.Application.Jobs;
using Identity.Api.Infrastructure.Redis;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Dtos;
using Identity.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Redis
builder.Services.AddSingleton<IRedisService>(_ =>
{
    string? connectionString = builder.Configuration.GetSection("RedisConnectionString").Value;
    if (!connectionString.IsNullOrEmpty())
        return new RedisService(connectionString);
    throw new ApplicationException("Redis service not configured");
});

// Redis
builder.Services.AddScoped(typeof(IRedisRepository<>), typeof(RedisRepository<>));


// DbContext
builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// MediatR
builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(Program).Assembly);
});


#region Quartz

builder.Services.AddQuartz(options =>
{

    var jobKey = JobKey.Create(nameof(SyncUsersWithDatabaseJob));
    options
        .AddJob<SyncUsersWithDatabaseJob>(jobKey)
        .AddTrigger(trigger =>
            trigger.ForJob(jobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInMinutes(2).RepeatForever()));

    options.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService();
#endregion

builder.service.
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

app.MapGet("/users/{phoneNumber}", async (string phoneNumber, AppDbContext dbContext, IRedisRepository<User> redisRepository) =>
{
    var findUserFromCache = await redisRepository.GetAsync(phoneNumber.ToString());
    if (findUserFromCache is not null)
    {
        return TypedResults.Ok(findUserFromCache);
        
    }

    var findUserFromDatabase = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber)
                               ?? throw new Exception("User not found");
    await redisRepository.SortedSetAsync(phoneNumber.ToString(), findUserFromDatabase);
    return TypedResults.Ok(findUserFromDatabase);
    
});

app.MapPost("/users", async (UpdateCreateUserDto user, AppDbContext dbContext, IRedisRepository<User> redisRepository) =>
{

    
    DateTime now = DateTime.Now;
    User newUser = new User()
    {
        Address = user.Address,
        Email = user.Email,
        Name = user.Name,
        Age = user.Age,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = now,
        UpdatedAt = now,
    };
    
    #region Redis

    await redisRepository.SortedSetAsync(user.PhoneNumber, newUser);

    #endregion
    
    return Results.Created($"/users/{newUser.Id}", newUser);
});

app.MapPut("/users/{phoneNumber}", async (string phoneNumber, UpdateCreateUserDto inputUser, IRedisRepository<User> _redisRepository, AppDbContext _dbContext) =>
{
    
    var findUserFromCache = await _redisRepository.GetAsync(phoneNumber.ToString());
    if (findUserFromCache is null)
    {
        var findFromDatabase = await _dbContext.Users.FirstOrDefaultAsync(x=>x.PhoneNumber==phoneNumber)??throw new Exception("User not found");
        await _redisRepository.SortedSetAsync(phoneNumber, findFromDatabase);
        return TypedResults.Ok(findFromDatabase);
    }


    else
    {
        findUserFromCache.Address = inputUser.Address;
        findUserFromCache.Name = inputUser.Name;
        findUserFromCache.Email = inputUser.Email;
        findUserFromCache.Age = inputUser.Age;
        findUserFromCache.PhoneNumber = inputUser.PhoneNumber;
        findUserFromCache.UpdatedAt = DateTime.Now;
        
        await _redisRepository.SortedSetAsync(findUserFromCache.PhoneNumber, findUserFromCache);
        if(phoneNumber != findUserFromCache.PhoneNumber)
           await _redisRepository.DeleteAsync(phoneNumber);
        
        return TypedResults.Ok(findUserFromCache);
    }
    
    
    // var user = await dbContext.Users.FindAsync(id);
    // if (user is null) return Results.NotFound();
    //
    // user.Name = inputUser.Name;
    // user.Email = inputUser.Email;
    // user.Age = inputUser.Age;
    // user.Address = inputUser.Address;   
    // user.PhoneNumber = inputUser.PhoneNumber;
    // user.UpdatedAt = DateTime.Now;
    // await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/users/{phoneNumber}", async (string phoneNumber, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(phoneNumber);
    if (user is null) return Results.NotFound();

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

app.UseHttpsRedirection();

app.Run();

