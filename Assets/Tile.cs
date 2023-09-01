using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TTD
{

    public class Tile : MonoBehaviour
    {
        public Tile above, upR, downR, below, downL, upL; // 1 2 3 4 5 6. 1->3->6->4->5->2->1  
        bool set;

        [Range(0, 3)] // 0 plain, 1 treasure, 2 monster, 3 pitfall
        private int type;

        [Range(0, 2)] // 00 line, 01 blob, 02 corner, 10-2 2 tiles, 20 M, 21 Y, 22 line, 30-2 big C
        private int shape;

        [Range(0, 5)]
        int rotation;

        Dictionary<(int, int), (int, int, int, int)> tilesPositions;
        private SpriteRenderer renderer;

        void Start()
        {
            renderer = GetComponent<SpriteRenderer>();
            set = false;
            //centre = false;
            type = 0;
            shape = 0;
            rotation = 0;
            tilesPositions = new Dictionary<(int, int), (int, int, int, int)>()
            {
                {(0,0), (1,1,0,0)},{(0,1), (1,3,0,0)},{(0,6), (1,2,0,0)},
                {(1,0), (1,0,0,0)},
                {(2,0), (6,1,2,0)},{(2,1), (1,1,3,0)},{(2,2), (1,1,1,0)},
                {(3,0), (6,1,1,2)}
            };
        }

        void Update()
        {
            if (Input.GetKeyDown("r"))
            {
                OnMouseExit();
                rotation++;
                if (rotation > 5)
                {
                    rotation = 0;
                }
            }
        }

        SpriteRenderer GetSpriteRenderer() => renderer;

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

        public void setType(int type)
        {
            this.type = type;
        }

        public void setShape(int shape)
        {
            this.shape = shape;
        }

        bool checkTiles(int[] ints, Tile tile, int rotation)
        {
            if (ints.Length == 0 || tile == null)
            {
                return !tile.set;
            }
            else
            {
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;

                if (ints[0] == 0) return !tile.set;
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        if (tile.above == null) return false;
                        tempTile = tile.above;
                        break;
                    case 2:
                        if (tile.upR == null) return false;
                        tempTile = tile.upR;
                        break;
                    case 3:
                        if (tile.downR == null) return false;
                        tempTile = tile.downR;
                        break;
                    case 4:
                        if (tile.below == null) return false;
                        tempTile = tile.below;
                        break;
                    case 5:
                        if (tile.downL == null) return false;
                        tempTile = tile.downL;
                        break;
                    case 0:
                        if (tile.upL == null) return false;
                        tempTile = tile.upL;
                        break;
                }
                return checkTiles(temp, tempTile, rotation) && !tile.set;
            }
        }

        void colorTiles(int[] ints, Tile tile, int type, int rotation)
        {
            if (ints.Length == 0)
            {
                switch (type)
                {
                    case -1:
                        tile.GetSpriteRenderer().color = Color.white;
                        break;
                    case 0:
                        tile.GetSpriteRenderer().color = Color.yellow;
                        break;
                    case 1:
                        tile.GetSpriteRenderer().color = Color.blue;
                        break;
                    case 2:
                        tile.GetSpriteRenderer().color = Color.red;
                        break;
                    case 3:
                        tile.GetSpriteRenderer().color = Color.black;
                        break;
                }
            }
            else
            {

                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;
                switch (type)
                {
                    case -1:
                        tile.GetSpriteRenderer().color = Color.white;
                        break;
                    case 0:
                        tile.GetSpriteRenderer().color = Color.yellow;
                        break;
                    case 1:
                        tile.GetSpriteRenderer().color = Color.blue;
                        break;
                    case 2:
                        tile.GetSpriteRenderer().color = Color.red;
                        break;
                    case 3:
                        tile.GetSpriteRenderer().color = Color.black;
                        break;
                }

                if (ints[0] == 0) return;
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        tempTile = tile.above;
                        break;
                    case 2:
                        tempTile = tile.upR;
                        break;
                    case 3:
                        tempTile = tile.downR;
                        break;
                    case 4:
                        tempTile = tile.below;
                        break;
                    case 5:
                        tempTile = tile.downL;
                        break;
                    case 0:
                        tempTile = tile.upL;
                        break;
                }

                colorTiles(temp, tempTile, type, rotation);
            }
        }

        void OnMouseOver()
        {
            if (!set)
            {
                (int, int, int, int) nextStep = tilesPositions[(type, shape)];
                if (checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation))
                {
                    colorTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, type, rotation);
                }
            }
        }
        void OnMouseExit()
        {
            if (!set)
            {
                (int, int, int, int) nextStep = tilesPositions[(type, shape)];
                if (checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation))
                {
                    colorTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, -1, rotation);
                }
            }
        }
        void OnMouseDown()
        {
            if (!set)
            {
                (int, int, int, int) nextStep = tilesPositions[(type, shape)];
                if (checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation))
                {
                    setTrue(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, type, rotation);
                }
            }
        }

        void setTrue(int[] ints, Tile tile, int type, int rotation)
        {

            if (ints.Length == 0)
            {
                tile.set = true;
            }
            else
            {
                tile.set = true;
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;
                if (ints[0] == 0) return;
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        tempTile = tile.above;
                        break;
                    case 2:
                        tempTile = tile.upR;
                        break;
                    case 3:
                        tempTile = tile.downR;
                        break;
                    case 4:
                        tempTile = tile.below;
                        break;
                    case 5:
                        tempTile = tile.downL;
                        break;
                    case 0:
                        tempTile = tile.upL;
                        break;
                }
                setTrue(temp, tempTile, type, rotation);
            }
        }
    }
}