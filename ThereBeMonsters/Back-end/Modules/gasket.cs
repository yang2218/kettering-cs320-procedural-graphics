using System;
using System.Collections.Generic;
using System.IO;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
    //Defining a point
    public class Point
    {
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x;
        public double y;
    }

    // This attribute is optional and just for adding metadata
    [Module("Apollonian Gasket",  // Adds a description to this module's metadata
        Reusable = true)] // Marks reusable (the default). Set this to false if you want this module instantiated for each time it appears in the module graph, or leave it out.
    public class Gasket : Module
    {
        //Gets the points from the file
        public static List<Point> getPoints()
        {
            List<Point> listOfPoints = new List<Point>();

            for (int i = 0; i < file.Length; i++)
            {
                Point point = new Point(0,0);
                point.x = Int32.Parse(file[i].Split(' ')[0]);
                point.y = Int32.Parse(file[i].Split(' ')[1]);

                listOfPoints.Add(point);
            }

            return listOfPoints;
        }

        public static Point findCentroid(List<Point> points)
        {
            Point centroid = new Point(0,0);
            double area = 0;

            //Calculate the Area 
            for (int i = 0; i < points.Count - 1; i++)
            {
                area += ((points[i].x * points[i + 1].y) - (points[i + 1].x * points[i].y));
            }
            area = area / 2;

            //Calculate x value
            for (int i = 0; i < points.Count - 1; i++)
            {
                centroid.x += (points[i].x + points[i + 1].x *
                    (points[i].x * points[i + 1].y) - 
                    (points[i + 1].x * points[i].y));
            }
            centroid.x = (1 / (6 * area)) * centroid.x;

            //Calculate y value
            for (int i = 0; i < points.Count - 1; i++)
            {
                centroid.y += (points[i].y + points[i + 1].y *
                    (points[i].x * points[i + 1].y) -
                    (points[i + 1].x * points[i].y));
            }
            centroid.y = (1 / (6 * area)) * centroid.y;

            return centroid;
        }

        public double y1(double x, double ss, Point centroid)
        {
            return ((x - centroid.x) * (-1/ss) + centroid.y);
        }

        public double distance(Point one, Point two)
        {
            return Math.Sqrt(Math.Pow((one.x - two.x), 2) + Math.Pow((one.y - two.y) , 2));
        }

        public double findRadius(List<Point> vertices, Point centroid)
        {
            double radius = 0;
            double slope_s = 0;
            Point edgeInt = new Point(0, 0);
            List<double> radii = new List<double>();

            //Go through every edge and calculate distance to it
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                //Calculate the intersection of the radius and the edge
                slope_s = Math.Abs((vertices[i].y - vertices[i + 1].y) / 
                    (vertices[i].x - vertices[i + 1].x));

                edgeInt.x = ((1 / slope_s) * centroid.y + centroid.y +
                    slope_s * vertices[i].x - vertices[i].y) /
                    (slope_s + 1 / slope_s);

                edgeInt.y = y1(edgeInt.x, slope_s, centroid);

                //Calculate the distance between the center and the edgeInt
                double dist = distance(centroid, edgeInt);
                radii.Add(dist);
            }

            //Add the last edge (between n and 0)
            slope_s = Math.Abs((vertices[vertices.Count - 1].y - vertices[0].y) /
                (vertices[vertices.Count - 1].x - vertices[0].x));

            edgeInt.x = ((1 / slope_s) * centroid.y + centroid.y +
                slope_s * vertices[vertices.Count - 1].x - vertices[vertices.Count - 1].y) /
                (slope_s + 1 / slope_s);

            edgeInt.y = y1(edgeInt.x, slope_s, centroid);

            double l_dist = distance(centroid, edgeInt);
            radii.Add(l_dist);

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
            List<Point> points = getPoints();

            //Find centroid
            Point centroid = findCentroid(points);

            //Calculate radius
            double radius = findRadius(points, centroid);

            
        }
    }
}
