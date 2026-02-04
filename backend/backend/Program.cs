using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;


const string endpoint = "http://localhost:9000";
const string accessKey = "minio";
const string secretKey = "minio12345";
const string bucket = "supportway";

var config = new AmazonS3Config
{
    ServiceURL = endpoint,
    ForcePathStyle = true
};

using var client = new AmazonS3Client(accessKey, secretKey, config);

// 1) Перевір/створи bucket
try
{
    await client.HeadBucketAsync(new Amazon.S3.Model.HeadBucketRequest
    {
        BucketName = bucket
    });

    Console.WriteLine($"Bucket '{bucket}' exists");
}
catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    await client.PutBucketAsync(new Amazon.S3.Model.PutBucketRequest
    {
        BucketName = bucket
    });

    Console.WriteLine($"Bucket '{bucket}' created");
}


// 2) Upload простого файлу
var key = $"test/{Guid.NewGuid():N}.txt";
await client.PutObjectAsync(new PutObjectRequest
{
    BucketName = bucket,
    Key = key,
    ContentBody = "Hello from SupportWay console app!"
});

Console.WriteLine($"Uploaded: {bucket}/{key}");

// 3) Download і вивід
var get = await client.GetObjectAsync(new GetObjectRequest
{
    BucketName = bucket,
    Key = key
});

using var reader = new StreamReader(get.ResponseStream);
var text = await reader.ReadToEndAsync();

Console.WriteLine("Downloaded content:");
Console.WriteLine(text);
