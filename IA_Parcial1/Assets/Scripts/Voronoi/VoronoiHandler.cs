using System.Collections.Generic;

using UnityEngine;

public class VoronoiHandler : MonoBehaviour
{
    #region EXPOSED_FIELDS
    public float alpha = 0;
    #endregion
    
    #region PRIVATE_FIELDS
    private List<Edge> edges = new List<Edge>();
    private List<Sector> sectors = new List<Sector>();
    #endregion

    #region UNITY_CALLS
    private void OnDrawGizmos()
    {
        if (sectors == null) return;

        foreach (var sector in sectors)
        {
            sector.DrawSector(alpha);
            
            sector.DrawSegments();
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void Config()
    {
        //Configure initial values of voronoi diagram by creating 4 edges that represent it's boundaries (left, up, right, down)
        edges.Add(new Edge(new Vector2(0, 0), DIR.LEFT));
        edges.Add(new Edge(new Vector2(0f, NodeUtils.MapSize.y), DIR.UP));
        edges.Add(new Edge(new Vector2(NodeUtils.MapSize.x, NodeUtils.MapSize.y), DIR.RIGHT));
        edges.Add(new Edge(new Vector2(NodeUtils.MapSize.x, 0f), DIR.DOWN));
    }

    public void UpdateSectors(List<Vector2> mines)
    {
        sectors.Clear();
        if (mines.Count == 0) return;

        //Add mine sectors
        foreach (var mine in mines)
        {
            sectors.Add(new Sector(mine));
        }

        //Add segment limits
        foreach (var mineSector in sectors)
        {
            mineSector.AddSegmentLimits(edges);
        }

        //Connect mines
        for (int i = 0; i < mines.Count; i++)
        {
            for (int j = 0; j < mines.Count; j++)
            {
                if (i == j) continue;

                sectors[i].AddSegment(mines[i], mines[j]);
            }
        }

        //Set intersections  for each sector
        foreach (var mineSector in sectors)
        {
            mineSector.SetIntersections();
        }
    }

    public Vector2Int GetNearestMine(Vector2 currentPos)
    {
        if (sectors == null) return Vector2Int.zero;
        
        foreach (var sector in sectors)
        {
            if (sector.IsPointInSector(currentPos))
            {
                return new Vector2Int((int)sector.minePos.x, (int)sector.minePos.y);
            }
        }

        return Vector2Int.zero;
    }
    #endregion
}