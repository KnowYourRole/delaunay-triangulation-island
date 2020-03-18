using UnityEngine;
using TriangleNet.Geometry;

public class PolygonList
{
    public enum FaceType
    { 
        Square,
        Random,
        PoisonDisc,
        Hexagon
    }

    private static Vector2 values;
    private static int seed;
    private static int territoryCount;
    private static float orbit;

    public static Polygon Create(Vector2 values, int seed, FaceType faceType, int territoryCount = 0, float orbit = 0)
    {
        //naming the variables in this script
        PolygonList.values = values;
        PolygonList.seed = seed;
        PolygonList.territoryCount = territoryCount;
        PolygonList.orbit = orbit;

        switch (faceType)   //can switch between face types
        {
            case FaceType.Random:
                return CreateRandom();
            case FaceType.Square:
                return CreateSquare();
            case FaceType.Hexagon:
                return CreateHexagon();
            case FaceType.PoisonDisc:
                return CreatePoisonDisc();
           default:
            return null;
        }
    }

    private static Polygon CreateRandom()
    {
        Polygon polygon = new Polygon();
        Random.InitState(seed);
        for (int i = 0; i < territoryCount; i++)
            polygon.Add(new Vertex(Random.Range(0, values.x), Random.Range(0, values.y)));
      
        return polygon;

    }

    private static Polygon CreatePoisonDisc()
    {
        Polygon polygon = new Polygon();
        Random.InitState(seed);
        DiscPoisonShape poissonDiscSampler = new DiscPoisonShape(values.x, values.y, orbit);
        foreach (var sample in poissonDiscSampler.Samples())
            polygon.Add(sample.ToVertex());
        return polygon;
    }

    private static Polygon CreateHexagon()
    {
        Polygon centroids = new Polygon();
        territoryCount = (int)Mathf.Sqrt(territoryCount);

        for (int x = 0; x < territoryCount; x++)
        {
            for (int y = 0; y < territoryCount; y++)
            {
                Vertex vertex = new Vertex(((0.5f + x) / territoryCount) * values.x, ((0.25f + (0.5f * (x % 2)) + y) / territoryCount) * values.y);
                centroids.Add(vertex);
            }
        }

        return centroids;
    }

    private static Polygon CreateSquare()
    {
        Polygon centroids = new Polygon();
        territoryCount = (int)Mathf.Sqrt(territoryCount);

        for (int x = 0; x < territoryCount; x++)
        {
            for (int y = 0; y < territoryCount; y++)
            {
                Vertex vertex = new Vertex(((0.5f + x) / territoryCount) * values.x, ((0.5f + y) / territoryCount) * values.y);
                centroids.Add(vertex);
            }
        }

        return centroids;
    }
}
