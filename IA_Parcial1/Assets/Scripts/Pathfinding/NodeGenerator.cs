using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    public Vector2Int mapSize;
    private Node[] map;

    private Pathfinding pathfinding;

    // Start is called before the first frame update
    void Start()
    {
        pathfinding = new Pathfinding();
        NodeUtils.MapSize = mapSize;
        map = new Node[mapSize.x * mapSize.y];
        int ID = 0;
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                map[ID] = new Node(ID, new Vector2Int(j, i));
                ID++;
            }
        }

        map[NodeUtils.PositionToIndex(new Vector2Int(1, 0))].state = Node.NodeState.Obstacle;
        map[NodeUtils.PositionToIndex(new Vector2Int(3, 1))].state = Node.NodeState.Obstacle;
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 1))].SetWeight(2);
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 2))].SetWeight(2);
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 3))].SetWeight(2);
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 4))].SetWeight(2);
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 5))].SetWeight(2);
        map[NodeUtils.PositionToIndex(new Vector2Int(1, 6))].SetWeight(2);
        
        List<Vector2Int> path = pathfinding.GetPath(map, map[NodeUtils.PositionToIndex(new Vector2Int(0, 0))], map[NodeUtils.PositionToIndex(new Vector2Int(8, 3))]);

        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log(path[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if (map == null)
            return;
       
        GUIStyle style = new GUIStyle() { fontSize = 15 };
        
        foreach (Node node in map)
        {
            Vector3 worldPosition = new Vector3((float)node.position.x, (float)node.position.y, 0.0f);

            Gizmos.color = node.state == Node.NodeState.Obstacle ? Color.red : Color.green;
            Gizmos.DrawWireSphere(worldPosition, 0.2f);
            Handles.Label(worldPosition, node.position.ToString(), style);
        }
    }
}