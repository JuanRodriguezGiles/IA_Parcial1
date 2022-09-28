using System;
using System.Collections.Generic;

using UnityEngine;

public class AbruptReturn : FSMAction
{
    #region PRIVATE_FIELDS
    private readonly Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath;
    private Action<Vector2Int> onUpdateTarget;
    private readonly Func<Vector2> onGetPos;
    private Func<float> onGetDeltaTime;

    private Vector3 currentDestination;
    private readonly Vector2Int deposit;
    private List<Vector2Int> path;
    private Vector2 miner;
    private int posIndex;
    private bool reached = false;
    #endregion

    #region CONSTRUCTOR
    public AbruptReturn(Action<int> onSetFlag, Func<float> onGetDeltaTime, Func<Vector2> onGetPos, Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath, Action<Vector2Int> onUpdateTarget, Vector2Int deposit)
    {
        this.onSetFlag = onSetFlag;
        this.onGetDeltaTime = onGetDeltaTime;
        this.onGetPos = onGetPos;
        this.onGetPath = onGetPath;
        this.onUpdateTarget = onUpdateTarget;
        this.deposit = deposit;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
        if (reached) return;
        
        miner = onGetPos.Invoke();

        if (path == null)
        {
            path = onGetPath.Invoke(new Vector2Int((int)miner.x, (int)miner.y), deposit);

            posIndex = 0;

            currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);

            onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
        }
        else if (Vector2.Distance(currentDestination, miner) < 0.1f)
        {
            posIndex++;

            if (posIndex >= path.Count - 1)
            {
                path = null;
                reached = true;
                return;
            }

            currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);
            onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
        }
    }

    public override void AbruptExit()
    {
        reached = false;
        path = null;
    }
    #endregion
}