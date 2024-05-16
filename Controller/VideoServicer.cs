using Grpc.Core;
using Google.Protobuf;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace GrpcSpiderWatcher.Controller
{
    public class VideoServicer : VideoService.VideoServiceBase
    {
        private const int ChunkSize = 1024;
        private readonly string _connectionString;
        private readonly string _containerName;

        public VideoServicer(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public override async Task downloadVideo(DownloadFileRequest request, 
            IServerStreamWriter<DataChunkResponse> responseStream, 
            ServerCallContext context)
        {
            var buffer = new byte[ChunkSize];

            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);
            var blob = container.GetBlockBlobReference(request.Nombre);

            Console.WriteLine($"\n\nEnviando el archivo: {request.Nombre}");

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    memoryStream.Position = 0;
                    
                    int numBytesRead;
                    while ((numBytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await responseStream.WriteAsync(new DataChunkResponse
                        {
                            Data = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, numBytesRead))
                        });
                        Console.Write(".");
                    }
                }
                Console.WriteLine($"\n\nArchivo enviado: {request.Nombre}");
            }
            catch (Exception ex)
            {
                // Maneja la excepción según sea necesario
                Console.WriteLine($"Error al enviar el archivo: {ex.Message}");
            }
        }
    }
}
