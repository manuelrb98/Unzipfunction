using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace unzip
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([BlobTrigger("input-files/{name}", Connection = "connectionstring")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            string destinationStorage = Environment.GetEnvironmentVariable("destinationStorage");
            string destinationContainer = Environment.GetEnvironmentVariable("destinationContainer");

            try
            {
                if (name.Split('.').Last().ToLower() == "zip")
                {
                    ZipArchive archive = new ZipArchive(myBlob);

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(destinationStorage);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(destinationContainer);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        log.LogInformation($"Now proccessing {entry.FullName}");

                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(entry.Name);
                        using (var fileStream = entry.Open())
                        {
                            blockBlob.UploadFromStreamAsync(fileStream);
                        }
                    }


                }
            }
            catch( Exception ex) {
                log.LogInformation($"Ërror {ex.Message}");
                
                }
            
            }



        }
    }

