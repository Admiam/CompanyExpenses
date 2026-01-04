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
    private const int MaxWidth = 800;
    private const int MaxHeight = 800;
    private const int JpegQuality = 75;

    public async Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType)
    {
        // Decode base64 to bytes
        var imageBytes = Convert.FromBase64String(base64Input);

        using var inputStream = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(inputStream);

        // Resize if image is too large
        if (image.Width > MaxWidth || image.Height > MaxHeight)
        {
            var ratioX = (double)MaxWidth / image.Width;
            var ratioY = (double)MaxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Compress and convert to JPEG
        using var outputStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = JpegQuality };
        await image.SaveAsync(outputStream, encoder);

        // Convert to base64
        var compressedBytes = outputStream.ToArray();
        var base64Data = Convert.ToBase64String(compressedBytes);

        return (base64Data, compressedBytes.Length);
    }
}
