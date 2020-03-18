using UnityEngine;

public class EnvironmentConstructionScript
{
    public enum FunctionType
    {
        Radial
    }

    private float width;
    private float height;
    private int bumps;
    private float startAngle;
    private float dipAngle;
    private float dipWidth;
    private FunctionType functionType;
    
    private const float ISLAND_FACTOR = 1.7f;
    public EnvironmentConstructionScript(int seed, float width, float height, FunctionType functionType)
    {
        this.width = width;
        this.height = height;
        this.functionType = functionType;

        Random.InitState(seed);
        
        bumps = Random.Range(1, 8);
        startAngle = Random.Range(0f, 2 * Mathf.PI);
        dipAngle = Random.Range(0f, 2 * Mathf.PI);
        dipWidth = Random.Range(0.2f, 0.6f);
    }

    public bool IsInside(Vector2 pos)
    {
        switch (functionType)
        {
            case FunctionType.Radial:   //check if all objects are inside each other and if so break
                return IsInsideRadial(pos);
            default:
                break;
        }
        return true;
    }

    private bool IsInsideRadial(Vector2 pos)
    {
        pos = new Vector2(2 * (pos.x / width - 0.5f), 2 * (pos.y / height - 0.5f));

        float angle = Mathf.Atan2(pos.y,pos.x);
        float length = 0.5f * (Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y)) + pos.magnitude);

        float r1 = 0.5f + 0.40f * Mathf.Sin(startAngle + bumps * angle + Mathf.Cos((bumps + 3) * angle));
        float r2 = 0.7f - 0.20f * Mathf.Sin(startAngle + bumps * angle - Mathf.Sin((bumps + 2) * angle));
        if (Mathf.Abs(angle - dipAngle) < dipWidth
            || Mathf.Abs(angle - dipAngle + 2 * Mathf.PI) < dipWidth
            || Mathf.Abs(angle - dipAngle - 2 * Mathf.PI) < dipWidth)
        {
            r1 = r2 = 0.2f;
        }
        return (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
    }


   

}
