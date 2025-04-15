using System.Text.Json;
using Confluent.Kafka;
using Identity.Api.Application;
using Identity.Api.Application.Events.EventHandlers;
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
using Identity.Api.Infrastructure.Logging;

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

builder.Services.AddScoped<UserCreatedEventHandler>();
builder.Services.AddScoped<UserUpdatedEventHandler>();


#region Quartz
builder.Services.AddQuartz(options =>
{
    // Job 1: SendDataToMessageQueueJob
    var jobKey1 = JobKey.Create(nameof(SendDataToMessageQueueJob));
    options
        .AddJob<SendDataToMessageQueueJob>(jobKey1)
        .AddTrigger(trigger =>
            trigger.ForJob(jobKey1)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInMinutes(1).RepeatForever()));

    // Job 2: UserCreatedDatabaseSyncJob
    var userCreatedJobKey = JobKey.Create(nameof(UserCreatedDatabaseSyncJob));
    options
        .AddJob<UserCreatedDatabaseSyncJob>(userCreatedJobKey)
        .AddTrigger(trigger =>
            trigger.ForJob(userCreatedJobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInMinutes(1).RepeatForever()));

    // Job 3: UserUpdatedDatabaseSyncJob
    var userUpdatedJobKey = JobKey.Create(nameof(UserUpdatedDatabaseSyncJob));
    options
        .AddJob<UserUpdatedDatabaseSyncJob>(userUpdatedJobKey)
        .AddTrigger(trigger =>
            trigger.ForJob(userUpdatedJobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInMinutes(1).RepeatForever()));

    options.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService();
#endregion

builder.Services.AddOpenApi();

// Configure Serilog
builder.ConfigureSerilog();

var app = builder.Build();

// Add correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

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
    
    // Save to database
    await dbContext.Users.AddAsync(newUser);
    await dbContext.SaveChangesAsync();
    
    // Save to Redis
    await redisRepository.SortedSetAsync(user.PhoneNumber, newUser);
    
    return Results.Created($"/users/{newUser.PhoneNumber}", newUser);
});

app.MapPut("/users/{phoneNumber}", async (string phoneNumber, UpdateCreateUserDto inputUser, IRedisRepository<User> redisRepository, AppDbContext dbContext) =>
{
    // Find user in database
    var user = await dbContext.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
    if (user is null) return Results.NotFound();

    // Update user properties
    user.Address = inputUser.Address;
    user.Name = inputUser.Name;
    user.Email = inputUser.Email;
    user.Age = inputUser.Age;
    user.PhoneNumber = inputUser.PhoneNumber;
    user.UpdatedAt = DateTime.Now;

    // Update Redis
    await redisRepository.SortedSetAsync(user.PhoneNumber, user);
    if (phoneNumber != user.PhoneNumber)
        await redisRepository.DeleteAsync(phoneNumber);

    return Results.Ok(user);
});

app.MapDelete("/users/{phoneNumber}", async (string phoneNumber, AppDbContext dbContext, IRedisRepository<User> redisRepository) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
    if (user is null) return Results.NotFound();

    // Remove from database
    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();

    // Remove from Redis
    await redisRepository.DeleteAsync(phoneNumber);

    return Results.NoContent();
});

#endregion

app.UseHttpsRedirection();

app.Run();

