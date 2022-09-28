using UnityEngine;

using UnityEditor;

public class GameManager : Singleton<GameManager>
{
    #region EXPOSED_FIELDS
    public Miners miners;
    public Vector2Int nodePos;
    public int newWeight;
    #endregion
}

[CustomEditor(typeof(GameManager))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager myScript = (GameManager)target;
        if (GUILayout.Button("Spawn Miner"))
        {
            myScript.miners.SpawnMiner();
        }

        if (GUILayout.Button("Update Weight"))
        {
            myScript.miners.UpdateWeight(GameManager.Instance.nodePos, GameManager.Instance.newWeight);
        }
    }
}