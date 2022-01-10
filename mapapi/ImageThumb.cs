using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi
{
    public static class ImageThumb
    {
        public static byte[] GetReducedImage(int width, int height, Stream resourceImage)
        {
            //try
            //{
                Image image = Image.FromStream(resourceImage);
                var orig_width = image.Width;
                var orig_height = image.Height;
                int newcoord_y, newcoord_x;
                if (orig_width > orig_height)
                {
                    newcoord_y = 0;
                    newcoord_x = (orig_width - orig_height) / 2;
                    orig_width = orig_height;
                }
                else
                {
                    newcoord_y = (orig_height - orig_width) / 2;
                    newcoord_x = 0;
                    orig_height = orig_width;
                }
                Image cropedImage = CropImage(image, new Rectangle(newcoord_x, newcoord_y, orig_width, orig_height));
                Image thumb = cropedImage.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
                //Image thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
                using (var ms = new MemoryStream())
                {
                    thumb.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            //}
            //catch (Exception e)
            //{
            //    return null;
            //}
        }

        public static Image CropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            Bitmap croped = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            Bitmap target = new Bitmap(croped.Width, croped.Height);
            using (var g = Graphics.FromImage(target))
            {
                g.Clear(Color.White);
                g.DrawImageUnscaled(croped, 0, 0);
            }
            return target;
        }
    }
}
