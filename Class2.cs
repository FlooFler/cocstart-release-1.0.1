using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;


public class BallGenerator
{
    private World world;
    private Vector2 position;
    private List<Body> balls;
    private int maxBalls = 10;
    public int ballsCreated = 0;
    private float timer = 0f;
    private float spawnInterval = 0.1f;
    public bool start = false;
    

    private float size;
    private Image image;

    public BallGenerator(World world, Vector2 position, float size, Image image, List<Body> balls)
    {
        this.world = world;
        this.position = position;
        this.balls = balls;
        this.image = image;
        this.size = size;
    }

    public void tick(Graphics g)
    {
        g.DrawImage(image, position.X - size / 2, position.Y, size, size);
    }

    public void Update()
    {
        // Detectăm dacă start a trecut de la false la true
        

        // Stocăm starea actuală pentru următorul frame
        

        if (!start) return;
        if (ballsCreated >= maxBalls) return;

        timer += 1f / 60f;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBall();
        }
    }

    private void SpawnBall()
    {


        Random rand = new Random();
         Body ball = BodyFactory.CreateCircle(world, 0.2f, 1f, position);
        ball.BodyType = BodyType.Dynamic;

        ball.Friction = 0.2f;
        ball.Restitution = 0.4f;
        ball.LinearDamping = 0.0f;
        ball.AngularDamping = 0.0f;

        ball.CollidesWith = Category.All & ~Category.Cat1;
        ball.CollisionCategories = Category.Cat1;

        // Adaugă un mic defazaj aleator la viteza inițială
        float dx = (float)(rand.NextDouble() * 0.2 - 0.1); // între -0.1 și 0.1
        float dy = (float)(rand.NextDouble() * 0.2 - 0.1);

        ball.LinearVelocity = new Vector2(dx, dy); // opțional: adaugă și o direcție generală

        balls.Add(ball);
        ballsCreated++;
    }
}

public class star
{
    private World world;
    private Vector2 position;
    private float size;
    private Image image;
    private List<Body> balls;   
    public bool isActive = true;
    Body starBody;

    public star(World world, Vector2 position, float size, Image image, List<Body> balls)
    {
        this.world = world;
        this.position = position;
        this.image = image;
        this.size = size;
        this.balls = balls;
        
        starBody = BodyFactory.CreateCircle(world, 0.3f, 1f, position);
        starBody.BodyType = BodyType.Static;
        starBody.IsSensor = true;
    }
    public void tick(Graphics g)
    {

        if (!isActive || balls == null || starBody == null || image == null) return;
        foreach (var ball in balls.ToList())
        {
           
                float dist = Vector2.Distance(ball.Position , starBody.Position + new Vector2(size / 2, size / 2));
            if (dist < 0.6f) 
            {
                isActive = false;
                break;
            }


        }
        g.DrawImage(image, position.X, position.Y, size, size);
            
        
    }
    




}
public class StaticPolygon
{
    private List<Vector2> vertices;
    private Body polygonBody;
    private bool isActive = true;
    

    public StaticPolygon(World world, List<Vector2> vertices, List<Body> balls)
    {
        
        this.vertices = vertices;
        
        

        polygonBody = BodyFactory.CreatePolygon(world, new Vertices(vertices), 0.1f);
        polygonBody.BodyType = BodyType.Static;
        polygonBody.CollisionCategories = Category.Cat2;    
    }

    public void Tick(Graphics g)
    {
        if (!isActive) return;




        g.FillPolygon(Brushes.Black, ConvertToPoints(vertices));
    }

    public bool ShouldRemove() => !isActive;
    public static PointF[] ConvertToPoints(List<Vector2> vectors)
    {
        return vectors.Select(v => new PointF(v.X, v.Y)).ToArray();
    }
}
public class Staticline
{
    private List<Vector2> points;
    private Body lineBody;
    private bool isActive = true;

    public Staticline(World world, List<Vector2> points, List<Body> balls)
    {
        this.points = points;

        var vertices = new Vertices(points);
        lineBody = BodyFactory.CreateChainShape(world, vertices);
        lineBody.BodyType = BodyType.Static;
        lineBody.CollisionCategories = Category.Cat2;
    }

