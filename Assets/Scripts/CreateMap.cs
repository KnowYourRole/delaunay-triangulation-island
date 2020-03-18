using UnityEngine;

public class CreateMap : MonoBehaviour
{
    public Vector2 parameters = new Vector2(200, 200);

    public bool autoUpdate = false;

    public float radius = 10;

    public int generations = 12;
    public int relaxationCount = 2;
    public int regionCount = 40;

    private EnvironmentConstructionScript.FunctionType islandFunction;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material baseMaterial;
    public AnimationCurve heightMap;

    public float heightFactor = 20f;

    private int newGenerations;

    public PolygonList.FaceType cellType = PolygonList.FaceType.Random;
    private PolygonList.FaceType newCellTypes1;
    private PolygonList.FaceType newCellTypes2;
    private PolygonList.FaceType newCellTypes3;
    private PolygonList.FaceType newCellTypes4;


    private Map mapData;
    // generates a new terrain with random seeds 
    public void NewGeneration()
    {
        newGenerations = Random.Range(0, 10000);
        generations = newGenerations;
        Random.InitState(generations);
        GenerateMap map = new GenerateMap();
        Create();
    }
    // generates new random cell type, used for the buttons 
    public void CellTypes1()
    {
        newCellTypes1 = PolygonList.FaceType.Random;
      cellType = newCellTypes1;
        Create();
    }
    // generates new square cell type, used for the buttons 
    public void CellTypes2()
    {
        newCellTypes2 = PolygonList.FaceType.Square;

        cellType = newCellTypes2;
        Create();
    }
    // generates new hexagon cell type, used for the buttons 
    public void CellTypes3()
    {
        newCellTypes3 = PolygonList.FaceType.Hexagon;
        cellType = newCellTypes3;
        Create();
    }
    // generates new poisson disc cell type, used for the buttons 
    public void CellTypes4()
    {
        newCellTypes4 = PolygonList.FaceType.PoisonDisc;
        cellType = newCellTypes4;
        Create();
    }

    // generates the whole map 
    public void Create()
    {
        Random.InitState(generations);
        GenerateMap map = new GenerateMap();
        mapData = map.Generate(parameters, generations, cellType, islandFunction, heightFactor, heightMap, regionCount, relaxationCount, radius);
        GenerateMesh meshData = map.GenerateMeshData();
        meshFilter.mesh = meshData.CreateMesh();

        meshRenderer.sharedMaterials = new Material[mapData.biomes.Count];
        Material[] materials = new Material[mapData.biomes.Count];
        for (int i = 0; i < mapData.biomes.Count; i++)
        {
            Material material = new Material(baseMaterial);
            ColorUtility.TryParseHtmlString(CheckingScript.displayColors[mapData.biomes[i]], out Color biome);
            material.color = biome;
            material.name = mapData.biomes[i];
            materials[i] = material;
        }

        meshRenderer.sharedMaterials = materials;
    }
   }
