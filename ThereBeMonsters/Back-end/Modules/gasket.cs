using System;
using System.Collections.Generic;
using System.IO;
using ThereBeMonsters.Back_end;
using OpenTK;

namespace ThereBeMonsters.Back_end.Modules
{

    // This attribute is optional and just for adding metadata
    [Module("Apollonian Gasket",  // Adds a description to this module's metadata
        Reusable = true)] // Marks reusable (the default). Set this to false if you want this module instantiated for each time it appears in the module graph, or leave it out.
    public class Gasket : Module
    {
        //Gets the points from the file
        public static List<Vector2> getPoints()
        {
            List<Vector2> listOfPoints = new List<Vector2>();

            for (int i = 0; i < file.Length; i++)
            {
                Vector2 point = new Vector2(0,0);
                point.X = Int32.Parse(file[i].Split(' ')[0]);
                point.Y = Int32.Parse(file[i].Split(' ')[1]);

                listOfPoints.Add(point);
            }

            return listOfPoints;
        }

        public static Vector2 findCentroid(List<Vector2> points)
        {
            Vector2 centroid = new Vector2(0,0);
            float area = 0;

            //Calculate the Area 
            for (int i = 0; i < points.Count - 1; i++)
            {
                area += ((points[i].X * points[i + 1].Y) - (points[i + 1].X * points[i].Y));
            }
            area = area / 2;

            Console.WriteLine("Area: " + area);

            //Calculate x value
            for (int i = 0; i < points.Count - 1; i++)
            {
                centroid.X += (points[i].X + points[i + 1].X) *
                    ((points[i].X * points[i + 1].Y) - 
                    (points[i + 1].X * points[i].Y));
            }
            centroid.X = (1 / (6 * area)) * centroid.X;

            //Calculate y value
            for (int i = 0; i < points.Count - 1; i++)
            {
                centroid.Y += (points[i].Y + points[i + 1].Y) *
                    ((points[i].X * points[i + 1].Y) -
                    (points[i + 1].X * points[i].Y));
            }
            centroid.Y = (1 / (6 * area)) * centroid.Y;

            return centroid;
        }

        public float y1(float x, float ss, Vector2 centroid)
        {
            return ((x - centroid.X) * (-1/ss) + centroid.Y);
        }

        /*public float distance(Vector2 one, Vector2 two)
        {
            return (float)Math.Sqrt(Math.Pow((one.X - two.X), 2) + Math.Pow((one.Y - two.Y) , 2));
        }*/

        public float findRadius(List<Vector2> vertices, Vector2 centroid)
        {
            float radius = 10380f;
            float slope_s = 0f;
            float dist = 0f;
            Vector2 edgeInt = new Vector2(0, 0);
            List<float> radii = new List<float>();

            //Go through every edge and calculate distance to it
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                //Check for zeros (aka horizontala and vertical lines)
                if (vertices[i].Y - vertices[i + 1].Y == 0)
                {
                    dist = Math.Abs(vertices[i].X - centroid.X);
                }
                else if (vertices[i].X - vertices[i + 1].X == 0)
                {
                    dist = Math.Abs(vertices[i].Y - centroid.Y);
                }
                else
                {
                    //Calculate the intersection of the radius and the edge
                    slope_s = (vertices[i].Y - vertices[i + 1].Y) /
                        (vertices[i].X - vertices[i + 1].X);

                    edgeInt.X = ((1 / slope_s) * vertices[i].X - centroid.Y +
                        slope_s * centroid.X + vertices[i].Y) /
                        (slope_s + (1 / slope_s));

                    edgeInt.Y = y1(edgeInt.X, slope_s, centroid);
                    
                    //Calculate the distance between the center and the edgeInt
                    dist = (centroid - edgeInt).Length; //distance(centroid, edgeInt);
                }
                radii.Add(dist);
            }

            //Find the smallest of the radii
            for (int i = 0; i < radii.Count; i++)
            {
                if (radius > radii[i])
                    radius = radii[i];
            }

            //This is the radius of the circle
            return radius;
        }

        protected static string[] file = File.ReadAllLines("colbysFile.txt");

        /// <summary>
        /// Run the class
        /// </summary>
        public override void Run()
        {

            //Get points from file
            List<Vector2> points = getPoints();

            //Find centroid
            Vector2 centroid = findCentroid(points);
            Console.WriteLine(centroid.X);
            Console.WriteLine(centroid.Y);
            //Calculate radius
            float radius = findRadius(points, centroid);
            Console.WriteLine(radius);
            Console.ReadLine();
            //Store circle in list
            List<Vector3> circles = new List<Vector3>();
            Vector3 firstCircle = new Vector3(centroid.X, centroid.Y, radius);
            circles.Add(firstCircle);


        }
    }
}
