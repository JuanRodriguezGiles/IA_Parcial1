using System;
using System.Collections.Generic;

using UnityEngine;

public class GoToMine : FSMAction
{
    #region PRIVATE_FIELDS
    private readonly Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath;
    private Action<Vector2Int> onUpdateTarget;
    private Action<Vector2Int> onUpdateMine;
    private readonly Func<Vector2> onGetPos;
    private Func<Vector2Int> onGetMine;
    private Func<bool> rePath;

    private Vector3 currentDestination;
    private List<Vector2Int> path;
    private Vector2Int mine;
    private Vector2 miner;
    private int posIndex;
    #endregion

    #region CONSTRUCTOR
    public GoToMine(Action<int> onSetFlag, Func<Vector2> onGetPos, Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath, Action<Vector2Int> onUpdateTarget, Func<Vector2Int> onGetMine, Action<Vector2Int> onUpdateMine,
        Func<bool> rePath)
    {
        this.onSetFlag = onSetFlag;
        this.onGetPos = onGetPos;
        this.onGetPath = onGetPath;
        this.onUpdateTarget = onUpdateTarget;
        this.onGetMine = onGetMine;
        this.onUpdateMine = onUpdateMine;
        this.rePath = rePath;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
        miner = onGetPos.Invoke();

        if (path == null)
        {
            mine = onGetMine.Invoke();
            onUpdateMine?.Invoke(mine);

            path = onGetPath.Invoke(new Vector2Int((int)miner.x, (int)miner.y), mine);

            posIndex = 0;

            currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);

            onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
        }
        else if (Vector2.Distance(currentDestination, miner) < 0.1f)
        {
            if (rePath.Invoke())
            {
                path = onGetPath.Invoke(new Vector2Int((int)miner.x, (int)miner.y), mine);

                posIndex = 0;

                currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);

                onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
            }
            else
            {
                posIndex++;

                if (posIndex >= path.Count - 1)
                {
                    path = null;
                    onSetFlag?.Invoke((int)Flags.OnReachMine);
                    return;
                }

                currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);
                onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
            }
        }
    }

    public override void AbruptExit()
    {
        path = null;
    }
    #endregion
}