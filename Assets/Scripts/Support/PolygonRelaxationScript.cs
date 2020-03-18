using UnityEngine;
using TriangleNet.Voronoi;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using System.Collections.Generic;
using Vertex = TriangleNet.Geometry.Vertex;
using TriangleNet.Topology.DCEL;
/// https://penetcedric.wordpress.com/2017/06/13/polygon-maps/
public static class PolygonRelaxationScript
{
    public static Vector2 ToVector(this Point point)
    {
        return new Vector2((float)point.X, (float)point.Y);
    }

    public static Vertex ToVertex(this Vector2 vector)
    {
        return new Vertex(vector.x, vector.y);
    }

    public static Polygon Lloyd_Relaxation(this VoronoiBase voronoi, Rectangle rectangle)
    {
     
        Polygon centroid = new Polygon(voronoi.Faces.Count);

        //loop 
        for (int i = 0; i < voronoi.Faces.Count; ++i)
        {
            Vector2 average = new Vector2(0, 0);
            HashSet<Vector2> verts = new HashSet<Vector2>();

            var edge = voronoi.Faces[i].Edge;
            var first = edge.Origin.ID;

            voronoi.Faces[i].BorderLooping(rectangle, true, (v1, v2) =>
            {
                if (!verts.Contains(v1))
                    verts.Add(v1);
                if (!verts.Contains(v2))
                    verts.Add(v2);
            });

            if (verts.Count == 0)
                continue;
        
            // centroid
            var vertsEnum = verts.GetEnumerator();
            while (vertsEnum.MoveNext())
                average += vertsEnum.Current;
            average /= verts.Count;
            centroid.Add(average.ToVertex());
        }
        return centroid;
    }

    
    public static void BorderLooping(this Face face, Rectangle rectangle, bool clipEdges, System.Action<Vector2, Vector2> OnEdge)
    {
        var border = face.Edge;
        var first = border.Origin.ID;
        do
        {
            // vertices position
            Point p1 = new Point(border.Origin.X, border.Origin.Y);
            Point p2 = new Point(border.Twin.Origin.X, border.Twin.Origin.Y);

            if (clipEdges)
            {
                if ((rectangle.Contains(p1) && !rectangle.Contains(p2)))
                {
                    IntersectionHelper.BoxRayIntersection(rectangle, p1, p2, ref p2); 
                }
                else if (!rectangle.Contains(p1) && rectangle.Contains(p2))
                {
                    IntersectionHelper.BoxRayIntersection(rectangle, p2, p1, ref p1); 
                }
                else if (!rectangle.Contains(p1) && !rectangle.Contains(p2)) 
                {
                    border = border.Next;
                    continue;
                }
            }

            Vector2 origin = p1.ToVector();
            Vector2 end = p2.ToVector();

            OnEdge?.Invoke(origin, end);
            border = border.Next;
        } while (border != null && border.Origin.ID != first);
    }

    public static void BorderLooping(this Face face, System.Action<HalfEdge> OnEdge)
    {
        var border = face.Edge;
        var first = border.Origin.ID;
        do
        {
            OnEdge?.Invoke(border);
            border = border.Next;
        } while (border != null && border.Origin.ID != first);
    }


}
