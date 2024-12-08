using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using pinq.api.Filters;
using pinq.api.Repository;
using pinq.api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectName"]}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidIssuer = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectName"]}",
            ValidateAudience = false,
            ValidAudience = builder.Configuration["Firebase:ProjectName"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(builder.Configuration["Firebase:CredentialsFileLocation"])
}));
builder.Services.AddScoped<IStorageService, FirebaseStorageService>();

builder.Services.AddScoped<IDbConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRepository>();

builder.Services.AddMvc()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    });

builder.Services.AddSingleton(ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
builder.Services.AddScoped<ISessionDatabaseService, SessionDatabaseService>();
builder.Services.AddScoped<ISessionCacheService, RedisSessionCacheService>();
builder.Services.AddTransient<ValidateSessionAttribute>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();