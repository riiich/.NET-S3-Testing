using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using server.Models;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
builder.Services.AddScoped<S3FileStorageService>();

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