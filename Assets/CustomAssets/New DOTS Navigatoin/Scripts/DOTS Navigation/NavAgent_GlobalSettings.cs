using UnityEngine;
using Unity.Mathematics;

public class NavAgent_GlobalSettings : MonoBehaviour
{
    public static NavAgent_GlobalSettings instance { get; private set; }
    public int maxPathSize;
    public int maxEntitiesRoutedPerFrame;
    public int maxPathNodePoolSize;
    public int maxIterations;
    public float3 extents;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
