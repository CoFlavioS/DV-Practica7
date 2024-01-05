using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLocation
{
    public int x;
    public int z;

    public MapLocation(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, z);
    }

    public static MapLocation operator +(MapLocation a, MapLocation b)
       => new MapLocation(a.x + b.x, a.z + b.z);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return x == ((MapLocation)obj).x && z == ((MapLocation)obj).z;
    }

    public override int GetHashCode()
    {
        return 0;
    }

}

public class Maze : MonoBehaviour
{
    public List<MapLocation> directions = new List<MapLocation>() {
                                            new MapLocation(1,0),
                                            new MapLocation(1,1),
                                            new MapLocation(0,1),
                                            new MapLocation(-1,1),
                                            new MapLocation(-1,0),
                                            new MapLocation(-1,-1),
                                            new MapLocation(0,-1),
                                            new MapLocation(1,-1) };

    public float magnification = 7.0f;  // recommend 4 to 20
    public Tile[] tileset;
    Tilemap tilemap;
    public int halfsizemap = 20;
    public byte[,] map;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        map = new byte[halfsizemap * 2, halfsizemap * 2];
        DrawMap();
    }

    public void DrawMap()
    {
        int pos_x = Random.Range(0, 60000);
        int pos_y = Random.Range(0, 60000);

        for (int x = -halfsizemap; x < halfsizemap; x++)
        {
            for (int y = -halfsizemap; y < halfsizemap; y++)
            {
                int tile_id = GetIdUsingPerlin(x + pos_x, y + pos_y);
                if (tile_id == 2 || tile_id == 3)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileset[tile_id]);
                    map[x + halfsizemap, y + halfsizemap] = 1;
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    map[x + halfsizemap, y + halfsizemap] = 0;
                }

            }
        }
    }
    int GetIdUsingPerlin(int x, int y)
    {
        float raw_perlin = Mathf.PerlinNoise(x / magnification, y / magnification);
        if (raw_perlin < 0) raw_perlin *= -1;
        int categorized_perlin = Mathf.FloorToInt(raw_perlin * tileset.Length) % tileset.Length;
        return categorized_perlin;
    }
}
