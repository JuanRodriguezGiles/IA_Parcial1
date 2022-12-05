using UnityEngine;

public class IntersectionPoint
{
    #region EXPOSED_FIELDS
    public Vector2 position = Vector2.zero;
    public float angle = 0f;
    #endregion

    #region CONSTRUCTOR
    public IntersectionPoint(Vector2 position)
    {
        this.position = position;

        angle = 0f;
    }
    #endregion
}