    public void Tick(Graphics g)
    {
        if (!isActive) return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            g.DrawLine( new Pen(Brushes.Black, 0.1f), ConvertToPoint(points[i]), ConvertToPoint(points[i + 1]));
        }
    }

    public bool ShouldRemove() => !isActive;

    public static PointF ConvertToPoint(Vector2 v)
    {
        return new PointF(v.X, v.Y);
    }
}



public static class MarchingSquares
{
    private const float Tolerance = 0.0001f;

    private static int GetState(float A, float B, float C, float D, float isoLevel)
    {
        int a = A >= isoLevel ? 1 : 0;
        int b = B >= isoLevel ? 1 : 0;
        int c = C >= isoLevel ? 1 : 0;
        int d = D >= isoLevel ? 1 : 0;

        return (a << 3) | (b << 2) | (c << 1) | d;
    }

    private static Vector2 Interpolate(float isoLevel, Vector2 p1, Vector2 p2, float valp1, float valp2)
    {
        if (Math.Abs(isoLevel - valp1) < 1e-6)
            return p1;
        if (Math.Abs(isoLevel - valp2) < 1e-6)
            return p2;
        if (Math.Abs(valp1 - valp2) < 1e-6)
            return p1;

        float t = (isoLevel - valp1) / (valp2 - valp1);
        return new Vector2(p1.X + t * (p2.X - p1.X), p1.Y + t * (p2.Y - p1.Y));
    }

