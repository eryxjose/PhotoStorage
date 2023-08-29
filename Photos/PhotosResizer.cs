using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using static System.Net.Mime.MediaTypeNames;

namespace Photos
{
    public class PhotosResizer
    {
        [FunctionName("PhotosResizer")]
        public async Task Run([BlobTrigger("photos/{name}", Connection = Literals.StorageConnectionString)] Stream myBlob,
            [Blob("photos-small/{name}", FileAccess.Write, Connection = Literals.StorageConnectionString)] Stream imageSmall,
            [Blob("photos-medium/{name}", FileAccess.Write, Connection = Literals.StorageConnectionString)] Stream imageMedium,
            string name, ILogger logger)
        {
            logger?.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            logger?.LogInformation($"Redimensionando...");

            try
            {
                var msMedium = CreateMemoryStream(myBlob, ImageSize.Medium);
                await msMedium.CopyToAsync(imageMedium);

                var msSmall = CreateMemoryStream(myBlob, ImageSize.Small);
                await msSmall.CopyToAsync(imageSmall);

            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                throw;
            }
            finally
            {
                imageSmall.Close();
                imageMedium.Close();
            }
        }

        private static MemoryStream CreateMemoryStream(Stream image, ImageSize imageSize)
        {
            var ms = new MemoryStream();
            var img = Image.FromStream(image);
            var desiredWidth = imageSize == ImageSize.Medium ? img.Width / 2 :
                                                        img.Width / 4;
            var ratio = (decimal)desiredWidth / img.Width;
            var resized = ResizeImage(img, desiredWidth, (int)Math.Floor((img.Height * ratio)));
            resized.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private enum ImageSize
        {
            Medium,
            Small
        }
    }
}
