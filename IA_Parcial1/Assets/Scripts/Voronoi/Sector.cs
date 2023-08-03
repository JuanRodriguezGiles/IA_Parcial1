using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class Sector
{
    #region EXPOSED_FIELDS
    public Vector2 minePos = default;
    public Color sectorColor = Color.white;
    public List<Segment> segments = null;
    public List<Vector2> intersections = null;
    public Vector3[] points = null;
    #endregion

    #region CONSTRUCTOR
    public Sector(Vector2 minePos)
    {
        this.minePos = minePos;

        sectorColor = Random.ColorHSV();
        sectorColor.a = 0.35f;

        segments = new List<Segment>();
        intersections = new List<Vector2>();
    }
    #endregion

    #region PUBLIC_METHODS
    public void AddSegment(Vector2 origin, Vector2 final)
    {
        segments.Add(new Segment(origin, final));
    }

    public void DrawSegments()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].Draw();
        }
    }

    public void DrawSector(float alpha)
    {
        sectorColor.a = alpha;
        Handles.color = sectorColor;
        Handles.DrawAAConvexPolygon(points);

        Handles.color = Color.black;
        Handles.DrawPolyLine(points);
    }

    public void SetIntersections()
    {
        intersections.Clear();

        //Iterate all segments (without comparing with itself)
        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                //Calculate the intersection point between the mediatrices of the two segments 
                //Mediatrices == lines perpendicular to the segments that pass through their midpoints
                Vector2 intersectionPoint = GetIntersection(segments[i], segments[j]);

                //If intersection is already on list skip
                if (intersections.Contains(intersectionPoint)) continue;

                //Calculate maximum distance between intersectionPoint and the origin point of segment
                //OriginPoint represents the starting point of the segment
                float maxDistance = Vector2.Distance(intersectionPoint, segments[i].originPoint);

                bool isBorder = true;
                for (int k = 0; k < segments.Count; k++)
                {
                    if (k == i || k == j) continue;

                    //Comparar si la distancia entre la intersección de las mediatrices y todas las demás minas es más pequeña que la distancia maxima 
                    //Checks if the distance between the intersectionPoint and the endpoint of  segment is less than the maximum distance
                    if (IsPositionCloser(intersectionPoint, segments[k].endPoint, maxDistance))
                    {
                        //intersectionPoint is closer to another mine's segment than to the current segment 
                        isBorder = false;
                        break;
                    }
                }

                //If still true
                if (isBorder)
                {
                    intersections.Add(intersectionPoint); //Set vaild border point
                    segments[i].intersections.Add(intersectionPoint); //Add to i intersection
                    segments[j].intersections.Add(intersectionPoint); //Add to j intersection
                }
            }
        }
        
        //If they dont have 2 intersections they are not part of theb order
        segments.RemoveAll((segment) => segment.intersections.Count != 2);

        SortIntersections();
        SetPointsInSector();
    }

    public void AddSegmentLimits(List<Edge> limits)
    {
        for (int i = 0; i < limits.Count; i++)
        {
            Vector2 origin = minePos;
            Vector2 final = limits[i].GetPosition(origin);

            segments.Add(new Segment(origin, final));
        }
    }

    public bool IsPointInSector(Vector3 point)
    {
        bool inside = false;

        if (points == null) return false;

        //Last point
        Vector2 endPoint = points[^1];
        //Extract x and y
        float endX = endPoint.x;
        float endY = endPoint.y;

        //Iterate all points
        for (int i = 0; i < points.Length; i++)
        {
            //Store x and y of current endPoint
            float startX = endX; 
            float startY = endY;

            //Update endpoint to current point in loop
            endPoint = points[i];

            //Extract x and y
            endX = endPoint.x; 
            endY = endPoint.y;

            //bitwise exclusive OR -> returns true if the two operands have different bool values
            //endY > point.y ^ startY > point.y -> checks if the point is between the two Y coords of the line
            //point.x - endX < (point.y - endY) * (startX - endX) / (startY - endY) -> evaluates if point is in right side of segment
            inside ^= endY > point.y ^ startY > point.y && point.x - endX < (point.y - endY) * (startX - endX) / (startY - endY);
        }

        return inside;
    }
    #endregion

    #region PRIVATE_METHODS
    private bool IsPositionCloser(Vector2 intersectionPoint, Vector2 pointEnd, float maxDistance)
    {
        float distance = Vector2.Distance(intersectionPoint, pointEnd);
        return distance < maxDistance;
    }

    private void SortIntersections()
    {
        List<IntersectionPoint> intersectionPoints = new List<IntersectionPoint>();
        for (int i = 0; i < intersections.Count; i++)
        {
            intersectionPoints.Add(new IntersectionPoint(intersections[i]));
        }

        //Init values
        float minX = intersectionPoints[0].position.x;
        float maxX = intersectionPoints[0].position.x;
        float minY = intersectionPoints[0].position.y;
        float maxY = intersectionPoints[0].position.y;

        //Iterate all intersectionPoints to find min and max values
        for (int i = 0; i < intersections.Count; i++)
        {
            if (intersectionPoints[i].position.x < minX) minX = intersectionPoints[i].position.x;
            if (intersectionPoints[i].position.x > maxX) maxX = intersectionPoints[i].position.x;
            if (intersectionPoints[i].position.y < minY) minY = intersectionPoints[i].position.y;
            if (intersectionPoints[i].position.y > maxY) maxY = intersectionPoints[i].position.y;
        }

        //Calculate center of the sector using the min/max values
        Vector2 center = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            Vector2 pos = intersectionPoints[i].position;

            //Calculate the angle between the position of the current intersection point and the center of the sector using the arccosine function
            //Determines the angle in radians between the positive x-axis and the line segment connecting the pos and the center.
            intersectionPoints[i].angle = Mathf.Acos((pos.x - center.x) / Mathf.Sqrt(Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.y - center.y, 2f)));
            
            if (pos.y > center.y)
            {
                //Intersection point is in the lower half of the sector
                //Adjust angle to place the points in clockwise order around the center instead of counter clockwise order
                intersectionPoints[i].angle = Mathf.PI + Mathf.PI - intersectionPoints[i].angle;
            }
        }

        //Sort the points by angle o have a list of intersection points that represents a polygon surrounding the sector
        intersectionPoints = intersectionPoints.OrderBy(p => p.angle).ToList();

        intersections.Clear();
        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            intersections.Add(intersectionPoints[i].position);
        }
    }

    private void SetPointsInSector()
    {
        //+1 To repeat first intersection point at end of array
        points = new Vector3[intersections.Count + 1];

        for (int i = 0; i < intersections.Count; i++)
        {
            points[i] = new Vector3(intersections[i].x, intersections[i].y, 0f);
        }
        
        //Close sector polygon forming a "loop". Represents convex polygon
        points[intersections.Count] = points[0];
    }

    //Line line intersection https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
    private Vector2 GetIntersection(Segment seg1, Segment seg2)
    {
        Vector2 intersection = Vector2.zero;

        Vector2 p1 = seg1.mediatrix;
        Vector2 p2 = seg1.mediatrix + seg1.direction * NodeUtils.MapSize.magnitude;
        Vector2 p3 = seg2.mediatrix;
        Vector2 p4 = seg2.mediatrix + seg2.direction * NodeUtils.MapSize.magnitude;

        if ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x) == 0) return intersection;

        intersection.x = ((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
        intersection.y = ((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));

        return intersection;
    }
    #endregion
}
