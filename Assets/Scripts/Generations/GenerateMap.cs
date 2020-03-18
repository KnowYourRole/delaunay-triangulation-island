using UnityEngine;
using System.Linq;
using TriangleNet.Geometry;
using System.Collections.Generic;
using TriangleNet.Voronoi;

public class Map
{
    public int generations;
    public Rectangle bounds;

    public List<Characteristics.Border> borders;
    public List<string> biomes;
    public List<Characteristics.Center> centers;
    public List<Characteristics.Corner> corners;

    public Map()
    {
        biomes = new List<string>();
        centers = new List<Characteristics.Center>();
        corners = new List<Characteristics.Corner>();
        borders = new List<Characteristics.Border>();
    }
}

public class GenerateMap
{
    private Map data;
    private Rectangle rectangle;
    private EnvironmentConstructionScript islandShape;
    private float height;
    private AnimationCurve heigtMap;

    public Map Generate(Vector2 dimensions, 
                        int generations,
                        PolygonList.FaceType faceType,
                        EnvironmentConstructionScript.FunctionType functionType,
                        float height, 
                        AnimationCurve heightMap, 
                        int regionCount, 
                        int relaxationCount, 
                        float radius)
    {
        data = new Map();
        data.generations = generations;
        this.height = height;
        this.heigtMap = heightMap;
        islandShape = new EnvironmentConstructionScript(generations, dimensions.x, dimensions.y, functionType);
        rectangle = new Rectangle(0, 0, dimensions.x, dimensions.y);
        if (faceType == PolygonList.FaceType.Hexagon || faceType == PolygonList.FaceType.Square)
            relaxationCount = 0;        // relaxation is used to create accurate polygon size
        // no specific value for rectangle - able to resize in the editor 

        Polygon polygon = PolygonList.Create(dimensions, generations, faceType, regionCount, radius);
        VoronoiBase voronoi = GenerateVoronoi(ref polygon, relaxationCount);
        Build(polygon, voronoi);
        ImproveBorders();
        // Determine the elevations and water at Voronoi corners.
        Elevation.AssignCorner(ref data, islandShape, faceType == PolygonList.FaceType.Hexagon || faceType == PolygonList.FaceType.Square);


        // Determine polygon and corner type: ocean, coast, land.

        CheckingScript.AssignSeaSealandAndLand(ref data);

        // Rescale elevations so that the highest is 1.0, and they're
        // distributed well. Lower elevations will be more common
        // than higher elevations. 

        List<Characteristics.Corner> corners = LandCorners(data.corners);
        Elevation.Readjust(ref corners);

        //  elevations assigned to water corners
        foreach (var q in data.corners)
        {
            if (q.sea || q.sealine)
                q.built = 0.0f; 
        }

        // Polygon elevations are the average of their corners
        Elevation.AllocatePolygon(ref data);

        // Determine humidity at corners, starting at rivers
        // and lakes, but not oceans. Then redistribute
        // humidity to cover the entire range evenly from 0.0
        // to 1.0. Then assign polygon humidity as the average
        // of the corner humidity.
       Humidity.AccountEdge(ref data);
       Humidity.Redistribute(ref corners);
       Humidity.AssignPolygon(ref data);

        CheckingScript.AssignHabitat(ref data);

        return data;
    }

    public GenerateMesh GenerateMeshData()
    {
        GenerateMesh meshData = new GenerateMesh();

        foreach (var centre in data.centers)
        {
            List<Vector3> vertices = new List<Vector3>();
            centre.corners = centre.corners.OrderBy(x => Mathf.Atan2(centre.pos.y - x.pos.y, centre.pos.x - x.pos.x)).ToList();

            foreach (var corner in centre.corners)
                vertices.Add(new Vector3(corner.pos.x, corner.pos.y, - heigtMap.Evaluate(corner.built) * height * corner.built));


            meshData.vertices.AddRange(vertices);
            meshData.AddPolygon(vertices.Count, centre.biome);
        }

        return meshData;
    }

    private VoronoiBase GenerateVoronoi(ref Polygon polygon, int relaxationCount = 0)
    {
        if (polygon.Count < 3)
            return null;

        StandardVoronoi voronoi = null;
        for (int i = 0; i < relaxationCount + 1; i++)
        {
            TriangleNet.Mesh mesh = (TriangleNet.Mesh)polygon.Triangulate();
            data.bounds = mesh.Bounds;
            voronoi = new StandardVoronoi(mesh, rectangle);
            if (relaxationCount != 0)
                polygon = voronoi.Lloyd_Relaxation(rectangle);
        }
        return voronoi;
    }

    private void AddToCornerList(List<Characteristics.Corner> corners, Characteristics.Corner corner)
    {
        if (corner != null && !corners.Contains(corner))
            corners.Add(corner);
    }

    private void AddToCenterList(List<Characteristics.Center> centers, Characteristics.Center center)
    {
        if (center != null && centers.IndexOf(center) < 0)
            centers.Add(center);

    }

