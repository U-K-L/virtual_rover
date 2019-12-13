using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public GameObject rover;
    public SerialRotation serial;
    public List <Tiles> tiles;
    public Tiles prefab;
    public int nodeSize;
    // Start is called before the first frame update
    void Start()
    {
        AddTiles();
        resetTiles();
    }

    public void AddTiles()
    {
        Flush();
        for (int i = 0; i < nodeSize; i++)
        {
            tiles.Add(Instantiate(prefab, rover.transform));
        }
    }
    void Flush()
    {
        for (int i = 0; i < nodeSize; i++)
        {
            Destroy(tiles[i]);
        }
        tiles = new List<Tiles>();
    }
    // Update is called once per frame
    void Update()
    {

    }

    void resetTiles()
    {
        foreach (Tiles tile in tiles)
        {
            tile.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
