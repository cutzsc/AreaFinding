using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IslandSquare
{
    public class Picture
    {
        BitmapImage bitmapImage;
        int stride;
        byte[] pixels;

        WriteableBitmap bitmap;
        public WriteableBitmap Bitmap { get { return bitmap; }
            private set
            {
                bitmap = value;
            }
        }
        
        public Picture(string path)
        {
            bitmapImage = new BitmapImage(new Uri(path));
            Bitmap = new WriteableBitmap(new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, null, 0));
            stride = Bitmap.PixelWidth * 4;
            pixels = new byte[stride * Bitmap.PixelHeight];
            Bitmap.CopyPixels(pixels, stride, 0);
        }

        public void SetPixelColor(int x, int y, Color c)
        {
            byte[] pixelColor = { c.B, c.G, c.R, c.A };
            Bitmap.WritePixels(new Int32Rect(x, y, 1, 1), pixelColor, stride, 0);
        }

        public Color GetPixelColor(int x, int y)
        {
            return Color.FromArgb(
                pixels[y * stride + x * 4 + 3],
                pixels[y * stride + x * 4 + 2],
                pixels[y * stride + x * 4 + 1],
                pixels[y * stride + x * 4]);
        }

        public void Refresh()
        {
            Bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, stride, 0);
        }
    }
}
