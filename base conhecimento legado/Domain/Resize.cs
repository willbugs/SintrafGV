using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
namespace Domain
{public class Resize
    {
        public string ResizeBase64(string base64String, int toWidth, int toHeight)
        {

            var posicao = base64String.IndexOf(",", StringComparison.Ordinal) + 1;
            var tamanho = base64String.Length - posicao;
            base64String = base64String.Substring(posicao , tamanho);


            var resizedImage = ResizeImage(base64String, new Size { Width = toWidth, Height = toHeight });
            return string.Format("data:image/jpeg;base64,{0}", resizedImage);
        }
        private static Image Base64ToImage(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            var ms = new MemoryStream(imageBytes, 0,imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            var image = Image.FromStream(ms, true);
            return image;
        }
        private static string ResizeImage(string base64String, Size size)
        {
            var imgToResize = Base64ToImage(base64String);
            var sourceWidth = imgToResize.Width;
            var sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = (size.Width / (float)sourceWidth);
            nPercentH = (size.Height / (float)sourceHeight);

            nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

            var destWidth = (int)(sourceWidth * nPercent);
            var destHeight = (int)(sourceHeight * nPercent);

            var b = new Bitmap(destWidth, destHeight);
            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            string base64ImageString;

            using (var ms = new MemoryStream())
            {
                var qualityParam = new EncoderParameter(Encoder.Quality, 90L);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                var jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                b.Save(ms, jgpEncoder, encoderParams);
                var imageBytes = ms.ToArray();
                base64ImageString = Convert.ToBase64String(imageBytes);
            }
            return base64ImageString;
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}