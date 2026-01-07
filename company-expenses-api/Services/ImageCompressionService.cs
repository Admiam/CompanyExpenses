using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CompanyExpenses.Api.Services;

/// <summary>
/// Service interface for image compression operations.
/// </summary>
public interface IImageCompressionService
{
    /// <summary>
    /// Compresses an image and returns the result as a Base64 string.
    /// </summary>
    Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType);
}

/// <summary>
/// Image compression service that resizes and compresses images to JPEG format for storage optimization.
/// </summary>
public class ImageCompressionService : IImageCompressionService
{
    private readonly int _maxWidth;
    private readonly int _maxHeight;
    private readonly int _jpegQuality;
    private readonly ILogger<ImageCompressionService> _logger;

    public ImageCompressionService(IConfiguration configuration, ILogger<ImageCompressionService> logger)
    {
        _maxWidth = configuration.GetValue<int>("ImageCompression:MaxWidth", 800);
        _maxHeight = configuration.GetValue<int>("ImageCompression:MaxHeight", 800);
        _jpegQuality = configuration.GetValue<int>("ImageCompression:JpegQuality", 75);
        _logger = logger;
    }

    /// <summary>
    /// Compresses an image from Base64 input by resizing if necessary and converting to JPEG format.
    /// </summary>
    /// <param name="base64Input">The Base64 encoded image data.</param>
    /// <param name="contentType">The original content type of the image.</param>
    /// <returns>A tuple containing the compressed Base64 data and the compressed file size.</returns>
    public async Task<(string base64Data, long compressedSize)> CompressImageToBase64Async(string base64Input, string contentType)
    {
        _logger.LogInformation("Compressing image, original content type: {ContentType}", contentType);

        var imageBytes = Convert.FromBase64String(base64Input);
        var originalSize = imageBytes.Length;

        using var inputStream = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(inputStream);

        var originalWidth = image.Width;
        var originalHeight = image.Height;

        if (image.Width > _maxWidth || image.Height > _maxHeight)
        {
            var ratioX = (double)_maxWidth / image.Width;
            var ratioY = (double)_maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            _logger.LogInformation("Image resized from {OriginalWidth}x{OriginalHeight} to {NewWidth}x{NewHeight}",
                originalWidth, originalHeight, newWidth, newHeight);
        }

        using var outputStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = _jpegQuality };
        await image.SaveAsync(outputStream, encoder);

        var compressedBytes = outputStream.ToArray();
        var base64Data = Convert.ToBase64String(compressedBytes);

        _logger.LogInformation("Image compressed from {OriginalSize} bytes to {CompressedSize} bytes",
            originalSize, compressedBytes.Length);

        return (base64Data, compressedBytes.Length);
    }
}
