using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK;


namespace ThereBeMonsters.Back_end.Modules
{
    class ColorCircle : Module
    {

        public IEnumerable<Vector3> Circles { private get; set; }

        [Parameter(@"How the calculated heightmap will be blended with the input heightmap.
(Input heightmap will be the Source, generated heightmap will be Destination)",
          Editor = typeof(Blend32bppDelegateEditor))]
        public Blend32bppDelegate BlendFunc { private get; set; }

        [Parameter(Hidden = true)]
        public float BlendFuncSrcFactor { private get; set; }
        [Parameter(Hidden = true)]
        public float BlendFuncDstFactor { private get; set; }

        public Color? Color { private get; set; }

        private uint[,] inputColorMap, outputColorMap;


        public uint[,] ColorMap
        {
            get { return outputColorMap; }
            set { inputColorMap = value; }
        }

        public override void Run()
        {
            int res = inputColorMap.GetLength(0);
            outputColorMap = new uint[res, res];
            int centerX, centerY, circleRadius, radiusSq, distSqFromCenter;
            int r, l, t, b;
            uint colorValue = 0;

            foreach (Vector3 circle in Circles)
            {
                centerX = (int)(circle.X * res);
                centerY = (int)(circle.Y * res);
                circleRadius = (int)(circle.Z * res);
                radiusSq = circleRadius * circleRadius;

                if (this.Color.HasValue)
                {
                    colorValue = (uint)this.Color.Value.ToArgb();
                }
                else
                {
                    byte redValue = (byte)Module.rng.Next(255);
                    byte greenValue = (byte)Module.rng.Next(255);
                    byte blueValue = (byte)Module.rng.Next(255);
                    colorValue = 0xFF000000 + redValue << 16 + greenValue << 8 + blueValue;
                }

                for (int i = 0; i <= circleRadius; i++)
                {
                    for (int j = 0; j <= circleRadius; j++)
                    {
                        distSqFromCenter = i * i + j * j;
                        if (distSqFromCenter > radiusSq)
                        {
                            continue;
                        }

                        r = Clamp(centerX + i, 0, res - 1);
                        l = Clamp(centerX - i, 0, res - 1);
                        t = Clamp(centerY + j, 0, res - 1);
                        b = Clamp(centerY - j, 0, res - 1);

                        outputColorMap[r, t] = colorValue;
                        outputColorMap[l, t] = colorValue;
                        outputColorMap[r, b] = colorValue;
                        outputColorMap[l, b] = colorValue;
                    }
                }
            }
        }

        private int Clamp(int v, int min, int max)
        {
            return v < min ? min : v > max ? max : v;
        }
    }
}

