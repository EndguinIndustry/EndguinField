using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        var region = Environment.GetEnvironmentVariable("AWS_REGION");
        _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? throw new InvalidOperationException("AWS_BUCKET_NAME environment variable is not set");

        _s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            RegionEndpoint.GetBySystemName(region)
        );
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        using var stream = file.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = fileName,
            BucketName = _bucketName,
            ContentType = file.ContentType
        };

        var trasferUtility = new TransferUtility(_s3Client);
        await trasferUtility.UploadAsync(uploadRequest);

        return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
    }
}