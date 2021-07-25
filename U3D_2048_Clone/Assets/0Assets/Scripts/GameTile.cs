using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile
{
    public int value;
    public Vector2? currentIdx = null; //Current index in game matrix
    public Vector2? nextIdx = null; //Next index in game matrix
    public Vector2? prevIdx = null; //Previous index in game matrix

    public GameTile(Vector2 idx)
    {
        value = 2;
        currentIdx = idx;
    }
}
