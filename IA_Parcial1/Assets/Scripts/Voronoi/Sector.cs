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

        //Recorrer todos los segmentos (sin comparar con si mismo)
        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                //Calcular punto de interesccion entre las mediatrices 
                Vector2 intersectionPoint = GetIntersection(segments[i], segments[j]);

                //Si ya esta en la lista ignorar
                if (intersections.Contains(intersectionPoint)) continue;

                //Distancia entre la mina actual y el punto de interseccion
                float maxDistance = Vector2.Distance(intersectionPoint, segments[i].originPoint);

                //Reviso si es un punto valido para definir fronteras
                bool isBorder = true;
                for (int k = 0; k < segments.Count; k++)
                {
                    //maxDistance es la distancia entre la interseccion y la mina i y, como son mediatrices, la otra mina equidistante seria la j
                    if (k == i || k == j) continue;

                    //Comparar si la distancia entre la intersección de las mediatrices y todas las demás minas es más pequeña que la distancia maxima 
                    if (IsPositionCloser(intersectionPoint, segments[k].endPoint, maxDistance))
                    {
                        isBorder = false;
                        break;
                    }
                }

                if (isBorder)
                {
                    intersections.Add(intersectionPoint);
                    segments[i].intersections.Add(intersectionPoint);
                    segments[j].intersections.Add(intersectionPoint);
                }
            }
        }
        
        //Remuevo los segmentos que no representan a la frontera de mi region
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

        Vector2 endPoint = points[^1];
        float endX = endPoint.x;
        float endY = endPoint.y;

        for (int i = 0; i < points.Length; i++)
        {
            float startX = endX; 
            float startY = endY;

            endPoint = points[i];

            endX = endPoint.x; 
            endY = endPoint.y;

            inside ^= (endY > point.y ^ startY > point.y) && ((point.x - endX) < (point.y - endY) * (startX - endX) / (startY - endY));
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

        float minX = intersectionPoints[0].position.x;
        float maxX = intersectionPoints[0].position.x;
        float minY = intersectionPoints[0].position.y;
        float maxY = intersectionPoints[0].position.y;

        for (int i = 0; i < intersections.Count; i++)
        {
            if (intersectionPoints[i].position.x < minX) minX = intersectionPoints[i].position.x;
            if (intersectionPoints[i].position.x > maxX) maxX = intersectionPoints[i].position.x;
            if (intersectionPoints[i].position.y < minY) minY = intersectionPoints[i].position.y;
            if (intersectionPoints[i].position.y > maxY) maxY = intersectionPoints[i].position.y;
        }

        Vector2 center = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            Vector2 pos = intersectionPoints[i].position;

            //Angulo entre interseccion y centro
            intersectionPoints[i].angle = Mathf.Acos((pos.x - center.x) / Mathf.Sqrt(Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.y - center.y, 2f)));

            
            if (pos.y > center.y)
            {
                intersectionPoints[i].angle = Mathf.PI + Mathf.PI - intersectionPoints[i].angle;
            }
        }

        //sort por angulo para asimilar un circulo
        intersectionPoints = intersectionPoints.OrderBy(p => p.angle).ToList();

        intersections.Clear();
        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            intersections.Add(intersectionPoints[i].position);
        }
    }

    private void SetPointsInSector()
    {
        points = new Vector3[intersections.Count + 1];

        for (int i = 0; i < intersections.Count; i++)
        {
            points[i] = new Vector3(intersections[i].x, intersections[i].y, 0f);
        }
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

        if (((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x)) == 0) return intersection;

        intersection.x = ((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
        intersection.y = ((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));

        return intersection;
    }
    #endregion
}
