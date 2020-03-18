using System.Collections.Generic;

public class CheckingScript
{
    public static Dictionary<string, string> displayColors = new Dictionary<string, string>()
    {
        // Features
        { "SEA", "#1919B7" },
        { "SEALINE", "#33335a" },
        { "LAKE", "#3497D7" },
        { "RIVER", "#225588" },
        { "MARSH", "#14708E"},
        { "BEACH", "#C6B382"},

        // Terrain
        { "SNOW", "#ffffff"},
        { "BARE", "#888888"},
        { "TAIGA", "#6E804A"},
        { "TEMPERATE_RAIN_FOREST", "#448855"},
        { "TEMPERATE_DECIDUOUS_FOREST", "#34791F"},
        { "GRASSLAND", "#73BC45"},
        { "SUBTROPICAL_DESERT", "#D9D87E"},
        { "TROPICAL_RAIN_FOREST", "#337755"},
        { "TROPICAL_SEASONAL_FOREST", "#478A36"}
    };

    private const float LakeRarity = 0.35f; //The rarity of lakes, the higher the number the less chance of spawning a lake

    // Determine polygon and corner types: scean, sealine, land.
    public static void AssignSeaSealandAndLand(ref Map data)
    {
        // Compute polygon attributes 'ocean' and 'water' based on the
        // corner attributes. Count the water corners per
        // polygon. Oceans are all polygons connected to the edge of the
        // map. In the first pass, mark the edges of the map as ocean;
        // in the second pass, mark any water-containing polygon
        // connected an ocean as ocean.
        Queue<Characteristics.Center> queue = new Queue<Characteristics.Center>();
        int numWater;

        foreach (var p in data.centers)
        {
            numWater = 0;
            foreach (var q in p.corners)
            {
                if (q.border)
                {
                    p.border = true;
                    p.sea = true;
                    q.water = true;
                    queue.Enqueue(p);
                }
                if (q.water)
                    numWater += 1;
            }
            p.water = (p.sea || numWater >= p.corners.Count * LakeRarity);
        }

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();

            foreach (var r in p.neighbours)
            {
                if (r.water && !r.sea)
                {
                    r.sea = true;
                    queue.Enqueue(r);
                }
            }
        }

        int numSea = 0;
        int numLand = 0;

        // Based on the polygons next to it it will be a sealine. eg. if it has at least one
        // sea polygon next to it and one land polygon then this is a sealine
        foreach (var p in data.centers)
        {
            numSea = 0;
            numLand = 0;
            foreach (var r in p.neighbours)
            {
                numSea += r.sea ? 1 : 0;
                numLand += r.water ? 0 : 1;
            }
            p.sealine = (numSea > 0) && (numLand > 0);
        }


        // if all of the corners of the polygons are connected then it is the sea
        // if all of the conncected are land it is assigned as land, however, if none of this
        // it is a sealine 
      
        foreach (var q in data.corners)
        {
            numSea = 0;
            numLand = 0;
            foreach (var p in q.touches)
            {
                numSea += p.sea ? 1 : 0;
                numLand += p.water ? 0 : 1;
            }
            q.sea = (numSea == q.touches.Count);
            q.sealine = (numSea > 0) && (numLand > 0);
            q.water = q.border || ((numLand != q.touches.Count) && !q.sealine);
        }
    }


    public static void AssignHabitat(ref Map map)
    {
        foreach (var p in map.centers)
        {
            string biomeName = GetHabitat(p);
            if (!map.biomes.Contains(biomeName))
                map.biomes.Add(biomeName);
            p.biome = biomeName;
        }
    }

    // Assigning habitats to each polygon meaning that if there is sealine or sea next to it, 
    // then it is habitat otherwise it will depend on the humidity. it will check whether it is high low or medium humidity. 
    private static string GetHabitat(Characteristics.Center p)
    {
        if (p.sea)
        {
            return "SEA";
        }
        else if (p.water)
        {
            if (p.built < 0.1f)
                return "MARSH";
            if (p.built > 0.8f)
                return "ICE";
            return "LAKE";
        }
        else if (p.sealine)
        {
            return "BEACH";
        }
        else if (p.built > 0.8f)
        {
            if (p.humidity > 0.50f) return "SNOW";
            else if (p.humidity > 0.33f) return "TUNDRA";
            else if (p.humidity > 0.16f) return "BARE";
            else return "SCORCHED";
        }
        else if (p.built > 0.6f)
        {
            if (p.humidity > 0.66f) return "TAIGA";
            else if (p.humidity > 0.33f) return "SHRUBLAND";
            else return "TEMPERATE_DESERT";
        }
        else if (p.built > 0.3f)
        {
            if (p.humidity > 0.83f) return "TEMPERATE_RAIN_FOREST";
            else if (p.humidity > 0.50f) return "TEMPERATE_DECIDUOUS_FOREST";
            else if (p.humidity > 0.16f) return "GRASSLAND";
            else return "TEMPERATE_DESERT";
        }
        else
        {
            if (p.humidity > 0.66f) return "TROPICAL_RAIN_FOREST";
            else if (p.humidity > 0.33f) return "TROPICAL_SEASONAL_FOREST";
            else if (p.humidity > 0.16f) return "GRASSLAND";
            else return "SUBTROPICAL_DESERT";
        }
    }
}
