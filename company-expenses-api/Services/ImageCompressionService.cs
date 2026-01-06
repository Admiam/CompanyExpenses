using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CompanyExpenses.Api.Services;

public interface IImageCompressionService
{
    Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType);
}

public class ImageCompressionService : IImageCompressionService
{
    private readonly int _maxWidth;
    private readonly int _maxHeight;
    private readonly int _jpegQuality;

    public ImageCompressionService(IConfiguration configuration)
    {
        _maxWidth = configuration.GetValue<int>("ImageCompression:MaxWidth", 800);
        _maxHeight = configuration.GetValue<int>("ImageCompression:MaxHeight", 800);
        _jpegQuality = configuration.GetValue<int>("ImageCompression:JpegQuality", 75);
    }

    public async Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType)
    {
        // Decode base64 to bytes
        var imageBytes = Convert.FromBase64String(base64Input);

        using var inputStream = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(inputStream);

        // Resize if image is too large
        if (image.Width > _maxWidth || image.Height > _maxHeight)
        {
            var ratioX = (double)_maxWidth / image.Width;
            var ratioY = (double)_maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Compress and convert to JPEG
        using var outputStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = _jpegQuality };
        await image.SaveAsync(outputStream, encoder);

        // Convert to base64
        var compressedBytes = outputStream.ToArray();
        var base64Data = Convert.ToBase64String(compressedBytes);

        return (base64Data, compressedBytes.Length);
    }
}
