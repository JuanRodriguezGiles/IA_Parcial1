using System;

public class Mine : FSMAction
{
    #region PRIVATE_FIELDS
    private readonly Func<float> onGetDeltaTime;
    private Action onEmptyMine;
    private Action stopFlocking;

    private float currentMiningTime = 0;
    private int mineUses = 2;

    private const float miningTime = 2.0f;
    #endregion

    #region CONSTRUCTOR
    public Mine(Action<int> onSetFlag, Func<float> onGetDeltaTime, Action onEmptyMine, Action stopFlocking)
    {
        this.onGetDeltaTime = onGetDeltaTime;
        this.onSetFlag = onSetFlag;
        this.onEmptyMine = onEmptyMine;
        this.stopFlocking = stopFlocking;
    }
    #endregion

    #region OVERRIDE
    public override void Execute()
    {
        if (currentMiningTime == 0)
        {
            stopFlocking?.Invoke();
        }
        
        if (currentMiningTime < miningTime)
        {
            currentMiningTime += onGetDeltaTime.Invoke();
        }
        else
        {
            currentMiningTime = 0.0f;

            mineUses--;

            if (mineUses == 0)
            {
                mineUses = 2;
                onEmptyMine?.Invoke();
            }
            onSetFlag?.Invoke((int)Flags.OnFullInventory);
        }
    }

    public override void AbruptExit()
    {
        currentMiningTime = 0;
    }
    #endregion
}