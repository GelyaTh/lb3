using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Lab3_AOCI
{
    class Photoshop
    {   

        private Image<Bgr, byte> srcImg;
        public Photoshop(Image<Bgr, byte> srcImg)
        {
            this.srcImg = srcImg;
        }

        public Image<Bgr, byte> GetSourceImage()
        {
            return srcImg;
        }

        public Image<Bgr, byte> GetBilinearInterpolatedImage()
        {
            return GetScaledImage(1, 1);
        }

        public Image<Bgr, byte> GetScaledImage(double kx, double ky)
        {         
            Image<Bgr, byte> scaledImg = new Image<Bgr, byte>((int)(srcImg.Width * kx), (int)(srcImg.Height * ky));

            for (int i = 0; i < scaledImg.Width - 1; i++)
                for (int j = 0; j < scaledImg.Height - 1; j++)
                {
                    double I = (i / kx);
                    double J = (j / ky);

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);

                    if (baseI >= srcImg.Width - 1) continue;
                    if (baseJ >= srcImg.Height - 1) continue;

                    double rI = I - baseI;
                    double rJ = J - baseJ;

                    double irI = 1 - rI;
                    double irJ = 1 - rJ;

                    Bgr c1 = new Bgr();
                    c1.Blue = srcImg.Data[(int)baseJ, (int)baseI, 0] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 0] * rI;
                    c1.Green = srcImg.Data[(int)baseJ, (int)baseI, 1] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 1] * rI;
                    c1.Red = srcImg.Data[(int)baseJ, (int)baseI, 2] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 2] * rI;

                    Bgr c2 = new Bgr();
                    c2.Blue = srcImg.Data[(int)(baseJ + 1), (int)baseI, 0] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 0] * rI;
                    c2.Green = srcImg.Data[(int)(baseJ + 1), (int)baseI, 1] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 1] * rI;
                    c2.Red = srcImg.Data[(int)(baseJ + 1), (int)baseI, 2] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 2] * rI;

                    Bgr c = new Bgr();
                    c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                    c.Green = c1.Green * irJ + c2.Green * rJ;
                    c.Red = c1.Red * irJ + c2.Red * rJ;

                    scaledImg[j, i] = c;

                }
            return scaledImg;
        }

        //Сдвиг
        public Image<Bgr, byte> GetShearedImage(double shiftX, double shiftY)
        {
            Image<Bgr, byte> shearedImg = srcImg.CopyBlank();

            for (int x = 0; x < srcImg.Width - 1; x++)
                for (int y = 0; y < srcImg.Height - 1; y++)
                {
                    int newX = (int)(x + shiftX * (srcImg.Height - y));
                    int newY = (int)(y + shiftY * (srcImg.Width - x));
                    if (newX >= srcImg.Width - 1) continue;
                    if (newY >= srcImg.Height - 1) continue;
                    for (int i = 0; i < 3; i++)
                        shearedImg.Data[newY, newX, i] = srcImg.Data[y, x, i];
                }
            return shearedImg;
        }

        //Отражение
        public Image<Bgr, byte> GetReflectionImage(bool rotateOnHorizontal, bool rotateOnVerticale)
        {
            Image<Bgr, byte> reflectionImg = srcImg.CopyBlank();

            int flipOnHorizontal = rotateOnHorizontal ? 1 : 0;
            int flipOnVerticale = rotateOnVerticale ? 1 : 0;

            for (int x = 0; x < srcImg.Width - 1; x++)
                for (int y = 0; y < srcImg.Height - 1; y++)
                {
                   int newX = Math.Abs(x - (srcImg.Width - 1) * flipOnHorizontal);
                   int newY = Math.Abs(y - (srcImg.Height - 1) * flipOnVerticale);

                    for (int i = 0; i < 3; i++)
                        reflectionImg.Data[newY, newX, i] = srcImg.Data[y, x, i];
                }
                    
            return reflectionImg;
        }

        public Image<Bgr, byte> GetRotatedImage(double centerX, double centerY, int angle)
        {
            Image<Bgr, byte> rotatedImg = srcImg.CopyBlank();
            double radian = angle * Math.PI / 180;

            for (int x = 0; x < srcImg.Width - 1; x++)
                for (int y = 0; y < srcImg.Height - 1; y++)
                {
                    double newX = Math.Cos(radian) * (x - centerX) - Math.Sin(radian) * (y - centerY) + centerX;
                    double newY = Math.Sin(radian) * (x - centerX) + Math.Cos(radian) * (y - centerY) + centerY;

                    if (isPixelOnImage(newX, newY))
                        for (int i = 0; i < 3; i++)
                            rotatedImg.Data[(int)newY, (int)newX, i] = srcImg.Data[y, x, i];
                }
            for (int i = 0; i < rotatedImg.Width - 1; i++)
                for (int j = 0; j < rotatedImg.Height - 1; j++)
                {
                    double I = Math.Cos(-radian) * (i - centerX) - Math.Sin(-radian) * (j - centerY) + centerX;
                    double J = Math.Sin(-radian) * (i - centerX) + Math.Cos(-radian) * (j - centerY) + centerY;

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);

                    if (isPixelOnImage(baseI, baseJ))
                    {
                        double rI = I - baseI;
                        double rJ = J - baseJ;

                        double irI = 1 - rI;
                        double irJ = 1 - rJ;

                        try
                        {
                            Bgr c1 = new Bgr();
                            c1.Blue = srcImg.Data[(int)baseJ, (int)baseI, 0] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 0] * rI;
                            c1.Green = srcImg.Data[(int)baseJ, (int)baseI, 1] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 1] * rI;
                            c1.Red = srcImg.Data[(int)baseJ, (int)baseI, 2] * irI + srcImg.Data[(int)baseJ, (int)(baseI + 1), 2] * rI;

                            Bgr c2 = new Bgr();
                            c2.Blue = srcImg.Data[(int)(baseJ + 1), (int)baseI, 0] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 0] * rI;
                            c2.Green = srcImg.Data[(int)(baseJ + 1), (int)baseI, 1] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 1] * rI;
                            c2.Red = srcImg.Data[(int)(baseJ + 1), (int)baseI, 2] * irI + srcImg.Data[(int)(baseJ + 1), (int)(baseI + 1), 2] * rI;

                            Bgr c = new Bgr();
                            c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                            c.Green = c1.Green * irJ + c2.Green * rJ;
                            c.Red = c1.Red * irJ + c2.Red * rJ;

                            rotatedImg[j, i] = c;
                        }
                        catch(IndexOutOfRangeException ex)
                        {

                        }
                    }

                }

            return rotatedImg;
        }

        public Image<Bgr, byte> GetGomoImage(PointF[] srcPoints)
        {
            var destPoints= new PointF[]
            {
                    // плоскость, на которую осуществляется проекция,
                    // задаётся четыремя точками
                    new PointF(0, 0),
                    new PointF(0, srcImg.Height -1),
                    new PointF(srcImg.Width -1, srcImg.Height -1),
                    new PointF(srcImg.Width -1, 0)
            };
            // функция нахождения матрицы гомографической проекции
            var homographyMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, destPoints);
            var destImage = new Image<Bgr, byte>(srcImg.Size);
            CvInvoke.WarpPerspective(srcImg, destImage, homographyMatrix, destImage.Size);
            return destImage;
        }

        private bool isPixelOnImage(double x, double y)
        {
            bool onImage = true;
            if (x < 0) onImage = false;
            if (x >= srcImg.Width) onImage = false;
            if (y < 0) onImage = false;
            if (y >= srcImg.Height) onImage = false;
            return onImage;
        }


    }
}
