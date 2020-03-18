using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///     http://gregschlom.com/devlog/2014/06/29/Poisson-disc-sampling-Unity.html
///     http://devmag.org.za/2009/05/03/poisson-disk-sampling/
///     http://bl.ocks.org/mbostock/19168c663618b7f07158
///     http://bl.ocks.org/mbostock/dbb02448b0f93e4c82c3
public class DiscPoisonShape
{
    private const int k = 30;  // This is the number of attempts that a person who is playing the project can make before the sample becoming inactive. 

    private readonly Rect rect;
    private readonly float radius2;  // squared radius 
    private readonly float cellSize;
    private Vector2[,] grid;
    private List<Vector2> activeSamples = new List<Vector2>();


    /// Sample is creates with following variables 

    /// radius: each sample will be at least `radius` units away from any other sample, and at most 2 * `radius`.
    /// height:  y coordinate will be between [0, height]
    /// width:   x coordinate will be between [0, width]

    public DiscPoisonShape(float width, float height, float radius)
    {
        rect = new Rect(0, 0, width, height);
        radius2 = radius * radius;
        cellSize = radius / Mathf.Sqrt(2);
        grid = new Vector2[Mathf.CeilToInt(width / cellSize),
                           Mathf.CeilToInt(height / cellSize)];
    }

    public IEnumerable<Vector2> Samples()
    {
        // First sample is created randomly
        yield return AddSample(new Vector2(Random.value * rect.width, Random.value * rect.height));

        while (activeSamples.Count > 0)
        {

            // random active sample
            int i = (int)Random.value * activeSamples.Count;
            Vector2 sample = activeSamples[i];

            // random candidates between radius, 2 * radius
            bool found = false;
            for (int j = 0; j < k; ++j)
            {

                float angle = 2 * Mathf.PI * Random.value;
                float r = Mathf.Sqrt(Random.value * 3 * radius2 + radius2); 
                Vector2 candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Adds sample candidates if they are in the rect and farer than 2*radius
                if (rect.Contains(candidate) && IsFarEnough(candidate))
                {
                    found = true;
                    yield return AddSample(candidate);
                    break;
                }
            }

            // If there are unvalid candidates after the attempts (k) then the sample will be removed from the queue 
            if (!found)
            {
                activeSamples[i] = activeSamples[activeSamples.Count - 1];
                activeSamples.RemoveAt(activeSamples.Count - 1);
            }
        }
    }

    private bool IsFarEnough(Vector2 sample)
    {
        GridPos pos = new GridPos(sample, cellSize);

        int xmin = Mathf.Max(pos.x - 2, 0);
        int ymin = Mathf.Max(pos.y - 2, 0);
        int xmax = Mathf.Min(pos.x + 2, grid.GetLength(0) - 1);
        int ymax = Mathf.Min(pos.y + 2, grid.GetLength(1) - 1);

        for (int y = ymin; y <= ymax; y++)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                Vector2 s = grid[x, y];
                if (s != Vector2.zero)
                {
                    Vector2 d = s - sample;
                    if (d.x * d.x + d.y * d.y < radius2) return false;
                }
            }
        }

        return true;
    }
    /// This sample is added to the queue and grid before returning it 
    private Vector2 AddSample(Vector2 sample)
    {
        activeSamples.Add(sample);
        GridPos pos = new GridPos(sample, cellSize);
        grid[pos.x, pos.y] = sample;
        return sample;
    }
    /// calculate the x and y indices of a sample 
    private struct GridPos
    {
        public int x;
        public int y;

        public GridPos(Vector2 sample, float cellSize)
        {
            x = (int)(sample.x / cellSize);
            y = (int)(sample.y / cellSize);
        }
    }
}