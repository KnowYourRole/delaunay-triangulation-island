using System.Collections.Generic;
using UnityEngine;

namespace Characteristics
{
    // stores information about different scripts that are refered to later 
    public class Corner
    {
        public int id;
        public Vector3 pos;
        public bool border;
        public bool water;
        public bool sea;
        public bool sealine;
        public int river;
        public float built;
        public float humidity;
        public List<Corner> adjacent;
        public List<Center> touches;
        public List<Border> protrudes;
    }

    public class Center
    {
        public int id;
        public Vector3 pos;
        public bool border;
        public bool sea;
        public bool water;
        public bool sealine;
        public float built;
        public float humidity;
        public string biome;
        public List<Center> neighbours;
        public List<Border> borders;
        public List<Corner> corners;

        public List<Vector3> vertices;
        public List<int> triangles;
    }

    public class Border
    {
        public int id;
        public Center d0, d1;
        public Corner v0, v1;
        public Vector3 midPoint;
    }
}