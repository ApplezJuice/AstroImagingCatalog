using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumSharp;
using nom.tam.fits;
using nom.tam.image;
using System.Drawing;

namespace AstroImagingCatalog
{
    struct Coordinants
    {
        public int x, y, value;
    }

    public class NumSharpTest
    {
        public void InitImage(ImageHDU hdu, string imagePath)
        {

            Array[] data = (Array[])hdu.Kernel;
            int width = data.Length;
            int height = data[0].Length;
            //List<short[]> newData = new List<short[]>();
            Bitmap b16test;
            b16test = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            int threshold = 254;
            List<Coordinants> brightObjects = new List<Coordinants>();

            //var test = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (width == 0)
                {
                    width = data.Length;
                }
                short[] tempArray = (short[])data[i];

                for (int j = 0; j < tempArray.Length; j++)
                {
                    if(height == 0)
                    {
                       height = tempArray.Length;
                    }
                    //tempArray[j] = (short)((hdu.BZero + hdu.BScale * tempArray[j]) / 255);
                    
                    tempArray[j] = (short)((hdu.BZero + hdu.BScale * tempArray[j]) / 256);
                    //short trueValue = (short)(hdu.BZero + hdu.BScale * tempArray[j]);

                    //tempArray[j] = (short)((Math.Log(trueValue) * 28.8) - 65); - This is to bring out the image
                    Color newColor;
                    if (tempArray[j] > threshold)
                    {
                        Coordinants saveCorods = new Coordinants { x = i, y = j, value = tempArray[j] };
                        brightObjects.Add(saveCorods);
                        newColor = Color.FromArgb(255, 0, 0);
                    }
                    else
                    {
                        newColor = Color.FromArgb(tempArray[j], tempArray[j], tempArray[j]);
                    }
                    b16test.SetPixel(i, j, newColor);
                }

                //newData.Add(tempArray);
                data[i] = tempArray;
            }

            Bitmap imageWithLine = DrawConnectingLines(b16test, brightObjects);

            SaveImage(imageWithLine);
            //SaveImage(b16test);

        }


        private Bitmap DrawConnectingLines(Bitmap bmp, List<Coordinants> coords)
        {
            Pen redPen = new Pen(Color.Red, 3);

            int length = coords.Count;

            for (int i = 0; i < length - 1; i++)
            {
                for (int j = 1; j < length - 1; j++)
                {
                    
                    if(Math.Abs(coords[i].x - coords[j].x) < 10 || Math.Abs(coords[i].y - coords[j].y) < 10)
                    {
                        
                    }
                    else if (Math.Abs(coords[i].x - coords[j].x) > 500 || Math.Abs(coords[i].y - coords[j].y) > 500)
                    {

                    }
                    else
                    {
                        using (var graphics = Graphics.FromImage(bmp))
                        {
                            graphics.DrawLine(redPen, coords[i].x, coords[i].y, coords[j].x, coords[j].y);
                        }
                    }
                }
            }

            return bmp;
        }

        private void SaveImage(Bitmap bmp)
        {
            Image image = bmp;
            image.Save(@"C:\Users\mhayes\Desktop\filecatalogtest\dst\imagetest.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
