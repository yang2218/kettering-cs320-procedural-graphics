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
        protected static List<float> beccahsJankyMethod = new List<float>();
        //Gets the points from the file
        public static List<Vector2> getPoints()
        {
            List<Vector2> listOfPoints = new List<Vector2>();

            for (int i = 0; i < file.Length; i++)
            {
                Vector2 point = new Vector2(0, 0);
                point.X = Int32.Parse(file[i].Split(' ')[0]);
                point.Y = Int32.Parse(file[i].Split(' ')[1]);

                beccahsJankyMethod.Add(point.X);

                listOfPoints.Add(point);
            }

            return listOfPoints;
        }

        public static Vector2 findCentroid(List<Vector2> points)
        {
            Vector2 centroid = new Vector2(0, 0);
            float area = 0;

            //Calculate the Area 
            for (int i = 0; i < points.Count - 1; i++)
            {
                area += ((points[i].X * points[i + 1].Y) - (points[i + 1].X * points[i].Y));
            }
            area = area / 2;

            Console.WriteLine("AREA: " + area);

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
            return ((x - centroid.X) * (-1 / ss) + centroid.Y);
        }

        public float distance(Vector2 one, Vector2 two)
        {
            return (float)Math.Sqrt(Math.Pow((one.X - two.X), 2) + Math.Pow((one.Y - two.Y), 2));
        }

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
                    Vector2 vertex;
                    Vector2 vertex2;
                    if (vertices[i].X > vertices[i + 1].X)
                    {
                        vertex = vertices[i];
                        vertex2 = vertices[i + 1];
                    }
                    else
                    {
                        vertex = vertices[i + 1];
                        vertex2 = vertices[i];
                    }

                    //Calculate the intersection of the radius and the edge
                    slope_s = (vertex.Y - vertex2.Y) / (vertex.X - vertex2.X);

                    edgeInt.X = ((1 / slope_s) * vertex.X + centroid.Y +
                        slope_s * centroid.X - vertex.Y) /
                        (slope_s + (1 / slope_s));

                    edgeInt.Y = y1(edgeInt.X, slope_s, centroid);

                    //Calculate the distance between the center and the edgeInt
                    dist = distance(centroid, edgeInt);
                }
                radii.Add(dist);
            }

            Console.WriteLine("some radii: ");
            //Find the smallest of the radii
            for (int i = 0; i < radii.Count; i++)
            {
                Console.Write(radii[i]);
                if (i != radii.Count - 1)
                {
                    Console.Write(",");
                }
                if (radius > radii[i] && radii[i] > 0.0005)
                    radius = radii[i];
            }
            Console.WriteLine();

            Console.WriteLine("DA MIN RADIUS!!!!!!!! " + radius);
            //This is the radius of the circle
            return radius;
        }

        public float findWidth()
        {
            float max = beccahsJankyMethod[0];
            float min = beccahsJankyMethod[0];

            //Find max and min
            for (int i = 1; i < beccahsJankyMethod.Count; i++)
            {
                if (beccahsJankyMethod[i] > max)
                {
                    max = beccahsJankyMethod[i];
                }
                else if (beccahsJankyMethod[i] < min)
                {
                    min = beccahsJankyMethod[i];
                }
            }

            Console.WriteLine("Max: " + max + "; min: " + min);
            return (max - min);
        }

        protected static string[] file = File.ReadAllLines("colbysFile.txt");

        /// <summary>
        /// Run the class
        /// </summary>
        public override void Run()
        {
            Queue<List<Vector2>> make = new Queue<List<Vector2>>();
            StreamWriter file = new StreamWriter("circles.txt");
            //Get points from file
            List<Vector2> points;
            make.Enqueue(getPoints());
            int depthMax = 2;
            int depthSection = 1;
            int newSection = 0;
            float width = findWidth();

            for (int depth = 0; depth < depthMax; depth += 1)
            {
                newSection = 0;
                for (int jj = 0; jj < depthSection; jj += 1)
                {
                    points = make.Dequeue();
                    //Find centroid
                    Vector2 centroid = findCentroid(points);
                    Console.WriteLine("CENTROID: (" + centroid.X + "," + centroid.Y + ")");
                    //Calculate radius
                    float radius = findRadius(points, centroid);
                    Console.WriteLine("RADIUS: " + radius);
                    //Store circle in list
                    Vector3 firstCircle = new Vector3(centroid.X, centroid.Y, radius);
                    file.WriteLine(centroid.X/width + " " + centroid.Y/width + " " + radius/width);

                    //Find that arcpoint
                    //This doesn't handle vertical or horizontal lines
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        Vector2 corner = points[i];
                        Vector2 arc = new Vector2();
                        Vector2 firstPointOfTheSemiAwesomeArcPoint = new Vector2();
                        Vector2 secondPointOfTheSemiAwesomeArcPoint = new Vector2();
                        if (corner.X > centroid.X)
                        {
                            firstPointOfTheSemiAwesomeArcPoint = corner;
                            secondPointOfTheSemiAwesomeArcPoint = centroid;
                        }
                        else if (corner.X < centroid.X)
                        {
                            firstPointOfTheSemiAwesomeArcPoint = centroid;
                            secondPointOfTheSemiAwesomeArcPoint = corner;
                        }
                        float cornerSlope = 2f;
                        if (corner.X == centroid.X)
                        {
                            arc.X = corner.X;
                            if (corner.Y > centroid.Y)
                            {
                                arc.Y = centroid.Y + radius;
                            }
                            else
                            {
                                arc.Y = centroid.Y - radius;
                            }
                        }
                        else
                        {
                            cornerSlope = (firstPointOfTheSemiAwesomeArcPoint.Y - secondPointOfTheSemiAwesomeArcPoint.Y) / (firstPointOfTheSemiAwesomeArcPoint.X - secondPointOfTheSemiAwesomeArcPoint.X);
                            arc.X = (float)(Math.Sqrt(Math.Pow(radius, 2.0) / (1 + Math.Pow(cornerSlope, 2.0))) + centroid.X);
                            //Distance forumla solution doesn't give sign
                            if (corner.X < centroid.X)
                            {
                                arc.X -= centroid.X;
                                arc.X = centroid.X - arc.X;
                            }
                            arc.Y = (float)(cornerSlope * (arc.X - centroid.X) + centroid.Y);
                        }
                        double tanSlope = -(1 / cornerSlope);
                        Console.WriteLine();
                        Console.WriteLine("CORNER: (" + corner.X + "," + corner.Y + ")");
                        Console.WriteLine("ARC: (" + arc.X + "," + arc.Y + ")");
                        Console.WriteLine("TANSLOPE: " + tanSlope);

                        //Find the intersection with sides
                        Vector2 cross1, cross2;
                        Vector2 prevPoint, nextPoint;
                        prevPoint.X = 0;
                        prevPoint.Y = 0;
                        nextPoint.X = 0;
                        nextPoint.Y = 0;
                        cross1.X = 0;
                        cross1.Y = 0;
                        cross2.X = 0;
                        cross2.Y = 0;
                        for (int ii = 0; ii < points.Count - 1; ii++)
                        {
                            if (points[ii].X == corner.X && points[ii].Y == corner.Y)
                            {
                                nextPoint.X = points[ii + 1].X;
                                nextPoint.Y = points[ii + 1].Y;
                                if (ii == 0)
                                {
                                    prevPoint.X = points[points.Count - 2].X;
                                    prevPoint.Y = points[points.Count - 2].Y;
                                }
                                else
                                {
                                    prevPoint.X = points[ii - 1].X;
                                    prevPoint.Y = points[ii - 1].Y;
                                }
                                break;
                            }
                        }
                        //Find first point
                        if (prevPoint.X == corner.X)
                        {
                            cross1.X = prevPoint.X;
                            cross1.Y = ((float)tanSlope) * (cross1.X - arc.X) + arc.Y;
                        }
                        else
                        {
                            Vector2 first;
                            Vector2 second;
                            if (prevPoint.X > corner.X)
                            {
                                first = prevPoint;
                                second = corner;
                            }
                            else
                            {
                                first = corner;
                                second = prevPoint;
                            }
                            float cslope = (first.Y - second.Y) / (first.X - second.X);
                            cross1.X = (prevPoint.X * cslope - prevPoint.Y + arc.Y - ((float)tanSlope) * arc.X) / (cslope - ((float)tanSlope));
                            cross1.Y = cslope * (cross1.X - prevPoint.X) + prevPoint.Y;
                        }
                        Console.WriteLine("Cross1: (" + cross1.X + "," + cross1.Y + ")");
                        //Find second point
                        if (nextPoint.X == corner.X)
                        {
                            cross2.X = nextPoint.X;
                            cross2.Y = ((float)tanSlope) * (cross2.X - arc.X) + arc.Y;
                        }
                        else
                        {
                            Vector2 first;
                            Vector2 second;
                            if (nextPoint.X > corner.X)
                            {
                                first = nextPoint;
                                second = corner;
                            }
                            else
                            {
                                first = corner;
                                second = nextPoint;
                            }
                            float cslope = (first.Y - second.Y) / (first.X - second.X);
                            cross2.X = (nextPoint.X * cslope - nextPoint.Y + arc.Y - ((float)tanSlope) * arc.X) / (cslope - ((float)tanSlope));
                            cross2.Y = cslope * (cross2.X - nextPoint.X) + nextPoint.Y;
                        }
                        Console.WriteLine("Cross1: (" + cross2.X + "," + cross2.Y + ")");
                        List<Vector2> newpart = new List<Vector2>();
                        newpart.Add(cross1);
                        newpart.Add(corner);
                        newpart.Add(cross2);
                        newpart.Add(cross1);
                        make.Enqueue(newpart);
                        newSection += 1;
                        Console.WriteLine();
                    }
                }
                depthSection = newSection;
            }
            file.Flush();
            file.Close();
            Console.ReadLine();
        }
    }
}
