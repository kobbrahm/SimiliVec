using Microsoft.AspNetCore.Hosting.Builder;
using VectorDataBase.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; 

// Just so you can connect whatever to the api
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Allow access from any origin
                          policy.AllowAnyOrigin() 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddVectorDataBaseServices();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize and index documents on startup BEFORE starting the app
Console.WriteLine("Pre-startup: About to index documents...");
try
{
    var vectorService = app.Services.GetRequiredService<IVectorService>();
    Console.WriteLine("Pre-startup: Got vector service");
    var indexTask = vectorService.IndexDocument();
    indexTask.Wait(); // Block and wait for indexing to complete
    Console.WriteLine("Pre-startup: Documents indexed on startup successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Pre-startup error indexing documents: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();