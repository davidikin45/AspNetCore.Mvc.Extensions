using DND.Common.Implementation.Repository.FileSystem.File;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class ImageHelperExtensions
    {
        //we init this once so that if the function is repeatedly called
        //it isn't stressing the garbage man
        private static Regex r = new Regex(":");

        public static string CaptionWithDate(this FileInfo fileinfo)
        {
            var caption = Caption(fileinfo);
            int num = 0;
            if (Int32.TryParse(caption, out num))
            {
                return fileinfo.MediaDateTaken().ToString("MMM yyyy");
            }
            else
            {
                return fileinfo.MediaDateTaken().ToString("MMM yyyy") + " - " + caption;
            }
        }

        public static string Caption(this FileInfo fileinfo)
        {
            return fileinfo.NameWithoutExtensionAndMain().GetStringWithSpacesAndFirstLetterUpper();
        }

        //retrieves the datetime WITHOUT loading the whole image
        public static DateTime MediaDateTaken(this FileInfo fileinfo)
        {
            if (fileinfo.IsImage())
            {
                using (FileStream fs = new FileStream(fileinfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    if (myImage.PropertyIdList.Contains(36867))
                    {
                        PropertyItem propItem = myImage.GetPropertyItem(36867);
                        if (propItem != null)
                        {
                            string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                            return DateTime.Parse(dateTaken);
                        }
                    }
                }
            }
            return fileinfo.LastWriteTimeUtc;
        }

        public static Double? Latitude(this FileInfo fileinfo)
        {
            using (FileStream fs = new FileStream(fileinfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try
                {
                    if (myImage.PropertyIdList.Contains(1) && myImage.PropertyIdList.Contains(2))
                    {
                        var latitudeRefQuery = myImage.GetPropertyItem(1);
                        var latitudeQuery = myImage.GetPropertyItem(2);

                        if (latitudeRefQuery != null && latitudeQuery != null)
                        {
                            string latitudeRef = System.Text.Encoding.ASCII.GetString(new byte[1] { latitudeRefQuery.Value[0] });
                            return ConvertCoordinate(latitudeQuery.Value, latitudeRef);
                        }
                    }

                    return null;
                }
                catch
                {
                    return null;
                }

            }
        }

        public static Double? Longitude(this FileInfo fileinfo)
        {
            using (FileStream fs = new FileStream(fileinfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try
                {
                    if (myImage.PropertyIdList.Contains(3) && myImage.PropertyIdList.Contains(4))
                    {
                        var longitudeRefQuery = myImage.GetPropertyItem(3);
                        var longitudeQuery = myImage.GetPropertyItem(4);

                        if (longitudeRefQuery != null && longitudeQuery != null)
                        {
                            string latitudeRef = System.Text.Encoding.ASCII.GetString(new byte[1] { longitudeRefQuery.Value[0] });
                            return ConvertCoordinate(longitudeQuery.Value, latitudeRef);
                        }

                    }
                    return null;
                }
                catch
                {
                    return null;
                }

            }
        }

        public static string PlaceId(this FileInfo fileinfo)
        {
            using (FileStream fs = new FileStream(fileinfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try
                {
                    if (myImage.PropertyIdList.Contains(40092))
                    {
                        var commentRefQuery = myImage.GetPropertyItem(40092);

                        if (commentRefQuery != null)
                        {
                            string commentRef = System.Text.Encoding.ASCII.GetString(commentRefQuery.Value).Replace("\0", "");
                            if (commentRef == null || commentRef.Contains("."))
                            {
                                return "";
                            }
                            return commentRef;
                        }
                    }

                    return "";
                }
                catch
                {
                    return "";
                }

            }
        }

        private static double ConvertCoordinate(byte[] bytes, string gpsRef)
        {
            if (bytes == null)
                return 0;

            int degrees;
            int minutes;
            double seconds;

            degrees = (int)splitLongAndDivide(bytes, 0);
            minutes = (int)splitLongAndDivide(bytes, 1);
            seconds = splitLongAndDivide(bytes, 2);

            double coordinate = (double)degrees + (minutes / 60.0) + (seconds / 3600.0);

            double roundedCoordinate = Math.Floor((double)(coordinate * 1000000.0)) / 1000000.0;
            if (gpsRef == "S" || gpsRef == "W")
                roundedCoordinate = 0 - roundedCoordinate;

            return roundedCoordinate;
        }

        private static double splitLongAndDivide(byte[] bytes, int num)
        {
            int int1 = BitConverter.ToInt32(bytes, 0 + (num * 8));
            int int2 = BitConverter.ToInt32(bytes, 4 + (num * 8));
            return ((double)int1 / (double)int2);
        }

        public static Boolean HasGPSCoordinates(this FileInfo fileinfo)
        {
            if (IsImage(fileinfo))
            {
                return fileinfo.Latitude().HasValue && fileinfo.Longitude().HasValue;
            }
            return false;
        }

        public static Boolean IsImage(this FileInfo fileinfo)
        {
            return MimeMapping.GetMimeMapping(fileinfo.Name).StartsWith("image/");
        }

        public static Boolean IsVideo(this FileInfo fileinfo)
        {
            return IsVideo(fileinfo.Name);
        }

        public static Boolean IsVideo(this string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ext == ".mp4")
                return true;

            return MimeMapping.GetMimeMapping(fileName).StartsWith("video/");
        }

        public static Boolean IsYouTube(this String id)
        {
            return id.Length == 11;
        }

        public static Boolean IsText(this FileInfo fileinfo)
        {
            return MimeMapping.GetMimeMapping(fileinfo.Name).StartsWith("text/");
        }

        public static FileInfo MainPhoto(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo != null)
            {
                var repository = new FileRepository(directoryInfo.FullName, true, "*.*", default(CancellationToken), ".jpg", ".jpeg");
                return repository.GetMain();
            }
            return null;
        }

        public static FileInfo MainPhotoOrVideo(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo != null)
            {
                var repository = new FileRepository(directoryInfo.FullName, true, "*.*", default(CancellationToken), ".jpg", ".jpeg", ".mp4", ".avi");
                return repository.GetMain();
            }
            return null;
        }

        public static FileInfo MainPhotoOrVideoOrText(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo != null)
            {
                var repository = new FileRepository(directoryInfo.FullName, true, "*.*", default(CancellationToken), ".jpg", ".jpeg", ".mp4", ".avi", ".txt");
                return repository.GetMain();
            }
            return null;
        }
    }

    public static class ImageHelper
    {
        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            ico,
            unknown
        }

        public enum CropOption
        {
            Centre,
            FromLeftOrTop,
            FromRightOrBottom,
            None
        }

        public enum Watermark
        {
            TopRight,
            TopLeft,
            BottomRight,
            BottomLeft,
            None
        }

        public class ImageFilterHelper
        {
            public enum Filters
            {
                None,
                GreyScale,
                BlackAndWhite,
                Sepia,
                Polaroid,
                RGBToBGR,
                Invert
            }

            public enum Rotate
            {
                None,
                Rotate90,
                Rotate180,
                Rotate270
            }

            public enum Shape
            {
                None,
                Circle
            }

            public static Bitmap ApplyFilter(Image sourceImage, ImageHelper.ImageFilterHelper.Filters filter)
            {
                switch (filter)
                {
                    case ImageHelper.ImageFilterHelper.Filters.None:
                        return (Bitmap)sourceImage;
                    case ImageHelper.ImageFilterHelper.Filters.GreyScale:
                        return ImageHelper.ImageFilterHelper.Grayscale(sourceImage);
                    case ImageHelper.ImageFilterHelper.Filters.BlackAndWhite:
                        return ImageHelper.ImageFilterHelper.BlackWhite(sourceImage);
                    case ImageHelper.ImageFilterHelper.Filters.Sepia:
                        return sourceImage.Sepia();
                    case ImageHelper.ImageFilterHelper.Filters.Polaroid:
                        return ImageHelper.ImageFilterHelper.Polaroid(sourceImage);
                    case ImageHelper.ImageFilterHelper.Filters.RGBToBGR:
                        return ImageHelper.ImageFilterHelper.RGBToBGR(sourceImage);
                    case ImageHelper.ImageFilterHelper.Filters.Invert:
                        return ImageHelper.ImageFilterHelper.Invert(sourceImage);
                    default:
                        return (Bitmap)sourceImage;
                }
            }

            public static Bitmap ApplyRotate(Image sourceImage, ImageHelper.ImageFilterHelper.Rotate filter)
            {
                switch (filter)
                {
                    case ImageHelper.ImageFilterHelper.Rotate.None:
                        return (Bitmap)sourceImage;
                    case ImageHelper.ImageFilterHelper.Rotate.Rotate90:
                        return ImageHelper.ImageFilterHelper.Rotate90(sourceImage);
                    case ImageHelper.ImageFilterHelper.Rotate.Rotate180:
                        return ImageHelper.ImageFilterHelper.Rotate180(sourceImage);
                    case ImageHelper.ImageFilterHelper.Rotate.Rotate270:
                        return ImageHelper.ImageFilterHelper.Rotate270(sourceImage);
                    default:
                        return (Bitmap)sourceImage;
                }
            }

            public static Bitmap ApplyShape(Image sourceImage, ImageHelper.ImageFilterHelper.Shape imageShape)
            {
                switch (imageShape)
                {
                    case ImageHelper.ImageFilterHelper.Shape.None:
                        return (Bitmap)sourceImage;
                    case ImageHelper.ImageFilterHelper.Shape.Circle:
                        return (Bitmap)ImageHelper.ImageFilterHelper.OvalImage(sourceImage);
                    default:
                        return (Bitmap)sourceImage;
                }
            }

            public static Bitmap Grayscale(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        0.3f,
                        0.3f,
                        0.3f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0.59f,
                        0.59f,
                        0.59f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0.11f,
                        0.11f,
                        0.11f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap BlackWhite(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        1.5f,
                        1.5f,
                        1.5f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        1.5f,
                        1.5f,
                        1.5f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        1.5f,
                        1.5f,
                        1.5f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        -1f,
                        -1f,
                        -1f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap SepiaTone(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        0.393f,
                        0.349f,
                        0.272f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0.769f,
                        0.686f,
                        0.534f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0.189f,
                        0.168f,
                        0.131f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap Polaroid(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        1.438f,
                        -0.062f,
                        -0.062f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        -0.122f,
                        1.378f,
                        -0.122f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        -0.016f,
                        -0.016f,
                        1.483f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        -0.03f,
                        0.05f,
                        -0.02f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap RGBToBGR(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        0f,
                        0f,
                        1f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        1f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        1f,
                        0f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap Invert(Image sourceImage)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        -1f,
                        0f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        -1f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        -1f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        1f,
                        1f,
                        1f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap Brightness(Image sourceImage, decimal adjustment)
            {
                if (decimal.Compare(adjustment, decimal.Zero) == 0)
                {
                    return (Bitmap)sourceImage;
                }
                float num = System.Convert.ToSingle(decimal.Divide(adjustment, new decimal(100L)));
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        1f,
                        0f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        1f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        1f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        num,
                        num,
                        num,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap Contrast(Image sourceImage, decimal adjustment)
            {
                if (decimal.Compare(adjustment, decimal.Zero) == 0)
                {
                    return (Bitmap)sourceImage;
                }
                float num = System.Convert.ToSingle(decimal.Add(decimal.One, decimal.Divide(adjustment, new decimal(100L))));
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        num,
                        0f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        num,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        num,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, decimal.One);
            }

            public static Bitmap Gamma(Image sourceImage, decimal adjustment)
            {
                if (decimal.Compare(adjustment, decimal.Zero) == 0)
                {
                    return (Bitmap)sourceImage;
                }
                adjustment = decimal.Add(decimal.One, decimal.Divide(adjustment, new decimal(100L)));
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[]
                    {
                        1f,
                        0f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        1f,
                        0f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        1f,
                        0f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new float[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });
                return ImageHelper.ImageFilterHelper.ApplyColorMatrix(sourceImage, colorMatrix, adjustment);
            }

            public static Bitmap Sharpen(Image image, double strengthInitial)
            {
                if (strengthInitial == 0.0)
                {
                    return (Bitmap)image;
                }
                double num = strengthInitial / 100.0;
                using (Bitmap bitmap = image as Bitmap)
                {
                    if (bitmap != null)
                    {
                        Bitmap bitmap2 = bitmap.Clone() as Bitmap;
                        int width = image.Width;
                        int height = image.Height;
                        double[,] array = new double[5, 5];
                        array[0, 0] = -1.0;
                        array[0, 1] = -1.0;
                        array[0, 2] = -1.0;
                        array[0, 3] = -1.0;
                        array[0, 4] = -1.0;
                        array[1, 0] = -1.0;
                        array[1, 1] = 2.0;
                        array[1, 2] = 2.0;
                        array[1, 3] = 2.0;
                        array[1, 4] = -1.0;
                        array[2, 0] = -1.0;
                        array[2, 1] = 2.0;
                        array[2, 2] = 16.0;
                        array[2, 3] = 2.0;
                        array[2, 4] = -1.0;
                        array[3, 0] = -1.0;
                        array[3, 1] = 2.0;
                        array[3, 2] = 2.0;
                        array[3, 3] = 2.0;
                        array[3, 4] = -1.0;
                        array[4, 0] = -1.0;
                        array[4, 1] = -1.0;
                        array[4, 2] = -1.0;
                        array[4, 3] = -1.0;
                        array[4, 4] = -1.0;
                        double[,] array2 = array;
                        double num2 = 1.0 - num;
                        double num3 = num / 16.0;
                        checked
                        {
                            Color[,] array3 = new Color[image.Width - 1 + 1, image.Height - 1 + 1];
                            if (bitmap2 != null)
                            {
                                Bitmap arg_26A_0 = bitmap2;
                                Rectangle rect = new Rectangle(0, 0, width, height);
                                System.Drawing.Imaging.BitmapData bitmapData = arg_26A_0.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                int num4 = bitmapData.Stride * height;
                                byte[] array4 = new byte[num4 - 1 + 1];
                                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, array4, 0, num4);
                                int arg_2A4_0 = 2;
                                int num5 = width - 2 - 1;
                                for (int i = arg_2A4_0; i <= num5; i++)
                                {
                                    int arg_2B4_0 = 2;
                                    int num6 = height - 2 - 1;
                                    for (int j = arg_2B4_0; j <= num6; j++)
                                    {
                                        double num7 = 0.0;
                                        double num8 = 0.0;
                                        double num9 = 0.0;
                                        int num10 = 0;
                                        do
                                        {
                                            int num11 = 0;
                                            int num14;
                                            do
                                            {
                                                int num12 = (i - 2 + num10 + width) % width;
                                                int num13 = (j - 2 + num11 + height) % height;
                                                num14 = num13 * bitmapData.Stride + 3 * num12;
                                                unchecked
                                                {
                                                    num7 += (double)array4[checked(num14 + 2)] * array2[num10, num11];
                                                    num8 += (double)array4[checked(num14 + 1)] * array2[num10, num11];
                                                    num9 += (double)array4[checked(num14 + 0)] * array2[num10, num11];
                                                }
                                                num11++;
                                            }
                                            while (num11 <= 4);
                                            num14 = j * bitmapData.Stride + 3 * i;
                                            int red = System.Math.Min(System.Math.Max((int)System.Math.Round(unchecked(num3 * num7 + num2 * (double)array4[checked(num14 + 2)])), 0), 255);
                                            int green = System.Math.Min(System.Math.Max((int)System.Math.Round(unchecked(num3 * num8 + num2 * (double)array4[checked(num14 + 1)])), 0), 255);
                                            int blue = System.Math.Min(System.Math.Max((int)System.Math.Round(unchecked(num3 * num9 + num2 * (double)array4[checked(num14 + 0)])), 0), 255);
                                            array3[i, j] = System.Drawing.Color.FromArgb(red, green, blue);
                                            num10++;
                                        }
                                        while (num10 <= 4);
                                    }
                                }
                                int arg_43E_0 = 2;
                                int num15 = width - 2 - 1;
                                for (int k = arg_43E_0; k <= num15; k++)
                                {
                                    int arg_44B_0 = 2;
                                    int num16 = height - 2 - 1;
                                    for (int l = arg_44B_0; l <= num16; l++)
                                    {
                                        int num14 = l * bitmapData.Stride + 3 * k;
                                        array4[num14 + 2] = array3[k, l].R;
                                        array4[num14 + 1] = array3[k, l].G;
                                        array4[num14 + 0] = array3[k, l].B;
                                    }
                                }
                                System.Runtime.InteropServices.Marshal.Copy(array4, 0, bitmapData.Scan0, num4);
                                bitmap2.UnlockBits(bitmapData);
                            }
                            return bitmap2;
                        }
                    }
                }
                return null;
            }

            private static Bitmap ApplyColorMatrix(Image sourceImage, ColorMatrix colorMatrix, decimal gamma = default(decimal))
            {
                Bitmap bitmap = new Bitmap(sourceImage.Width, sourceImage.Height);
                using (Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    ImageAttributes imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    imageAttributes.SetGamma(System.Convert.ToSingle(gamma), ColorAdjustType.Bitmap);
                    Graphics arg_65_0 = graphics;
                    Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                    arg_65_0.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, System.Drawing.GraphicsUnit.Pixel, imageAttributes);
                }
                sourceImage.Dispose();
                return bitmap;
            }

            public static Bitmap Rotate90(Image sourceImage)
            {
                sourceImage.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                return (Bitmap)sourceImage;
            }

            public static Bitmap Rotate180(Image sourceImage)
            {
                sourceImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                return (Bitmap)sourceImage;
            }

            public static Bitmap Rotate270(Image sourceImage)
            {
                sourceImage.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
                return (Bitmap)sourceImage;
            }

            public static Image OvalImage(Image img)
            {
                Bitmap bitmap = new Bitmap(img.Width, img.Height);
                bitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                using (System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    using (System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        using (Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            graphics.DrawImage(img, System.Drawing.Point.Empty);
                            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                            graphicsPath.AddEllipse(0, 0, img.Width, img.Height);
                            graphicsPath.AddRectangle(rect);
                            graphics.FillPath(solidBrush, graphicsPath);
                        }
                    }
                }
                return bitmap;
            }
        }

        public static Bitmap Sepia(this Image sourceImage)
        {
            return ImageHelper.ImageFilterHelper.SepiaTone(sourceImage);
        }

        public static Bitmap Brightness(this Image sourceImage, decimal adjustment)
        {
            return ImageHelper.ImageFilterHelper.Brightness(sourceImage, adjustment);
        }

        public static Bitmap Contrast(this Image sourceImage, decimal adjustment)
        {
            return ImageHelper.ImageFilterHelper.Contrast(sourceImage, adjustment);
        }

        public static Bitmap Sharpen(this Image sourceImage, double strengthInitial)
        {
            return ImageHelper.ImageFilterHelper.Sharpen(sourceImage, strengthInitial);
        }

        public static Bitmap Gamma(this Image sourceImage, decimal adjustment)
        {
            return ImageHelper.ImageFilterHelper.Gamma(sourceImage, adjustment);
        }

        public static bool IsValidImage(string filePath)
        {
            return System.IO.File.Exists(filePath) && ImageHelper.IsValidImage(FileHelper.GetBytes(filePath));
        }

        public static bool IsValidImage(byte[] bytes)
        {
            return ImageHelper.GetImageFormat(bytes) != ImageHelper.ImageFormat.unknown;
        }

        public static ImageHelper.ImageFormat GetImageFormat(byte[] bytes)
        {
            byte[] bytes2 = System.Text.Encoding.ASCII.GetBytes("BM");
            byte[] bytes3 = System.Text.Encoding.ASCII.GetBytes("GIF");
            byte[] array = new byte[]
            {
                137,
                80,
                78,
                71
            };
            byte[] array2 = new byte[]
            {
                73,
                73,
                42
            };
            byte[] array3 = new byte[]
            {
                77,
                77,
                42
            };
            byte[] array4 = new byte[]
            {
                255,
                216,
                255,
                224
            };
            byte[] array5 = new byte[]
            {
                255,
                216,
                255,
                225
            };
            byte[] array6 = new byte[]
            {
                255,
                216,
                255,
                219
            };
            byte[] array7 = new byte[]
            {
                255,
                216,
                255,
                237
            };
            byte[] array8 = new byte[]
            {
                255,
                216,
                255,
                226
            };
            byte[] array9 = new byte[]
            {
                0,
                0,
                1,
                0,
                2,
                0,
                16,
                16
            };
            if (array4.SequenceEqual(bytes.Take(array4.Length)))
            {
                return ImageHelper.ImageFormat.jpeg;
            }
            if (array5.SequenceEqual(bytes.Take(array5.Length)))
            {
                return ImageHelper.ImageFormat.jpeg;
            }
            if (array6.SequenceEqual(bytes.Take(array6.Length)))
            {
                return ImageHelper.ImageFormat.jpeg;
            }
            if (array7.SequenceEqual(bytes.Take(array7.Length)))
            {
                return ImageHelper.ImageFormat.jpeg;
            }
            if (array8.SequenceEqual(bytes.Take(array8.Length)))
            {
                return ImageHelper.ImageFormat.jpeg;
            }
            if (array.SequenceEqual(bytes.Take(array.Length)))
            {
                return ImageHelper.ImageFormat.png;
            }
            if (array2.SequenceEqual(bytes.Take(array2.Length)))
            {
                return ImageHelper.ImageFormat.tiff;
            }
            if (array3.SequenceEqual(bytes.Take(array3.Length)))
            {
                return ImageHelper.ImageFormat.tiff;
            }
            if (array9.SequenceEqual(bytes.Take(array9.Length)))
            {
                return ImageHelper.ImageFormat.ico;
            }
            if (bytes2.SequenceEqual(bytes.Take(bytes2.Length)))
            {
                return ImageHelper.ImageFormat.bmp;
            }
            if (bytes3.SequenceEqual(bytes.Take(bytes3.Length)))
            {
                return ImageHelper.ImageFormat.gif;
            }
            return ImageHelper.ImageFormat.unknown;
        }

        public static Image GetImage(byte[] bytes)
        {
            return ImageHelper.byteArrayToImage(bytes);
        }

        public static Image GetImage(string directory, string filename)
        {
            return ImageHelper.GetImage(directory + filename);
        }

        public static Image GetImage(string imagePath)
        {
            byte[] bytes = FileHelper.GetBytes(imagePath);
            Image result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
            {
                Image image = Image.FromStream(memoryStream);
                result = image;
            }
            return result;
        }

        public static byte[] GetImageAsBytes(string imagePath)
        {
            return ImageHelper.imageToJPEGByteArray(ImageHelper.GetImage(imagePath), 75L);
        }

        public static Image ResizeImageWidthHeightConstraint(Image image, int widthHeightConstraint, bool cropImage = false, ImageHelper.CropOption cropOptions = ImageHelper.CropOption.Centre)
        {
            Size size = new Size(widthHeightConstraint, widthHeightConstraint);
            return ImageHelper.ResizeImage(image, size, true, cropImage, cropOptions);
        }

        public static Image ResizeImageMaxWidthHeightConstraint(Image image, int maxWidthConstraint, int maxHeightConstraint, bool cropImage = false, ImageHelper.CropOption cropOptions = ImageHelper.CropOption.Centre)
        {
            int width = 0;
            int height = 0;
            decimal num = decimal.One;
            if (maxWidthConstraint != 0)
            {
                num = new decimal((double)maxWidthConstraint / (double)image.Width);
            }
            decimal d = decimal.One;
            if (maxHeightConstraint != 0)
            {
                d = new decimal((double)maxHeightConstraint / (double)image.Height);
            }
            if (decimal.Compare(d, decimal.One) < 0 && decimal.Compare(num, decimal.One) < 0)
            {
                if (decimal.Compare(d, num) < 0)
                {
                    height = maxHeightConstraint;
                }
                else
                {
                    width = maxWidthConstraint;
                }
            }
            else if (decimal.Compare(num, decimal.One) < 0)
            {
                width = maxWidthConstraint;
            }
            else
            {
                if (decimal.Compare(d, decimal.One) >= 0)
                {
                    return image;
                }
                height = maxHeightConstraint;
            }
            Size size = new Size(width, height);
            return ImageHelper.ResizeImage(image, size, true, cropImage, cropOptions);
        }

        public static Image ResizeImageWidthHeightConstraint(Image image, int widthConstraint, int heightConstraint, bool cropImage = false, ImageHelper.CropOption cropOptions = ImageHelper.CropOption.Centre)
        {
            Size size = new Size(widthConstraint, heightConstraint);
            return ImageHelper.ResizeImage(image, size, true, cropImage, cropOptions);
        }

        public static Image ResizeImageWidthConstraint(Image image, int widthConstraint)
        {
            Size size = new Size(widthConstraint, 0);
            return ImageHelper.ResizeImage(image, size, true, false, ImageHelper.CropOption.Centre);
        }

        public static Image ResizeImageHeightConstraint(Image image, int heightConstraint)
        {
            Size size = new Size(0, heightConstraint);
            return ImageHelper.ResizeImage(image, size, true, false, ImageHelper.CropOption.Centre);
        }

        public static Image ResizeImageAbsolute(Image image, int width, int height)
        {
            Size size = new Size(width, height);
            return ImageHelper.ResizeImage(image, size, false, false, ImageHelper.CropOption.Centre);
        }

        public static Image ResizeImage(string imagePath, int width, int height, bool preserveAspectRatio = true)
        {
            Size size = new Size(width, height);
            return ImageHelper.ResizeImage(imagePath, size, preserveAspectRatio);
        }

        public static Image ResizeImage(string imagePath, Size size, bool preserveAspectRatio = true)
        {
            Image image = ImageHelper.GetImage(imagePath);
            return ImageHelper.ResizeImage(image, size, preserveAspectRatio, false, ImageHelper.CropOption.Centre);
        }

        public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true, bool cropImage = false, ImageHelper.CropOption cropOption = ImageHelper.CropOption.Centre)
        {
            int width = image.Width;
            int height = image.Height;
            int height2 = size.Height;
            int width2 = size.Width;
            int x = 0;
            int y = 0;
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            double num5 = 0.0;
            if (width2 != 0)
            {
                num4 = (double)width2 / (double)width;
            }
            if (height2 != 0)
            {
                num5 = (double)height2 / (double)height;
            }
            checked
            {
                int num6;
                int num7;
                if (preserveAspectRatio)
                {
                    if (!cropImage)
                    {
                        if (height2 != 0 && width2 != 0)
                        {
                            num3 = System.Math.Min(num5, num4);
                        }
                        else if (size.Height == 0)
                        {
                            num3 = num4;
                        }
                        else
                        {
                            num3 = num5;
                        }
                    }
                    else if (num5 < 1.0 || num4 < 1.0)
                    {
                        unchecked
                        {
                            if (num5 < 1.0 && num4 < 1.0)
                            {
                                num3 = System.Math.Max(num5, num4);
                                num2 = ((double)height2 - (double)height * num3) / 2.0;
                                num = ((double)width2 - (double)width * num3) / 2.0;
                            }
                            else if (num5 < 1.0)
                            {
                                num3 = num5;
                                if (width2 != 0 && height2 != 0)
                                {
                                    num2 = ((double)height2 - (double)height * num3) / 2.0;
                                    num = ((double)width2 - (double)width * num3) / 2.0;
                                }
                            }
                            else if (num4 < 1.0)
                            {
                                num3 = num4;
                                if (width2 != 0 && height2 != 0)
                                {
                                    num2 = ((double)height2 - (double)height * num3) / 2.0;
                                    num = ((double)width2 - (double)width * num3) / 2.0;
                                }
                            }
                        }
                    }
                    else
                    {
                        num3 = 1.0;
                        num = (double)(width2 - width) / 2.0;
                        num2 = (double)(height2 - height) / 2.0;
                    }
                    if (num3 > 1.0)
                    {
                        num3 = 1.0;
                    }
                    num6 = (int)System.Math.Round(System.Math.Round(unchecked((double)width * num3)));
                    num7 = (int)System.Math.Round(System.Math.Round(unchecked((double)height * num3)));
                }
                else
                {
                    num6 = width2;
                    num7 = height2;
                }
                Bitmap bitmap = null;

                bitmap = new Bitmap(num6 + (int)System.Math.Round(System.Math.Round(unchecked(2.0 * num))), num7 + (int)System.Math.Round(System.Math.Round(unchecked(2.0 * num2))));
                bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                Image result;
                using (Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.Clear(System.Drawing.Color.White);
                    Rectangle destRect = new Rectangle();
                    if (cropOption == ImageHelper.CropOption.Centre)
                    {
                        destRect = new Rectangle((int)System.Math.Round(System.Math.Round(num)), (int)System.Math.Round(System.Math.Round(num2)), num6, num7);
                    }
                    else if (cropOption == ImageHelper.CropOption.FromLeftOrTop)
                    {
                        destRect = new Rectangle(0, 0, num6, num7);
                    }
                    else if (cropOption == ImageHelper.CropOption.FromRightOrBottom)
                    {
                        destRect = new Rectangle((int)System.Math.Round(System.Math.Round(unchecked(2.0 * num))), (int)System.Math.Round(System.Math.Round(unchecked(2.0 * num2))), num6, num7);
                    }
                    Rectangle srcRect = new Rectangle(x, y, width, height);
                    graphics.DrawImage(image, destRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                    result = bitmap;
                }
                return result;
            }
        }

        public static void SaveImageAsJPEG(string directory, string fileName, Image image)
        {
            ImageHelper.SaveImageAsJPEG(directory + fileName, image);
        }

        public static void SaveImageAsJPEG(string path, Image image)
        {
            string extension = System.IO.Path.GetExtension(path);
            path = path.Replace(extension, ".jpg");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static void SaveImageAsPNG(string directory, string fileName, Image image)
        {
            ImageHelper.SaveImageAsPNG(directory + fileName, image);
        }

        public static void SaveImageAsPNG(string path, Image image)
        {
            string extension = System.IO.Path.GetExtension(path);
            path = path.Replace(extension, ".png");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void SaveImageAsGIF(string directory, string fileName, Image image)
        {
            ImageHelper.SaveImageAsGIF(directory + fileName, image);
        }

        public static void SaveImageAsGIF(string path, Image image)
        {
            string extension = System.IO.Path.GetExtension(path);
            path = path.Replace(extension, ".gif");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Gif);
        }

        public static void SaveImageAsICON(string directory, string fileName, Image image)
        {
            ImageHelper.SaveImageAsICON(directory + fileName, image);
        }

        public static void SaveImageAsICON(string path, Image image)
        {
            string extension = System.IO.Path.GetExtension(path);
            path = path.Replace(extension, ".ico");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Icon);
        }

        public static void SaveImageAsTIFF(string directory, string fileName, Image image)
        {
            ImageHelper.SaveImageAsTIFF(directory + fileName, image);
        }

        public static void SaveImageAsTIFF(string path, Image image)
        {
            string extension = System.IO.Path.GetExtension(path);
            path = path.Replace(extension, ".tiff");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Tiff);
        }

        public static Image WatermarkImageWithTextFromTL(string watermarkText, System.Drawing.Font font, Color colour, int alpha, int fromLeft, int fromTop, Image image)
        {
            return ImageHelper.WatermarkImageWithText(watermarkText, font, colour, alpha, fromLeft, fromTop, image);
        }

        public static Image WatermarkImageWithTextFromTR(string watermarkText, System.Drawing.Font font, Color colour, int alpha, int fromRight, int fromTop, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            System.Drawing.SizeF sizeF = default(System.Drawing.SizeF);
            sizeF = graphics.MeasureString(watermarkText, font);
            return ImageHelper.WatermarkImageWithText(watermarkText, font, colour, alpha, checked(image.Size.Width - (int)System.Math.Round((double)sizeF.Width) - fromRight), fromTop, image);
        }

        public static Image WatermarkImageWithTextFromBL(string watermarkText, System.Drawing.Font font, Color colour, int alpha, int fromLeft, int fromBottom, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            System.Drawing.SizeF sizeF = default(System.Drawing.SizeF);
            sizeF = graphics.MeasureString(watermarkText, font);
            return ImageHelper.WatermarkImageWithText(watermarkText, font, colour, alpha, fromLeft, checked(image.Size.Height - (int)System.Math.Round((double)sizeF.Height) - fromBottom), image);
        }

        public static Image WatermarkImageWithTextFromBR(string watermarkText, System.Drawing.Font font, Color colour, int alpha, int fromRight, int fromBottom, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            System.Drawing.SizeF sizeF = default(System.Drawing.SizeF);
            sizeF = graphics.MeasureString(watermarkText, font);
            return checked(ImageHelper.WatermarkImageWithText(watermarkText, font, colour, alpha, image.Size.Width - (int)System.Math.Round((double)sizeF.Width) - fromRight, image.Size.Height - (int)System.Math.Round((double)sizeF.Height) - fromBottom, image));
        }

        public static Image WatermarkImageWithTextMiddle(string watermarkText, System.Drawing.Font font, Color colour, int alpha, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            System.Drawing.SizeF sizeF = default(System.Drawing.SizeF);
            sizeF = graphics.MeasureString(watermarkText, font);
            return checked(ImageHelper.WatermarkImageWithText(watermarkText, font, colour, alpha, (int)System.Math.Round((double)image.Size.Width / 2.0) - (int)System.Math.Round((double)(sizeF.Width / 2f)), (int)System.Math.Round((double)image.Size.Height / 2.0) - (int)System.Math.Round((double)(sizeF.Height / 2f)), image));
        }

        public static Image WatermarkImageWithText(string watermarkText, System.Drawing.Font font, Color colour, int alpha, int X, int Y, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            System.Drawing.SolidBrush solidBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(alpha, colour));
            Graphics arg_31_0 = graphics;
            System.Drawing.Brush arg_31_3 = solidBrush;
            System.Drawing.Point p = new System.Drawing.Point(X, Y);
            arg_31_0.DrawString(watermarkText, font, arg_31_3, p);
            return image;
        }

        public static Bitmap ApplyWatermark(Image image, ImageHelper.Watermark location, int minHeight, int minWidth, int topBottomPadding, int leftRightPadding, string watermarkPath, ref bool applied)
        {
            if (image.Height > minHeight || image.Width > minWidth)
            {
                return ApplyWatermark(image, location, topBottomPadding, leftRightPadding, watermarkPath, ref applied);
            }
            return (Bitmap)image;
        }

        public static Bitmap ApplyWatermark(Image image, ImageHelper.Watermark location, int topBottomPadding, int leftRightPadding, string watermarkPath, ref bool applied)
        {
            switch (location)
            {
                case ImageHelper.Watermark.TopRight:
                    {
                        applied = true;
                        using (Image image2 = ImageHelper.GetImage(""))
                        {
                            return (Bitmap)ImageHelper.WatermarkImageWithLogoFromTR(image2, 255, leftRightPadding, topBottomPadding, image);
                        }
                    }
                case ImageHelper.Watermark.TopLeft:
                    {
                        applied = true;
                        using (Image image3 = ImageHelper.GetImage(""))
                        {
                            return (Bitmap)ImageHelper.WatermarkImageWithLogoFromTL(image3, 255, leftRightPadding, topBottomPadding, image);
                        }
                    }
                case ImageHelper.Watermark.BottomRight:
                    {
                        applied = true;
                        using (Image image4 = ImageHelper.GetImage(""))
                        {
                            return (Bitmap)ImageHelper.WatermarkImageWithLogoFromBR(image4, 255, leftRightPadding, topBottomPadding, image);
                        }
                    }
                case ImageHelper.Watermark.BottomLeft:
                    {
                        applied = true;
                        using (Image image5 = ImageHelper.GetImage(""))
                        {
                            return (Bitmap)ImageHelper.WatermarkImageWithLogoFromBL(image5, 255, leftRightPadding, topBottomPadding, image);
                        }
                    }
                case ImageHelper.Watermark.None:
                    return (Bitmap)image;
            }

            return (Bitmap)image;
        }

        public static Image WatermarkImageWithLogoFromTL(Image logo, int alpha, int fromLeft, int fromTop, Image image)
        {
            return ImageHelper.WatermarkImageWithLogo(logo, alpha, fromLeft, fromTop, image);
        }

        public static Image WatermarkImageWithLogoFromTR(Image logo, int alpha, int fromRight, int fromTop, Image image)
        {
            return ImageHelper.WatermarkImageWithLogo(logo, alpha, checked(image.Size.Width - logo.Width - fromRight), fromTop, image);
        }

        public static Image WatermarkImageWithLogoFromBL(Image logo, int alpha, int fromLeft, int fromBottom, Image image)
        {
            return ImageHelper.WatermarkImageWithLogo(logo, alpha, fromLeft, checked(image.Size.Height - logo.Height - fromBottom), image);
        }

        public static Image WatermarkImageWithLogoFromBR(Image logo, int alpha, int fromRight, int fromBottom, Image image)
        {
            return checked(ImageHelper.WatermarkImageWithLogo(logo, alpha, image.Size.Width - logo.Width - fromRight, image.Size.Height - logo.Height - fromBottom, image));
        }

        public static Image WatermarkImageWithLogo(Image logo, int alpha, int X, int Y, Image image)
        {
            Graphics graphics = System.Drawing.Graphics.FromImage(image);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            int width = logo.Width;
            int width2 = logo.Width;
            ImageAttributes imageAttributes = new ImageAttributes();
            System.Drawing.Imaging.ColorMap[] map = new System.Drawing.Imaging.ColorMap[]
            {
                new System.Drawing.Imaging.ColorMap
                {
                    OldColor = System.Drawing.Color.FromArgb(255, 0, 255, 0),
                    NewColor = System.Drawing.Color.FromArgb(0, 0, 0, 0)
                }
            };
            imageAttributes.SetRemapTable(map, ColorAdjustType.Bitmap);
            float num = (float)((double)alpha / 255.0);
            float[][] newColorMatrix = new float[][]
            {
                new float[]
                {
                    1f,
                    0f,
                    0f,
                    0f,
                    0f
                },
                new float[]
                {
                    0f,
                    1f,
                    0f,
                    0f,
                    0f
                },
                new float[]
                {
                    0f,
                    0f,
                    1f,
                    0f,
                    0f
                },
                new float[]
                {
                    0f,
                    0f,
                    0f,
                    num,
                    0f
                },
                new float[]
                {
                    0f,
                    0f,
                    0f,
                    0f,
                    1f
                }
            };
            ColorMatrix newColorMatrix2 = new ColorMatrix(newColorMatrix);
            imageAttributes.SetColorMatrix(newColorMatrix2, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Graphics arg_1D4_0 = graphics;
            Rectangle destRect = new Rectangle(X, Y, width, width2);
            arg_1D4_0.DrawImage(logo, destRect, 0, 0, width, width2, System.Drawing.GraphicsUnit.Pixel, imageAttributes);
            return image;
        }

        public static Image AddBorder(Image original, int borderWidth, Color borderColor, bool expandForBorder = false)
        {
            checked
            {
                Pen pen = new Pen(borderColor, (float)(borderWidth * 2));
                Size size;
                if (expandForBorder)
                {
                    size = new Size(original.Width + borderWidth * 2, original.Height + borderWidth * 2);
                }
                else
                {
                    size = new Size(original.Width, original.Height);
                }
                Bitmap bitmap = new Bitmap(size.Width, size.Height);
                Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
                if (expandForBorder)
                {
                    Graphics arg_69_0 = graphics;
                    System.Drawing.Point point = new System.Drawing.Point(borderWidth, borderWidth);
                    arg_69_0.DrawImage(original, point);
                }
                else
                {
                    Graphics arg_7D_0 = graphics;
                    System.Drawing.Point point = new System.Drawing.Point(0, 0);
                    arg_7D_0.DrawImage(original, point);
                }
                graphics.DrawRectangle(pen, 0, 0, size.Width, size.Height);
                graphics.Dispose();
                return bitmap;
            }
        }

        public static Image CreateButton(Color color1, Color color2, System.Drawing.Drawing2D.LinearGradientMode gradient, string text, System.Drawing.Font textFont, Color textColour, int borderWidth, Color borderColour, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1, color2, gradient);
            graphics.FillRectangle(brush, 0, 0, width, height);
            if (text != "")
            {
                bitmap = (Bitmap)ImageHelper.WatermarkImageWithTextMiddle(text, textFont, textColour, 255, bitmap);
            }
            if (borderWidth != 0)
            {
                bitmap = (Bitmap)ImageHelper.AddBorder(bitmap, borderWidth, borderColour, false);
            }
            return bitmap;
        }

        public static Image RotateImageACW90(Image img)
        {
            img.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
            return img;
        }

        public static Image RotateImageCW90(Image img)
        {
            img.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
            return img;
        }

        public static Image RotateImage180(Image img)
        {
            img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            return img;
        }

        public static Image MirrorX(Image img)
        {
            img.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
            return img;
        }

        public static Image MirrorY(Image img)
        {
            img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
            return img;
        }

        public static string GetImageLatitude(Image image)
        {
            string result = "";
            string text = "";
            ImageHelper.GetImageLocation(image, ref result, ref text);
            return result;
        }

        public static string GetImageLongitude(Image image)
        {
            string text = "";
            string result = "";
            ImageHelper.GetImageLocation(image, ref text, ref result);
            return result;
        }

        public static void GetImageLocation(Image image, ref string latitudeString, ref string longitudeString)
        {
            double? num = null;
            double? num2 = null;
            ImageHelper.GetImageLocation(image, ref num, ref num2);
            if (num2.HasValue)
            {
                longitudeString = num2.ToString();
            }
            if (num.HasValue)
            {
                latitudeString = num.ToString();
            }
        }

        public static void GetImageLocation(Image image, ref double? latitude, ref double? longitude)
        {
            System.Drawing.Imaging.PropertyItem[] propertyItems = image.PropertyItems;
            checked
            {
                for (int i = 0; i < propertyItems.Length; i++)
                {
                    System.Drawing.Imaging.PropertyItem propertyItem = propertyItems[i];
                    short type = propertyItem.Type;
                    if (type == 5)
                    {
                        if (propertyItem.Id == 2)
                        {
                            latitude = ImageHelper.GetLatitudeAndLongitude(propertyItem);
                        }
                        if (propertyItem.Id == 4)
                        {
                            longitude = ImageHelper.GetLatitudeAndLongitude(propertyItem);
                        }
                    }
                }
            }
        }

        private static double? GetLatitudeAndLongitude(System.Drawing.Imaging.PropertyItem propItem)
        {
            double? result;
            try
            {
                uint value = System.BitConverter.ToUInt32(propItem.Value, 0);
                uint value2 = System.BitConverter.ToUInt32(propItem.Value, 4);
                uint value3 = System.BitConverter.ToUInt32(propItem.Value, 8);
                uint value4 = System.BitConverter.ToUInt32(propItem.Value, 12);
                uint value5 = System.BitConverter.ToUInt32(propItem.Value, 16);
                uint value6 = System.BitConverter.ToUInt32(propItem.Value, 20);
                result = System.Convert.ToDouble(value) / System.Convert.ToDouble(value2) + System.Convert.ToDouble(System.Convert.ToDouble(value3) / System.Convert.ToDouble(value4)) / 60.0 + System.Convert.ToDouble(System.Convert.ToDouble(value5) / System.Convert.ToDouble(value6) / 3600.0);
            }
            catch (Exception)
            {
                double? num = null;
                result = num;
            }
            return result;
        }

        private const int OrientationKey = 0x0112;
        private const int NotSpecified = 0;
        private const int NormalOrientation = 1;
        private const int MirrorHorizontal = 2;
        private const int UpsideDown = 3;
        private const int MirrorVertical = 4;
        private const int MirrorHorizontalAndRotateRight = 5;
        private const int RotateLeft = 6;
        private const int MirorHorizontalAndRotateLeft = 7;
        private const int RotateRight = 8;

        public static Image byteArrayToImage(byte[] data)
        {
            Image result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(data))
            {
                using (Image image = Image.FromStream(memoryStream))
                {
                    if (image.PropertyIdList.Contains(OrientationKey))
                    {
                        var orientation = (int)image.GetPropertyItem(OrientationKey).Value[0];
                        switch (orientation)
                        {
                            case NotSpecified: // Assume it is good.
                            case NormalOrientation:
                                // No rotation required.
                                break;
                            case MirrorHorizontal:
                                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case UpsideDown:
                                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            case MirrorVertical:
                                image.RotateFlip(RotateFlipType.Rotate180FlipX);
                                break;
                            case MirrorHorizontalAndRotateRight:
                                image.RotateFlip(RotateFlipType.Rotate90FlipX);
                                break;
                            case RotateLeft:
                                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case MirorHorizontalAndRotateLeft:
                                image.RotateFlip(RotateFlipType.Rotate270FlipX);
                                break;
                            case RotateRight:
                                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            default:
                                throw new NotImplementedException("An orientation of " + orientation + " isn't implemented.");
                        }
                    }
                    result = new Bitmap(image);
                }
            }
            return result;
        }

        public static byte[] imageToRawByteArray(Image image)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                image.Save(memoryStream, image.RawFormat);
                result = memoryStream.ToArray();
            }
            return result;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] imageEncoders = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            return imageEncoders.FirstOrDefault((ImageCodecInfo t) => t.MimeType == mimeType);
        }

        public static byte[] imageToJPEGByteArray(Image image, long compression = 75L)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                System.Drawing.Imaging.EncoderParameters encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);
                ImageCodecInfo encoderInfo = ImageHelper.GetEncoderInfo("image/jpeg");
                image.Save(memoryStream, encoderInfo, encoderParameters);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static byte[] imageToGIFByteArray(Image image)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Gif);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static byte[] imageToPNGByteArray(Image image)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static byte[] imageToICONByteArray(Image image)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Icon);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static byte[] imageToTIFFByteArray(Image image)
        {
            byte[] result;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static string CreateThumbnailBase64(byte[] PassedImage, int LargestSide)
        {
            return System.Convert.ToBase64String(ImageHelper.CreateThumbnailBytes(PassedImage, LargestSide));
        }

        public static byte[] CreateThumbnailBytes(byte[] PassedImage, int LargestSide)
        {
            checked
            {
                byte[] result;
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    using (System.IO.MemoryStream memoryStream2 = new System.IO.MemoryStream())
                    {
                        memoryStream.Write(PassedImage, 0, PassedImage.Length);
                        Bitmap bitmap = new Bitmap(memoryStream);
                        int num;
                        int num3;
                        if (bitmap.Height > bitmap.Width)
                        {
                            num = LargestSide;
                            double num2 = (double)LargestSide / (double)bitmap.Height;
                            num3 = (int)System.Math.Round(unchecked(num2 * (double)bitmap.Width));
                        }
                        else
                        {
                            num3 = LargestSide;
                            double num2 = (double)LargestSide / (double)bitmap.Width;
                            num = (int)System.Math.Round(unchecked(num2 * (double)bitmap.Height));
                        }
                        Bitmap bitmap2 = new Bitmap(num3, num);
                        bitmap2 = (Bitmap)ImageHelper.ResizeImageWidthHeightConstraint(bitmap, num3, num, false, ImageHelper.CropOption.Centre);
                        bitmap2.Save(memoryStream2, System.Drawing.Imaging.ImageFormat.Jpeg);
                        result = memoryStream2.ToArray();
                    }
                }
                return result;
            }
        }
    }
}
