using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    class ImageFilters
    {
        // article on Kaggle scanned document denoising challenge

        // https://medium.com/illuin/cleaning-up-dirty-scanned-documents-with-deep-learning-2e8e6de6cfa6
        static Bitmap Process(Bitmap bmp, Action<byte[],int,int,int> processAction)
        {
            int w = bmp.Width, h = bmp.Height;
            var rect = new Rectangle(0, 0, w, h);
            bmp = bmp.Clone(rect, PixelFormat.Format24bppRgb);
            var data = bmp.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = data.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int stride = Math.Abs(data.Stride);
            int bytes = stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
            //for (int counter = 2; counter < rgbValues.Length; counter += 3)
            //    rgbValues[counter] = 255;
            processAction(rgbValues, w, h, stride);

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);


            bmp.UnlockBits(data);
            return bmp;

        }
        public static Bitmap ToBlackAndWhite(Bitmap bmp)
        {
            return Process(bmp, (pixels, w, h, stride) =>
                {
                    for (var j = 0; j < h; ++j)
                    for (var i = 0; i < w; ++i)
                    {
                        var index = stride * j + i*3;
                        var b = pixels[index];
                        var g = pixels[index+1];
                        var r = pixels[index+2];
                        // 
                        var c = (byte)(0.21 * r + 0.72 * g + 0.07 * b);
                        //if (c > 192) 
                        //    c = 255;
                        //else 
                        //    c = 0;
                        pixels[index] = c;
                        pixels[index + 1] = c;
                        pixels[index + 2] = c;
                    }
                }
            );
        }

        static void Convolve(byte [] pixels, int w, int h, int stride, double[,] kernel)
        {
            var src = new byte[pixels.Length];
            Array.Copy(pixels, src, pixels.Length);

            var kw = kernel.GetLength(0);
            var kh = kernel.GetLength(1);

            for (var j = 0; j < h; ++j)
            for (var i = 0; i < w; ++i)
            {

                double rd = 0.0, gd = 0.0, bd = 0.0;
                for (var kj = 0; kj < kh; ++kj)
                for (var ki = 0; ki < kw; ++ki)
                {
                    // source coords
                    var si = (i + ki -kw/2 + w) % w;
                    var sj = (j + kj -kh/2 + h) % h;

                    var ind = sj * stride + si * 3;

                    var b = src[ind];
                    var g = src[ind + 1];
                    var r = src[ind + 2];

                    rd += r * kernel[ki, kj];
                    gd += g * kernel[ki, kj];
                    bd += b * kernel[ki, kj];
                }

                rd = Clamp(rd,0,255);
                gd = Clamp(gd, 0, 255);
                bd = Clamp(bd, 0, 255);


                    var index = stride * j + i * 3;
                pixels[index] = (byte)bd;
                pixels[index + 1] = (byte)gd;
                pixels[index + 2] = (byte)rd;
            }
        }



        static double Clamp(double c, double min, double max)
        {
            if (c < min) return min;
            if (max < c ) return max;
            return c;
        }

        /// <summary>
        /// Convert [0,1] to [0,1], pulling to 0 and 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        static double Spread(double x, double p)
        {
            if (x <= 0.5) return 0.5 * Math.Pow(2 * x, p);
            return 1 - 0.5 * Math.Pow(2*(1-x),p);
        }

        public static Bitmap Sharpen(Bitmap bmp)
        {
            return Process(bmp, (pixels, w, h, stride) =>
            {
                var kernel = new double[3, 3]
                {
                    {0,-1,0}, 
                    {-1,5,-1},
                    {0,-1,0}
                };
                Convolve(pixels, w, h, stride, kernel);
            });
        }

        /// <summary>
        /// Apply map to each channel, eacn in 0-1
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="stride"></param>
        /// <param name="map"></param>
        static void Apply(byte [] pixels, int w, int h, int stride, Func<double,double> map )
        {
            for (var j = 0; j < h; ++j)
            for (var i = 0; i < w; ++i)
            {
                var index = stride * j + i * 3;
                var b = pixels[index];
                var g = pixels[index + 1];
                var r = pixels[index + 2];
                
                pixels[index] = Do(b);
                pixels[index + 1] = Do(g);
                pixels[index + 2] = Do(r);
            }

            byte Do(byte v1)
            {
                var vv = v1 / 255.0;
                var vvv = map(vv);
                vvv = Clamp(vvv, 0, 1);
                return (byte) (255 * vvv);
            }

        }
        public static Bitmap Stretch(Bitmap bmp)
        {
            return Process(bmp, (pixels, w, h, stride) =>
            {
                var p = 2.5; // stretch amount
                Apply(pixels, w, h, stride, v => Spread(v, p));
            });
        }
        public static Bitmap Invert(Bitmap bmp)
        {
            return Process(bmp, (pixels, w, h, stride) =>
            {
                Apply(pixels, w, h, stride, v => 1.0-v);
            });
        }

        static (double, double, double) HSLtoRGB(double hue, double sat, double lum)
        {
            hue = Clamp(hue,0,1);
            if (hue == 1) hue = 0;
            var h = (int) (hue * 360);
            var c = (1 - Math.Abs(2 * lum - 1)) * sat;

            //Math.
            //(hue*6 mod 2)

            //var x = c * (1 - Math.Abs((h / 60) & 1) - 1);

            var x = c * (1 - Math.Abs( ((hue*6)%2) - 1));

            var m = lum-c/2;
            double r, g, b;
            if (h < 60)       (r, g, b) = (c, x, 0);
            else if (h < 120) (r, g, b) = (x, c, 0);
            else if (h < 180) (r, g, b) = (0, c, x);
            else if (h < 240) (r, g, b) = (0, x, c);
            else if (h < 300) (r, g, b) = (x, 0, c);
            else /*if (h < 300)*/ (r, g, b) = (c, 0, x);

            return (r,g,b);
        }


        static (byte,byte,byte) Colorize(byte r, byte g, byte b, double x, double y)
        {

            var hue = Math.Atan2(y - 0.5, x - 0.5); // -pi to pi
            hue = (hue + Math.PI) / (2 * Math.PI); //0-1
            var (rd, gd, bd) = HSLtoRGB(hue, 1.0, 0.5);

            //if (r + g + b < 64*3)
            //{ // blacks
            //    r = (byte) Clamp(rd * 255, 0, 255);
            //    g = (byte) Clamp(gd * 255, 0, 255);
            //    b = (byte) Clamp(bd * 255, 0, 255);
            //}
            //else
            //{ // whites
            //    r = 0;
            //    g = 0;
            //    b = 0;
            //}

            // use blackness of current color to blend between current and HSL
            byte Blend(double hsl, byte cur)
            {
                var r1 = cur / 255.0;
                var alpha = 1 - r1;
                hsl = alpha * hsl + (1 - alpha) * r1;
                return (byte)Clamp(hsl * 255, 0, 255);
            }

            r = Blend(rd, r);
            g = Blend(gd, g);
            b = Blend(bd, b);

            return (r, g, b);
        }

        public static Bitmap Colorize(Bitmap bmp)
        {
            return Process(bmp, (pixels, w, h, stride) =>
            {
                for (var j = 0; j < h; ++j)
                for (var i = 0; i < w; ++i)
                {
                    var index = stride * j + i * 3;
                    var b = pixels[index];
                    var g = pixels[index + 1];
                    var r = pixels[index + 2];

                    (r,g,b) = Colorize(r,g,b,(double)i/(w-1),(double)j/(h-1));

                    pixels[index] = b;
                    pixels[index + 1] = g;
                    pixels[index + 2] = r;
                }
            });

        }



#if false
        public static Bitmap Threshold(Bitmap bitmap)
        {

        }

        public static Bitmap Thicken(Bitmap bitmap)
        {

        }
        public static Bitmap Blur(Bitmap bitmap)
        {

        }
#endif
    }
}
