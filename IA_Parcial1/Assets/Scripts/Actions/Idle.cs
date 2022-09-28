using System;

using UnityEngine;

public class Idle : FSMAction
{
    #region CONSTRUCTOR
    public Idle(Action<int> onSetFlag)
    {
        this.onSetFlag = onSetFlag;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
        Debug.Log("Idle");
    }

    public override void AbruptExit()
    {
        
    }
    #endregion
}