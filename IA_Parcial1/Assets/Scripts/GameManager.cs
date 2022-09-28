using UnityEngine;

using UnityEditor;

public class GameManager : MonoBehaviour
{
    #region EXPOSED_FIELDS
    public Miners miners;
    #endregion
}

[CustomEditor(typeof(GameManager))]
public class ObjectBuilderEditor : Editor
{
    private Vector2Int nodePos;
    private int netWeight;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager myScript = (GameManager)target;
        if (GUILayout.Button("Spawn Miner"))
        {
            myScript.miners.SpawnMiner();
        }
        
        nodePos = EditorGUILayout.Vector2IntField("Node Pos", nodePos);
        netWeight = EditorGUILayout.IntField("Weight", netWeight);

        if (GUILayout.Button("Update Weight"))
        {
            myScript.miners.UpdateWeight(nodePos, netWeight);
        }
    }
}