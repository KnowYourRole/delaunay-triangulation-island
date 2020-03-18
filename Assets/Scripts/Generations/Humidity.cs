using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Humidity
{
    // Calculate Humidity. Humidity is the fresh water sources and they are the rivers and lakes 
    // Salt water sources do have this Humidity but they cannot spread it leading to the rivers 
    // and lakes not going on top of the land
    
    public static void AccountEdge(ref Map map)
    {
        float newHumidity;
        Queue<Characteristics.Corner> queue = new Queue<Characteristics.Corner>();
        // Fresh water
        foreach (var q in map.corners)
        {
            if ((q.water || q.river > 0) && !q.sea)
            {
                q.humidity = q.river > 0 ? Mathf.Min(3.0f, (0.2f * q.river)) : 1.0f;
                queue.Enqueue(q);
            }
            else
                q.humidity = 0.0f;
        }
        while (queue.Count > 0)
        {
            var q = queue.Dequeue();

            foreach (var r in q.adjacent)
            {
                newHumidity = q.humidity * 0.9f;
                if (newHumidity > r.humidity)
                {
                    r.humidity = newHumidity;
                    queue.Enqueue(r);
                }
            }
        }
        // This is the salt water
        foreach (var q in map.corners)
        {
            if (q.sea || q.sealine)
                q.humidity = 1.0f;
        }
    }

    // This changes the overall delivery of the Humidity to be evenly spread everywhere.
    public static void Redistribute(ref List<Characteristics.Corner> locations)
    {
        locations.OrderBy(x => x.humidity);
        for (int i = 0; i < locations.Count; i++)
        {
            locations[i].humidity = (float)i / (locations.Count - 1);
        }
    }

    // Polygon Humidity is the average of the Humidity at corners
    public static void AssignPolygon(ref Map map)
    {
        float sumHumidity;
        foreach (var p in map.centers)
        {
            sumHumidity = 0.0f;
            foreach (var q in p.corners)
            {
                if (q.humidity > 1.0) q.humidity = 1.0f;
                sumHumidity += q.humidity;
            }
            p.humidity = sumHumidity / p.corners.Count;
        }
    }

}
