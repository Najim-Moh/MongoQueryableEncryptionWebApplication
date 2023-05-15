///Created by Udaybhanu Karmakar
///2023-05-12
///To test Mongo DB client side field level encryption (CSFLE)
///With docker (Linux X64)

using MongoWebApplication.Models;
using MongoWebApplication.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
// Add services to the container.
builder.Services.Configure<MedicalRecordsStoreDatabaseSettings>(
    builder.Configuration.GetSection("MedicalRecordsDatabase"));

builder.Services.AddControllers().AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<BooksService>();
builder.Services.AddSingleton<PatientsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
