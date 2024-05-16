using GrpcSpiderWatcher.Controller;

var builder = WebApplication.CreateBuilder(args);

// Configuración del servicio gRPC
builder.Services.AddGrpc();

// Configuración de las variables de conexión al Blob Storage y el nombre del contenedor
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("AzureBlobStorage");
var containerName = configuration["BlobContainerName"];

// Agregar el servicio VideoServicer como un servicio de aplicación
if (connectionString != null && containerName != null)
{
    builder.Services.AddSingleton<VideoServicer>(new VideoServicer(connectionString, containerName));
}

var app = builder.Build();

// Configurar el middleware de gRPC
app.MapGrpcService<VideoServicer>();

// Ejecutar la aplicación
app.Run();