    private Characteristics.Corner MakeCorner(Point point)
    {
        if (point == null)
            return null;
        for (int i = (int)(point.X) - 1; i <= (int)(point.X) + 1; i++)
        {
            if (_cornerMap.ContainsKey(i))
            {
                foreach (Characteristics.Corner corner in _cornerMap[i])
                {
                    float dx = (float)point.X - corner.pos.x;
                    float dy = (float)point.Y - corner.pos.y;
                    if (Mathf.Sqrt(dx * dx + dy * dy) < Mathf.Epsilon)
                        return corner;
                }
            }
        }

        int index = (int)point.X;
        if (!_cornerMap.ContainsKey(index) || _cornerMap[index] == null)
            _cornerMap[index] = new List<Characteristics.Corner>();
        Characteristics.Corner q = new Characteristics.Corner
        {
            id = data.corners.Count,
            pos = point.ToVector(),
            border = (point.X == 0 || point.X == rectangle.Width || point.Y == 0 || point.Y == rectangle.Height),
            touches = new List<Characteristics.Center>(),
            protrudes = new List<Characteristics.Border>(),
            adjacent = new List<Characteristics.Corner>()
        };
        data.corners.Add(q);
        _cornerMap[index].Add(q);
        return q;
    }

    Dictionary<int, List<Characteristics.Corner>> _cornerMap = new Dictionary<int, List<Characteristics.Corner>>();

    private void Build(Polygon polygon, VoronoiBase voronoi)
    {
        Dictionary<Point, Characteristics.Center> centerLoopup = new Dictionary<Point, Characteristics.Center>();

        foreach (var point in polygon.Points)
        {
            Characteristics.Center center = new Characteristics.Center
            {
                id = data.centers.Count,
                pos = point.ToVector(),
                neighbours = new List<Characteristics.Center>(),
                borders = new List<Characteristics.Border>(),
                corners = new List<Characteristics.Corner>()
            };
            data.centers.Add(center);
            centerLoopup[point] = center;
        }

        foreach (var face in voronoi.Faces)
        {
            face.BorderLooping(halfEdge =>
            {
                Point voronoiEdge1 = halfEdge.Origin;
                Point voronoiEdge2 = halfEdge.Twin.Origin;
                Point delaunayEdge1 = polygon.Points[halfEdge.Face.ID];
                Point delaunayEdge2 = polygon.Points[halfEdge.Twin.Face.ID];

                Characteristics.Border border = new Characteristics.Border
                {
                    id = data.borders.Count,
                    midPoint = new Vector2((float)(voronoiEdge1.X + voronoiEdge2.X) / 2, (float)(voronoiEdge1.Y + voronoiEdge2.Y) / 2),
                    v0 = MakeCorner(voronoiEdge1),
                    v1 = MakeCorner(voronoiEdge2),
                    d0 = centerLoopup[delaunayEdge1],
                    d1 = centerLoopup[delaunayEdge2]
                };

                if (border.d0 != null) { border.d0.borders.Add(border); }
                if (border.d1 != null) { border.d1.borders.Add(border); }
                if (border.v0 != null) { border.v0.protrudes.Add(border); }
                if (border.v1 != null) { border.v1.protrudes.Add(border); }

                data.borders.Add(border);

                if (border.d0 != null && border.d1 != null)
                {
                    AddToCenterList(border.d0.neighbours, border.d1);
                    AddToCenterList(border.d1.neighbours, border.d0);
                }

                if (border.v0 != null && border.v1 != null)
                {
                    AddToCornerList(border.v0.adjacent, border.v1);
                    AddToCornerList(border.v1.adjacent, border.v0);
                }

                if (border.d0 != null)
                {
                    AddToCornerList(border.d0.corners, border.v0);
                    AddToCornerList(border.d0.corners, border.v1);
                }
                if (border.d1 != null)
                {
                    AddToCornerList(border.d1.corners, border.v0);
                    AddToCornerList(border.d1.corners, border.v1);
                }

                if (border.v0 != null)
                {
                    AddToCenterList(border.v0.touches, border.d0);
                    AddToCenterList(border.v0.touches, border.d1);
                }
                if (border.v1 != null)
                {
                    AddToCenterList(border.v1.touches, border.d0);
                    AddToCenterList(border.v1.touches, border.d1);
                }
            });
        }
    }

    //Short edges become longer and long edges tend to become shorter. The
    // polygons tend to be more uniform after this step.
    private void ImproveBorders()
    {
        Vector2[] newCorners = new Vector2[data.corners.Count];
      
        foreach (var q in data.corners)
        {
            if (q.border)
            {
                newCorners[q.id] = q.pos;
            }
            else
            {
                Vector3 point = Vector2.zero;
                foreach (var r in q.touches)
                {
                    point.x += r.pos.x;
                    point.y += r.pos.y;
                }
                point.x /= q.touches.Count;
                point.y /= q.touches.Count;
                newCorners[q.id] = point;
            }
        }

      
        for (int i = 0; i < data.corners.Count; i++)
        {
            data.corners[i].pos = newCorners[i];
        }

        // The edge midpoints were computed for the old corners and need
        // to be recomputed.
        foreach (var edge in data.borders)
        {
            if (edge.v0 != null && edge.v1 != null)
            {
                edge.midPoint = (edge.v0.pos + edge.v1.pos) / 2;
            }
        }
    }

    // Create an array of corners that are on land only, for use by
    // algorithms that work only on land.  We return an array instead
    // of a vector because the redistribution algorithms want to sort
    // this array using Array.sortOn.
    public List<Characteristics.Corner> LandCorners(List<Characteristics.Corner> corners)
    {
        List<Characteristics.Corner> locations = new List<Characteristics.Corner>();
        foreach (var q in corners)
        {
            if (!q.sea && !q.sealine)
                locations.Add(q);
        }
        return locations;
    }
}
