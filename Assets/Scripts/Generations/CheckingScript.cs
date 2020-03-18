using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Elevation
{
    // Checks the advancement and water in Voronoi. When built there is no local minimal.
    // This code will help later for the vectors used in the rives' algorithm. 
    // By built, there are low elevation areas in the island meaning that the rivers' ends will 
    // follow through the land. Lakes end up on river paths by built, because they do not rise
    // the elevation as much as the rest of the terrain. 
    public static void AssignCorner(ref Map map, EnvironmentConstructionScript islandShape, bool needsMoreRandomness)
    {
        System.Random mapRandom = new System.Random(map.generations);
        Queue<Characteristics.Corner> queue = new Queue<Characteristics.Corner>();
        foreach (var q in map.corners)
            q.water = !islandShape.IsInside(q.pos);
        
        foreach (var q in map.corners)
        {
            // Corners of the map is 0 when built
            if (q.border)
            {
                q.built = 0.0f;
                queue.Enqueue(q);
            }
            else
            {
                q.built = Mathf.Infinity;
            }
        }

        //If 'we' move away from the map border, the elevations will increase. This will enable the rivers go down 
        while (queue.Count > 0)
        {
            var q = queue.Dequeue();

            foreach (var s in q.adjacent)
            {
                // Every step up is epsilon over water or 1 over land. The
                // number doesn't matter because we'll rescale the
                // elevations later.
                var newBuilt = 0.01f + q.built;
                if (!q.water && !s.water)
                {
                    newBuilt += 1;
                    if (needsMoreRandomness)
                    {
                       // The map will look nice and smooth because of the random points, rivers and edges. 
                       // This random point slections are used in the square and hexagon grids only
                      
                        newBuilt += (float)mapRandom.NextDouble();
                    }
                }
                // If this point changed, we'll add it to the queue so
                // that we can process its neighbors too.
                if (newBuilt < s.built)
                {
                    s.built = newBuilt;
                    queue.Enqueue(s);
                }
            }
        }
    }

    // This will allow lower builds to be more than the higher builds. 
    // The X will have a frequency of 1-x. In order to do this the corners are 
    // sorted and set in their desired 'positions'  
    public static void Readjust(ref List<Characteristics.Corner> locations)
    {
        // MountainComponent increases the mountain area. At 1.0, it is barely seen, so it is set to 1.1.
        float MountainComponent = 1.1f;
        float x, y;

        locations = locations.OrderBy(loc => loc.built).ToList();
        for (int i = 0; i < locations.Count; i++)
        {
            y = (float)i / (locations.Count - 1);
            x = Mathf.Sqrt(MountainComponent) - Mathf.Sqrt(MountainComponent * (1 - y));
            x = x > 1.0f ? 1.0f : x;
            locations[i].built = x;
        }
    }
    // Pollygons are allocated average of the built of their corners
    public static void AllocatePolygon(ref Map map)
    {
        float sumBuilt;
        foreach (var p in map.centers)
        {
            sumBuilt = 0.0f;
            foreach (var q in p.corners)
                sumBuilt += q.built;
            p.built = sumBuilt / p.corners.Count;
        }
    }

}