    public static List<(Vector2, Vector2)> GenerateSegments(float[,] values, float isoLevel)
    {
        int width = values.GetLength(0);
        int height = values.GetLength(1);

        // Cache pentru punctele de intersecție pe muchii:
        Vector2?[,] edgeTop = new Vector2?[width - 1, height];
        Vector2?[,] edgeRight = new Vector2?[width, height - 1];
        Vector2?[,] edgeBottom = new Vector2?[width - 1, height];
        Vector2?[,] edgeLeft = new Vector2?[width, height - 1];

        // Calculează punctele de intersecție pe muchiile sus-jos
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                edgeTop[x, y] = Interpolate(
                    isoLevel,
                    new Vector2(x, y),
                    new Vector2(x + 1, y),
                    values[x, y],
                    values[x + 1, y]);
            }
        }

        // Calculează punctele de intersecție pe muchiile stânga-dreapta
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                edgeRight[x, y] = Interpolate(
                    isoLevel,
                    new Vector2(x, y + 1),
                    new Vector2(x, y),
                    values[x, y + 1],
                    values[x, y]);
            }
        }

        var lineSegments = new List<(Vector2, Vector2)>();

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                float A = values[x, y + 1];
                float B = values[x + 1, y + 1];
                float C = values[x + 1, y];
                float D = values[x, y];

                if (float.IsInfinity(A) || float.IsInfinity(B) ||
               float.IsInfinity(C) || float.IsInfinity(D) ||
               float.IsNaN(A) || float.IsNaN(B) ||
               float.IsNaN(C) || float.IsNaN(D))
                {
                    continue;
                }

                int state = GetState(A, B, C, D, isoLevel);
                if (state == 0 || state == 15)
                    continue;

                // Interpolare puncte pe margini celulei curente
                var top = edgeTop[x, y + 1].Value;       // muchia de sus
                var right = edgeRight[x + 1, y].Value;   // muchia din dreapta
                var bottom = edgeTop[x, y].Value;         // muchia de jos (sus a celulei de jos)
                var left = edgeRight[x, y].Value;         // muchia din stânga

                switch (state)
                {
                    case 1: lineSegments.Add((left, bottom)); break;
                    case 2: lineSegments.Add((bottom, right)); break;
                    case 3: lineSegments.Add((left, right)); break;
                    case 4: lineSegments.Add((right, top)); break;
                    case 5:
                        lineSegments.Add((left, top));
                        lineSegments.Add((bottom, right));
                        break;
                    case 6: lineSegments.Add((bottom, top)); break;
                    case 7: lineSegments.Add((left, top)); break;
                    case 8: lineSegments.Add((top, left)); break;
                    case 9: lineSegments.Add((top, bottom)); break;
                    case 10:
                        lineSegments.Add((top, right));
                        lineSegments.Add((left, bottom));
                        break;
                    case 11: lineSegments.Add((top, right)); break;
                    case 12: lineSegments.Add((right, left)); break;
                    case 13: lineSegments.Add((right, bottom)); break;
                    case 14: lineSegments.Add((bottom, left)); break;
                }
            }
        }

        return lineSegments;
    }

    public static List<PointF[]> GenerateContours(
    float[,] values, float isoLevel,
    float offsetX, float offsetY,
    float stepX, float stepY)
    {
        var segments = GenerateSegments(values, isoLevel);
        var contours = new List<PointF[]>();

        if (segments.Count == 0)
            return contours;

        var segmentMap = new Dictionary<Vector2, List<Vector2>>(new Vector2Comparer());

        foreach (var (a, b) in segments)
        {
            if (!segmentMap.TryGetValue(a, out var listA))
            {
                listA = new List<Vector2>();
                segmentMap[a] = listA;
            }
            listA.Add(b);

            if (!segmentMap.TryGetValue(b, out var listB))
            {
                listB = new List<Vector2>();
                segmentMap[b] = listB;
            }
            listB.Add(a);
        }

        var used = new HashSet<(Vector2, Vector2)>(new SegmentComparer());

        foreach (var start in segmentMap.Keys)
        {
            foreach (var next in segmentMap[start])
            {
                if (used.Contains((start, next)) || used.Contains((next, start)))
                    continue;

                var contour = new List<Vector2> { start, next };
                used.Add((start, next));

                Vector2 current = next;
                Vector2 previous = start;

                while (true)
                {
                    var neighbors = segmentMap[current];
                    Vector2? nextPoint = null;

                    foreach (var n in neighbors)
                    {
                        if (n != previous && !used.Contains((current, n)) && !used.Contains((n, current)))
                        {
                            nextPoint = n;
                            break;
                        }
                    }

                    if (nextPoint == null || nextPoint == start)
                        break;

                    previous = current;
                    current = nextPoint.Value;
                    contour.Add(current);
                    used.Add((previous, current));
                }

                if (contour.Count >= 2)
                {
                    var converted = contour
                        .Select(v => new PointF(offsetX + v.X * stepX, offsetY + v.Y * stepY))
                        .ToArray();
                    contours.Add(converted);
                }
            }
        }

        return contours;
    }


    public class Vector2Comparer : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) < Tolerance &&
                   Math.Abs(a.Y - b.Y) < Tolerance;
        }

        public int GetHashCode(Vector2 v)
        {
            int x = (int)Math.Round(v.X / Tolerance);
            int y = (int)Math.Round(v.Y / Tolerance);
            return x * 397 ^ y;
        }
    }
    private class SegmentComparer : IEqualityComparer<(Vector2, Vector2)>
    {
        private readonly Vector2Comparer comparer = new Vector2Comparer();

        public bool Equals((Vector2, Vector2) x, (Vector2, Vector2) y)
        {
            return (comparer.Equals(x.Item1, y.Item1) && comparer.Equals(x.Item2, y.Item2)) ||
                   (comparer.Equals(x.Item1, y.Item2) && comparer.Equals(x.Item2, y.Item1));
        }

        public int GetHashCode((Vector2, Vector2) obj)
        {
            int h1 = comparer.GetHashCode(obj.Item1);
            int h2 = comparer.GetHashCode(obj.Item2);
            return h1 ^ h2;
        }
    }

}

public class Level
{
    public List<Vector2> GenPositions { get; set; } = new List<Vector2>();
    public List<Vector2> StarPositions { get; set; } = new List<Vector2>();
    public List<List<Vector2>> Lines { get; set; } = new List<List<Vector2>>();
    public List<List<Vector2>> Polygons { get; set; } = new List<List<Vector2>>();
    public bool free_mode { get; set; }
    public int level_id { get; set; }
    public Color Graph_color { get; set; }
    public Color Ball_color { get; set; }
}
public class GameStats
{
    public HashSet<int> LevelsCompleted { get; set; } = new HashSet<int>();
    public int StarsCollected { get; set; }
    public float TotalPlayTimeSeconds { get; set; }
}

