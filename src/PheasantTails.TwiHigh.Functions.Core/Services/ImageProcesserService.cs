﻿using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public class ImageProcesserService : IImageProcesserService
    {
        private readonly ILogger<ImageProcesserService> _logger;

        public ImageProcesserService(ILogger<ImageProcesserService> log)
        {
            _logger = log;
        }

        public byte[] TrimmingToSquare(in byte[] buffer, SKEncodedImageFormat format)
        {
            try
            {
                if (buffer == null || !buffer.Any())
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                // オリジナルの取得
                _logger.LogInformation("{0}:: Get origin images.", nameof(TrimmingToSquare));
                using var originImage = SKBitmap.Decode(buffer);

                // 一辺の長さを設定
                _logger.LogInformation("{0}:: Set side size from origin width or height.", nameof(TrimmingToSquare));
                var sideSize = originImage.Width < originImage.Height ? originImage.Width : originImage.Height;

                // クリッピング領域の作成
                _logger.LogInformation("{0}:: Set rect.", nameof(TrimmingToSquare));
                var rect = new SKRectI((originImage.Width - sideSize) / 2, (originImage.Height - sideSize) / 2, (originImage.Width + sideSize) / 2, (originImage.Height + sideSize) / 2);

                // トリミング後の領域の作成
                _logger.LogInformation("{0}:: Get Skia bitmap object.", nameof(TrimmingToSquare));
                using var newImage = new SKBitmap(rect.Width, rect.Height);

                // クリッピング領域で新しい画像を作成
                _logger.LogInformation("{0}:: Get extract subset.", nameof(TrimmingToSquare));
                originImage.ExtractSubset(newImage, rect);

                // 400x400にリサイズ
                _logger.LogInformation("{0}:: Resize 400x400.", nameof(TrimmingToSquare));
                var data = newImage.Resize(new SKSizeI(400, 400), SKFilterQuality.High)
                    .Encode(format, 80).ToArray();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception happend at {0}", ex.Source);
                _logger.LogInformation(ex.StackTrace);
                throw;
            }
        }
    }
}