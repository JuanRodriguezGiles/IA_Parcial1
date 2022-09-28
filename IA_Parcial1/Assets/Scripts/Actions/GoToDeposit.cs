using System;
using System.Collections.Generic;

using UnityEngine;

public class GoToDeposit : FSMAction
{
    #region PRIVATE_FIELDS
    private readonly Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath;
    private Action<Vector2Int> onUpdateTarget;
    private readonly Func<Vector2> onGetPos;
    private Func<bool> rePath;

    private Vector3 currentDestination;
    private readonly Vector2Int deposit;
    private List<Vector2Int> path;
    private Vector2 miner;
    private int posIndex;
    #endregion

    #region CONSTRUCTOR
    public GoToDeposit(Action<int> onSetFlag, Func<Vector2> onGetPos, Func<Vector2Int, Vector2Int, List<Vector2Int>> onGetPath, Action<Vector2Int> onUpdateTarget, Vector2Int deposit, Func<bool> rePath)
    {
        this.onSetFlag = onSetFlag;
        this.onGetPos = onGetPos;
        this.onGetPath = onGetPath;
        this.onUpdateTarget = onUpdateTarget;
        this.deposit = deposit;
        this.rePath = rePath;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
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
            if (rePath.Invoke())
            {
                path = onGetPath.Invoke(new Vector2Int((int)miner.x, (int)miner.y), deposit);

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
                    onSetFlag?.Invoke((int)Flags.OnReachDeposit);
                    return;
                }

                currentDestination = new Vector3(path[posIndex].x, path[posIndex].y, 0);
                onUpdateTarget?.Invoke(new Vector2Int((int)currentDestination.x, (int)currentDestination.y));
            }
        }
    }

    public override void AbruptExit()
    {
    }
    #endregion
}