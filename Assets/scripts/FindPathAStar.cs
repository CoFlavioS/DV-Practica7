using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker (MapLocation l, float g, float h, float f, GameObject m, PathMarker p)
    {
        this.location = l;
        G = g;
        H = h;
        F = f;
        this.marker = m;
        this.parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class FindPathAStar : MonoBehaviour
{
    public Maze maze;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> close = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            if(!m.name.Contains("Start") && !m.name.Contains("End"))
                Destroy(m);
        }
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse.leftButton.wasPressedThisFrame && (startNode == null ? true : !startNode.marker.GetComponent<Player>().walking))
        {
            Vector3 mousePositionRaw = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            Vector3Int mousePosition = new Vector3Int(Mathf.RoundToInt(mousePositionRaw.x), Mathf.RoundToInt(mousePositionRaw.y), 0);
            RaycastHit2D hit = Physics2D.Raycast(mousePositionRaw, Camera.main.transform.forward);
            if (InsideCamera(mousePosition))
            {
                if(hit.collider == null)
                {
                    SetAStar(mousePosition);
                }
                else
                {
                    Debug.Log("There is: " + hit.collider.name);
                }
            }
            else 
            {
                Debug.Log("Outise of cameras view");
            }
        }
    }

    bool InsideCamera(Vector3 mp)
    {
        Vector3 relToVp = Camera.main.WorldToViewportPoint(mp);

        if (relToVp.x >= 0 && relToVp.x < 1 &&
            relToVp.y >= 0 && relToVp.y < 1)
            return true;

        return false;
    }

    void SetAStar(Vector3Int mp)
    {
        if (startNode == null)
        {
            startNode = new PathMarker(
                new MapLocation(mp.x, mp.y),
                0, 0, 0,
                Instantiate(start, mp, Quaternion.identity),
                null
            );
        }
        else if (goalNode == null)
        {
            goalNode = new PathMarker(
                new MapLocation(mp.x, mp.y),
                0, 0, 0,
                Instantiate(end, mp, Quaternion.identity),
                null
            );
            open.Clear();
            close.Clear();
            open.Add(startNode);

            while (!goalNode.Equals(lastPos) && open.Count > 0)
            {
                Search();
            }

            if (goalNode.Equals(lastPos))
            {
                GetPath();
            }
            else
            {
                Debug.Log("No hay camino al destino marcado.");
                Destroy(goalNode.marker);
                goalNode = null;
            }

        }
    }

    void Search()
    {
        if(open.Count > 0)
        {
            open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
            PathMarker pm = open.ElementAt(0);
            close.Add(pm);
            open.RemoveAt(0);
            lastPos = pm;
        }
        if (lastPos.Equals(goalNode)) return;

        foreach(MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + lastPos.location;

            if (InsideCamera(new Vector3(neighbour.x, neighbour.z, 0)))
            {
                if(maze.map[neighbour.x + maze.halfsizemap, neighbour.z + maze.halfsizemap] == 0)
                {
                    if (!IsClosed(neighbour))
                    {
                        float G = Vector2.Distance(lastPos.location.ToVector(), neighbour.ToVector()) + lastPos.G;
                        float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                        float F = G + H;

                        if(!UpdateMarker(neighbour, G, H, F, lastPos))
                        {
                            open.Add(new PathMarker(neighbour, G, H, F, null, lastPos));
                        }
                    }
                }
            }
        }
    }

    bool IsClosed(MapLocation m)
    {
        foreach (PathMarker p in close)
        {
            if (p.location.Equals(m)) return true;
        }
        return false;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                if(p.G > g)
                {
                    p.G = g;
                    p.H = h;
                    p.F = f;
                    p.parent = prt;
                }
                return true;
            }
        }
        return false;
    }

    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;
        Stack<PathMarker> stack = new Stack<PathMarker>();

        while(!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x, begin.location.z, 1), Quaternion.identity);
            stack.Push(begin);
            begin = begin.parent;
        }

        startNode.marker.GetComponent<Player>().SetPath(stack);
    }

    public void ClearOnEnd()
    {
        open.Clear();
        close.Clear();
        RemoveAllMarkers();
        Destroy(startNode.marker);
        Destroy(goalNode.marker);
        startNode = null;
        goalNode = null;
        maze.DrawMap();
    }
}
