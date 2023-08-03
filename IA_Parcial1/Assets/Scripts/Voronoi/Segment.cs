using UnityEngine;
using System.Collections.Generic;

public class Segment
{
    #region EXPOSED_FIELDS
    public Vector2 originPoint = Vector2.zero;
    public Vector2 endPoint = Vector2.zero;
    public Vector2 direction = Vector2.zero;
    public Vector2 mediatrix = Vector2.zero;
    public List<Vector2> intersections = null;
    public float weightOrigin;
    public float weightEnd;
    #endregion
    
    #region CONSTRUCTORS
    public Segment(Vector2 originPoint, Vector2 endPoint, float weightOrigin = 0, float weightEnd = 0)
    {
        this.originPoint = originPoint;
        this.endPoint = endPoint;
        this.weightOrigin = weightOrigin;
        this.weightEnd = weightEnd;
        
        //Get midpoint of segment
        mediatrix = new Vector2((originPoint.x + endPoint.x) / 2, (originPoint.y + endPoint.y) / 2);
        //Get direction vector by getting perpendicular vector from origin and end point
        direction = Vector2.Perpendicular(new Vector2(endPoint.x - originPoint.x, endPoint.y - originPoint.y));

        intersections = new List<Vector2>();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Draw()
    {
        Gizmos.DrawLine(originPoint, endPoint);
    }
    #endregion
}