using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

public class UnitSpawner : MonoBehaviour
{
    public int xGridCount;
    public int zGridCount;
    public float baseOffset;
    public float xPadding;
    public float zPadding;
    public int maxEntitiesToSpawn;
    public GameObject prefabToSpawn;
    public float interval;
    public float3 offset;

    public int destinationDistanceZAxis;
    public int minSpeed;
    public int maxSpeed;
    public float minDistance;

    private int toatalSpawned;
    private EntityManager entitymanager;
    private float elapsedTime;
    private float3 currentPosition;
    private BlobAssetStore bas;
    private GameObjectConversionSettings gocs;
    private Entity convertedEntity;

    // Start is called before the first frame update
    private void Start()
    {
        toatalSpawned = 0;
        entitymanager = World.DefaultGameObjectInjectionWorld.EntityManager;
        currentPosition = this.transform.position;
        bas = new BlobAssetStore();
        gocs = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, bas);
        convertedEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabToSpawn, gocs);
    }

    // Update is called once per frame
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= interval)
        {
            elapsedTime = 0;
            for (int i = 0; i < xGridCount; i++)
            {
                for (int j = 0; j < zGridCount; j++)
                {
                    if (toatalSpawned == maxEntitiesToSpawn)
                    {
                        break;
                    }
                    Entity e;
                    if (i == 0)
                    {
                        e = convertedEntity;
                    }
                    else
                    {
                        e = entitymanager.Instantiate(convertedEntity);
                    }
                    float3 pos = new float3(i * xPadding, baseOffset, j * zPadding) + currentPosition;
                    entitymanager.SetComponentData(e, new Translation
                    {
                        Value = pos
                    });

                    //Navigation
                    entitymanager.AddComponentData(e, new NavAgent_Component
                    {
                        fromLocation = pos,
                        toLocation = new float3(pos.x, pos.y, pos.z + destinationDistanceZAxis),
                        routed = false
                    });
                    entitymanager.AddBuffer<NavAgent_Buffer>(e);

                    //Movement
                    entitymanager.AddComponentData(e, new UnitComponentData
                    {
                        speed = UnityEngine.Random.Range(minSpeed, maxSpeed),
                        currentBufferIndex = 0,
                        minDistance = minDistance,
                        offset = offset
                    });

                    toatalSpawned++;
                }
            }
        }
    }

    private void OnDestroy()
    {
        bas.Dispose();
    }
}
