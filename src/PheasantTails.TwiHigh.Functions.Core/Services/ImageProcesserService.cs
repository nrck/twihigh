using SkiaSharp;

namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public class ImageProcesserService : IImageProcesserService
    {
        public byte[] TrimmingToSquare(in byte[] buffer, SKEncodedImageFormat format)
        {
            if (buffer == null || !buffer.Any())
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            // オリジナルの取得
            using var originImage = SKBitmap.Decode(buffer);

            // 一辺の長さを設定
            var sideSize = originImage.Width < originImage.Height ? originImage.Width : originImage.Height;

            // クリッピング領域の作成
            var rect = new SKRectI((originImage.Width - sideSize) / 2, (originImage.Height - sideSize) / 2, (originImage.Width + sideSize) / 2, (originImage.Height + sideSize) / 2);

            // トリミング後の領域の作成
            using var newImage = new SKBitmap(rect.Width, rect.Height);

            // クリッピング領域で新しい画像を作成
            originImage.ExtractSubset(newImage, rect);

            var data = newImage.Resize(new SKSizeI(400, 400), SKFilterQuality.High)
                .Encode(format, 80).ToArray();

            return data;
        }
    }
}
