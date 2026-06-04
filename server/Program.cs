using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using server.Data;
using server.Interfaces;
using server.Models;
using server.Repositories;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// configure AWS
builder.Services.Configure<S3Settings>(
    builder.Configuration.GetSection("AWS")
);
builder.Services.Configure<UploadSettings>(
    builder.Configuration.GetSection("Upload")
);
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var settings = sp
        .GetRequiredService<IOptions<S3Settings>>()
        .Value;

    var credentials = new BasicAWSCredentials(
        settings.AccessKey,
        settings.SecretKey
    );

    var config = new AmazonS3Config
    {
        RegionEndpoint =
            RegionEndpoint.GetBySystemName(
                settings.Region
            )
    };  

    return new AmazonS3Client(
        credentials,
        config
    );
});
builder.Services.AddScoped<IS3FileStorageService, S3FileStorageService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IS3ItemRepository, S3ItemRepository>();
builder.Services.AddScoped<IS3ItemService, S3ItemService>();
builder.Services.AddScoped<IS3UploadService, S3UploadService>();

// configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("MyAllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();

app.Run();
