using System;

using TMPro;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region EXPOSED_FIELDS
    public Miners miners;
    public TMP_InputField weightInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    #endregion

    private Vector2Int nodePos;
    private int netWeight;

    public void UpdateWeight()
    {
        nodePos = new Vector2Int(Convert.ToInt32(xInput.text), Convert.ToInt32(yInput.text));
        netWeight = Convert.ToInt32(weightInput.text);
            
        miners.UpdateWeight(nodePos, netWeight);
    }
    
    public void SpawnMiner()
    {
        miners.SpawnMiner();
    }

    public void AbruptExit()
    {
        miners.AbruptExit();
    }
}