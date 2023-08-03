using UnityEngine;

public enum DIR
{
    UP,
    RIGHT,
    DOWN,
    LEFT
}

public class Edge
{
    #region PRIVATE_FIELDS
    private Vector2 origin = Vector2.zero;
    private DIR thisDir = default;
    #endregion

    #region CONSTRUCTOR
    public Edge(Vector2 origin, DIR thisDir)
    {
        this.origin = origin;
        this.thisDir = thisDir;
    }
    #endregion

    #region PUBLIC_METHODS
    public Vector2 GetPosition(Vector2 pos)
    {
        //Get absoulte distance by calcultaing abs difference, used to determine shift amount
        float distanceX = Mathf.Abs(Mathf.Abs(pos.x) - Mathf.Abs(origin.x)) * 2f;
        float distanceY = Mathf.Abs(Mathf.Abs(pos.y) - Mathf.Abs(origin.y)) * 2f;

        switch (thisDir)
        {
            case DIR.LEFT:
                pos.x -= distanceX;
                break;
            case DIR.UP:
                pos.y += distanceY;
                break;
            case DIR.RIGHT:
                pos.x += distanceX;
                break;
            case DIR.DOWN:
                pos.y -= distanceY;
                break;
            default:
                pos = Vector2.zero;
                break;
        }

        //Return pos at toher end of edge
        return pos;
    }
    #endregion
}