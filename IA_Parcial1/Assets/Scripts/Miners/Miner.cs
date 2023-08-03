using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

public class Miner : MonoBehaviour
{
    #region PRIVATE_FIELDS
    private Action<Vector2Int> onEmptyMine;
    private FlockingMiners flockingMiners;
    private Pathfinding pathfinding;
    private FSM fsm;
    private Node[] map;
    #endregion

    #region EXPOSED_FIELDS
    public Vector2 currentPos;
    public Vector2Int currentMine;
    public bool updatePos;
    private bool updatePath = false;
    #endregion

    #region PUBLIC_METHODS
    public void Init(Vector2Int deposit, Vector2Int rest, Vector2 currentPos, Func<float> onGetDeltaTime, Func<Vector2, Vector2Int> onGetMine, Action<Vector2Int> onEmptyMine, ref Action onUpdateWeight, List<Vector2Int> buildings, List<Vector2Int> mines)
    {
        InitMap(buildings, mines);
        
        pathfinding = new Pathfinding();
        this.currentPos = currentPos;
        //--------------------------------------------------------------------------------
        fsm = new FSM((int)States._Count, (int)Flags._Count);

        //Base States
        fsm.SetRelation((int)States.GoToMine, (int)Flags.OnReachMine, (int)States.Mining);
        fsm.SetRelation((int)States.Mining, (int)Flags.OnFullInventory, (int)States.GoToDeposit);
        fsm.SetRelation((int)States.GoToDeposit, (int)Flags.OnReachDeposit, (int)States.GoToMine);
        fsm.SetRelation((int)States.GoToRest, (int)Flags.OnIdle, (int)States.Idle);

        //Early exit states
        fsm.SetRelation((int)States.Mining, (int)Flags.OnAbruptReturn, (int)States.GoToRest);
        fsm.SetRelation((int)States.GoToMine, (int)Flags.OnAbruptReturn, (int)States.GoToRest);
        fsm.SetRelation((int)States.Idle, (int)Flags.OnGoBackToWork, (int)States.GoToMine);
        fsm.SetRelation((int)States.GoToRest, (int)Flags.OnGoBackToWork, (int)States.GoToMine);

        //Behaviours
        fsm.AddBehaviour((int)States.Idle, new Idle(fsm.SetFlag), () => { fsm.SetFlag((int)Flags.OnGoBackToWork); });
        fsm.AddBehaviour((int)States.Mining, new Mine(fsm.SetFlag, onGetDeltaTime, OnEmptyMine,StopMovement), () => { fsm.SetFlag((int)Flags.OnAbruptReturn); });
        fsm.AddBehaviour((int)States.GoToMine, new GoToMine(fsm.SetFlag, GetPos, GetPath, UpdateTarget, onGetMine, UpdateMine, RePath), () => { fsm.SetFlag((int)Flags.OnAbruptReturn); });
        fsm.AddBehaviour((int)States.GoToDeposit, new GoToDeposit(fsm.SetFlag, GetPos, GetPath, UpdateTarget, deposit, RePath));
        fsm.AddBehaviour((int)States.GoToRest, new AbruptReturn(fsm.SetFlag, GetPos, GetPath, UpdateTarget, StopMovement, rest), () => { fsm.SetFlag((int)Flags.OnGoBackToWork); });

        fsm.ForceCurrentState((int)States.GoToMine);
        //--------------------------------------------------------------------------------
        flockingMiners = GetComponent<FlockingMiners>();
        flockingMiners.Init(UpdatePos, GetPos);

        this.onEmptyMine = onEmptyMine;

        onUpdateWeight = OnUpdateWeight;
    }

    public void UpdateMiner()
    {
        fsm.Update();
    }

    public void ExitMiner()
    {
        fsm.Exit();
    }
    
    public void UpdateWeight(Vector2Int nodePos, int nodeWeight)
    {
        for (int i = 0; i < map.Length; i++)
        {
            if (map[i].position == nodePos)
            {
                map[i].SetWeight(nodeWeight);
                updatePath = true;
                return;
            }
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void UpdatePos(Vector2 newPos)
    {
        updatePos = true;
        currentPos = newPos;
    }

    private void UpdateTarget(Vector2Int newTarget)
    {
        flockingMiners.ToggleFlocking(true);
        flockingMiners.UpdateTarget(newTarget);
    }

    private void StopMovement()
    {
        flockingMiners.ToggleFlocking(false);
    }
    
    private Vector2 GetPos()
    {
        return currentPos;
    }

    private List<Vector2Int> GetPath(Vector2Int origin, Vector2Int destination)
    {
        updatePath = false;
        return pathfinding.GetPath(map, map[NodeUtils.PositionToIndex(origin)], map[NodeUtils.PositionToIndex(destination)]);
    }

    public void UpdateMine(Vector2Int minePos)
    {
        currentMine = minePos;
        updatePath = true;
    }

    private void OnEmptyMine()
    {
        updatePath = true;
        onEmptyMine?.Invoke(currentMine);
    }

    private void OnUpdateWeight()
    {
        updatePath = true;
    }

    private bool RePath()
    {
        return updatePath;
    }

    private void InitMap(List<Vector2Int> buildings, List<Vector2Int> mines)
    {
        map = new Node[50 * 50];
        NodeUtils.MapSize = new Vector2Int(50, 50);
        int id = 0;

        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 50; j++)
            {
                map[id] = new Node(id, new Vector2Int(j, i));
                map[id].SetWeight(Random.Range(1, 6));

                for (int k = 0; k < buildings.Count; k++)
                {
                    if (map[id].position == buildings[k] && !mines.Contains(buildings[k])) 
                    {
                        map[id].state = Node.NodeState.Obstacle;
                    }
                }

                id++;
            }
        }
    }
    #endregion
}