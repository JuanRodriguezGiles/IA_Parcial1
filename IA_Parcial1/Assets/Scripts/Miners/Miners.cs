using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;

using Random = UnityEngine.Random;

public class Miners : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [Header("Config")]
    [SerializeField] private VoronoiHandler voronoiHandler;
    [SerializeField] private Vector3Int minerSpawnPos;
    [SerializeField] private int minesCount;
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private Transform minesParent;
    [SerializeField] private Transform wallsParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject depositGo;
    [SerializeField] private GameObject minerGo;
    [SerializeField] private GameObject mineGo;
    [SerializeField] private GameObject restGo;
    
    [Header("Data")]
    [SerializeField] private List<Vector2Int> buildings = new();
    [SerializeField] private List<GameObject> mines;
    #endregion

    #region PRIVATE_FIELDS
    private ConcurrentBag<Miner> miners = new();
    private ParallelOptions parallelOptions;
    private Action onUpdateWeight;
    private Vector2Int depositPos;
    private Vector2Int restPos;
    private float deltaTime;
    private List<Vector2Int> minesPos = new List<Vector2Int>();
    [SerializeField] private Node[] map;
    #endregion

    #region UNITY_CALLS
    private void Start()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 6 };

        depositPos = new Vector2Int((int)depositGo.transform.position.x, (int)depositGo.transform.position.y);
        restPos = new Vector2Int((int)restGo.transform.position.x, (int)restGo.transform.position.y);

        InitBuildings();
        InitMap();
        voronoiHandler.Config();
        
        for (int i = 0; i < minesCount; i++)
        {
            SpawnMine();
        }

        UpdateSectors();
    }

    private void Update()
    {
        deltaTime = Time.deltaTime;

        foreach (var miner in miners)
        {
            if (miner.updatePos)
            {
                miner.transform.position = miner.currentPos;
                miner.updatePos = false;
            }
        }

        Parallel.ForEach(miners, parallelOptions, miner => { miner.UpdateMiner(); });
    }
    
    private void OnDrawGizmos()
    {
        if (map == null)
            return;
        
        foreach (Node node in map)
        {
            Vector3 worldPosition = new Vector3(node.position.x, node.position.y, 0.0f);

            Gizmos.color = node.state == Node.NodeState.Obstacle ? Color.red : Color.green;
            Gizmos.DrawWireSphere(worldPosition, 0.2f);
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void SpawnMiner()
    {
        var go = Instantiate(minerGo, minerSpawnPos, Quaternion.identity, transform);
        var miner = go.GetComponent<Miner>();
        miner.Init(depositPos, restPos, miner.transform.position, GetDeltaTime, GetMine, OnEmptyMine, ref onUpdateWeight, buildings, minesPos);

        miners.Add(miner);
    }

    public void UpdateWeight(Vector2Int nodePos, int nodeWeight)
    {
        Parallel.ForEach(miners, parallelOptions, miner => { miner.UpdateWeight(nodePos, nodeWeight); });
    }

    public void RandomWeight()
    {
        
    }

    public void AbruptExit()
    {
        Parallel.ForEach(miners, parallelOptions, miner => { miner.ExitMiner(); });
    }
    #endregion

    #region PRIVATE_METHODS
    private void InitBuildings()
    {
        buildings.Add(depositPos);

        for (int i = 0; i < mines.Count; i++)
        {
            Vector2Int pos = new Vector2Int((int)mines[i].transform.position.x, (int)mines[i].transform.position.y);
            buildings.Add(pos);
        }

        for (int i = 0; i < wallsParent.childCount; i++)
        {
            Transform wall = wallsParent.GetChild(i);
            Vector2Int pos = new Vector2Int((int)wall.transform.position.x, (int)wall.transform.position.y);
            buildings.Add(pos);
        }
    }

    private void InitMap()
    {
        map = new Node[mapSize.x * mapSize.y];
        NodeUtils.MapSize = new Vector2Int(mapSize.x, mapSize.y);
        int id = 0;

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                map[id] = new Node(id, new Vector2Int(j, i));
                map[id].SetWeight(Random.Range(1, 6));
                
                for (int k = 0; k < buildings.Count; k++)
                {
                    if (map[id].position == buildings[k])
                    {
                        map[id].state = Node.NodeState.Obstacle;
                    }
                }

                id++;
            }
        }
    }

    private void SpawnMine()
    {
        int x = Random.Range(1, NodeUtils.MapSize.x - 1);
        int y = Random.Range(1, NodeUtils.MapSize.y - 1);
        Vector2Int pos = new Vector2Int(x, y);

        for (int i = 0; i < buildings.Count; i++)
        {
            if (pos == buildings[i])
            {
                SpawnMine();
                return;
            }
        }

        Vector3 posVec3 = new Vector3(pos.x, pos.y, 0);
        var go = Instantiate(mineGo, posVec3, Quaternion.identity, minesParent);

        mines.Add(go);
        buildings.Add(pos);
        minesPos.Add(pos);
    }

    private float GetDeltaTime()
    {
        return deltaTime;
    }

    private Vector2Int GetMine()
    {
        int index = Random.Range(0, mines.Count);

        Vector2Int pos = new Vector2Int((int)mines[index].transform.position.x, (int)mines[index].transform.position.y);

        return pos;
    }
    
    private Vector2Int GetMine(Vector2 minerPos)
    {
        return voronoiHandler.GetNearestMine(minerPos);
    }

    private void OnEmptyMine(Vector2Int minePos)
    {
        Vector2Int pos;
        for (int i = 0; i < mines.Count; i++)
        {
            pos = new Vector2Int((int)mines[i].transform.position.x, (int)mines[i].transform.position.y);
            if (minePos == pos)
            {
                buildings.Remove(pos);
                minesPos.Remove(pos);
                Destroy(mines[i]);
                mines.RemoveAt(i);
                UpdateSectors();
                
                Parallel.ForEach(miners, parallelOptions, miner =>
                {
                    if (miner.currentMine == minePos)
                    {
                        miner.UpdateMine(GetMine());
                    }
                });
                break;
            }
        }
    }

    private Node[] GetMap()
    {
        return map;
    }

    private void UpdateSectors()
    {
        List<Vector2> minesPos = new List<Vector2>();
        foreach (var mine in mines)
        {
            minesPos.Add(mine.transform.position);
        }
        voronoiHandler.UpdateSectors(minesPos);
    }
    #endregion
}