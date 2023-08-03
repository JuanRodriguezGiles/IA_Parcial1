using System;
using System.Collections.Generic;

using UnityEngine;

public class AbruptReturn : FSMAction
{
    #region PRIVATE_FIELDS
    private readonly Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath;
    private readonly Action<Vector2Int> onUpdateTarget;
    private readonly Func<Vector2> onGetPos;
    private Action stopFlocking;

    private Vector3 currentDestination;
    private readonly Vector2Int rest;
    private List<Vector2Int> path;
    private Vector2 miner;
    private int posIndex;
    private bool reached = false;
    #endregion

    #region CONSTRUCTOR
    public AbruptReturn(Action<int> onSetFlag, Func<Vector2> onGetPos, Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath, Action<Vector2Int> onUpdateTarget, Action stopFlocking, Vector2Int rest)
    {
        this.onSetFlag = onSetFlag;
        this.onGetPos = onGetPos;
        this.onGetPath = onGetPath;
        this.onUpdateTarget = onUpdateTarget;
        this.stopFlocking = stopFlocking;
        this.rest = rest;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
        if (reached) return;
        
        miner = onGetPos.Invoke();

        if (path == null)
        {
            path = onGetPath.Invoke(new Vector2Int((int)miner.x, (int)miner.y), rest);

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
                reached = false;
                stopFlocking?.Invoke();
                onSetFlag?.Invoke((int)Flags.OnIdle);
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