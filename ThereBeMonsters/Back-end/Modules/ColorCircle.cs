using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Drawing
{
    class DrawCircle
    {
        static bool initialized = false;//when first starting, color array is empty

        static Random red = new Random();
        static float redValue = red.Next(0) + red.Next(100) / 100f;

        static Random green = new Random();
        static float greenValue = green.Next(0) + green.Next(100) / 100f;

        static Random blue = new Random();
        static float blueValue = blue.Next(0) + blue.Next(100) / 100f;

        static int arrayWidth = 1000;
        static int arrayHeight = 1000;
        double[,] colorOutput = new double[arrayWidth, arrayHeight];

        void fillWithWhite()
        {//fills colorOutput 2D array with all white
            if (initialized == false)
            {
                for (int i = 0; i < 1000; i++)
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        colorOutput[i, j] = 0;
                    }
                }
                initialized = true;
            }
        }

        void populateColor()
        {
            double currentColor = colorOutput[0, 0];
            bool writeColor = false;
            for (int i = 0; i <= 1000-2; i++)
            {
                for (int j = 0; j <= 1000-2; j++)
                {
                    
                    if (currentColor != colorOutput[i, j + 1])
                    {
                        if (writeColor == true)
                            writeColor = false;
                        else
                            writeColor = true;
                    }
                        if (writeColor == true)
                            colorOutput[i,j + 1] = currentColor;

                }
            }

        }
        public void ColorRandom(Point center, float radius)
        {
            fillWithWhite();// if the bitmap is null, fill it all with wite placeholder color

            //Console.WriteLine("Red " + redValue);
            //Console.WriteLine("Green " + greenValue);
            //Console.WriteLine("Blue " + blueValue);

            GL.Color3(redValue, blueValue, greenValue);

            int redInt = Convert.ToInt32(redValue * 1000000);
            int blueInt = Convert.ToInt32(blueValue * 10000);
            int greenInt = Convert.ToInt32(greenValue * 100);
            int allColor = redInt + blueInt + greenInt;

            GL.Begin(BeginMode.TriangleFan);//draws circle, colored with supplied.

            for (int i = 0; i < 360; i++)
            {
                double degInRad = i * Math.PI / 180;
                double xPos = ((Math.Cos(degInRad) * radius) + center.X);
                int arrayXPos = Convert.ToInt32(xPos);
                double yPos = ((Math.Sin(degInRad) * radius + center.Y));
                int arrayYPos = Convert.ToInt32(yPos);
                GL.Vertex2(xPos, yPos);
                colorOutput[arrayXPos, arrayYPos] = allColor;
            }

            GL.End();
            populateColor();
            for (int i = 0; i < arrayHeight; i++)
            {
                for (int j = 0; j < arrayWidth; j++)
                {
                      Console.Write(colorOutput[i, j]);
                }
                Console.WriteLine("");
            }


        }
        void fillColorArray()
        {

        }
    }


}
