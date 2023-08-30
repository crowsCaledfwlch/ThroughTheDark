using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tile : MonoBehaviour
{
    private Tile above, upL, upR, below, downL, downR;
    private bool set;
    private bool centre;
    private int type;
    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        set = false;
        centre = false;
        type = 0;
    }
    // Start is called before the first frame update
    public void setAbove(Tile tile)
    {
        above = tile;
    }
    public void setUpL(Tile tile)
    {
        upL = tile;
    }
    public void setUpR(Tile tile)
    {
        upR = tile;
    }
    public void setBelow(Tile tile)
    {
        below = tile;
    }
    public void setDownL(Tile tile)
    {
        downL = tile;
    }
    public void setDownR(Tile tile)
    {
        downR = tile;
    }

    void OnMouseOver()
    {
        if (!set)
        {
            switch (type)
            {
                case 0:
                    renderer.color = Color.blue;
                    break;
            }
        }
    }
    void OnMouseExit()
    {
        if (!set)
        {
            renderer.color = Color.white;
        }
    }
}
