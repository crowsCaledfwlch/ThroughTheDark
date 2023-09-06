using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;

namespace TTD
{
    public class Tile : NetworkBehaviour
    {
        public Tile above, upR, downR, below, downL, upL; // 1 2 3 4 5 6. 1->3->6->4->5->2->1  
        public NetworkVariable<bool> set = new NetworkVariable<bool>();

        [Range(0, 3)] // 0 plain, 1 treasure, 2 monster, 3 pitfall
        static private NetworkVariable<int> type = new NetworkVariable<int>();

        [Range(0, 2)] // 00 line, 01 blob, 02 corner, 10-2 2 tiles, 20 M, 21 Y, 22 line, 30-2 big C
        static private NetworkVariable<int> shape = new NetworkVariable<int>();

        [Range(0, 5)]
        static NetworkVariable<int> rotation = new NetworkVariable<int>();
        static Tile tileBase;
        tilegrid playerObject;
        Dictionary<(int, int), (int, int, int, int)> tilesPositions = new Dictionary<(int, int), (int, int, int, int)>()
            {
                {(0,0), (1,1,0,0)},{(0,1), (1,3,0,0)},{(0,2), (1,2,0,0)},
                {(1,0), (1,0,0,0)},
                {(2,0), (6,1,2,0)},{(2,1), (1,1,3,0)},{(2,2), (1,1,1,0)},
                {(3,0), (6,1,1,2)}
            };
        private SpriteRenderer renderer;
        private NetworkVariable<Color> setColor = new NetworkVariable<Color>();
        bool mouseOver = false;
        void Start()
        {
            tileBase = this;
            if (NetworkManager.Singleton.IsServer) setColor.Value = Color.white;
            renderer = GetComponent<SpriteRenderer>();
            set.Value = false;
            type.Value = 0;
            shape.Value = 0;
            rotation.Value = 0;
            if (NetworkManager.Singleton.IsClient) playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<tilegrid>();
        }

        void FixedUpdate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<SpriteRenderer>();
            }
            renderer.color = setColor.Value;
        }
        [ServerRpc(RequireOwnership = false)]
        public void setSetExternalServerRpc()
        {
            StartCoroutine(starterSet());
        }
        IEnumerator starterSet()
        {
            yield return new WaitForSeconds(.5f);
            this.set.Value = true;
            this.setColor.Value = Color.yellow;
        }
        [ServerRpc(RequireOwnership = false)]
        void rotateServerRpc()
        {
            rotation.Value++;
            if (rotation.Value > 5)
            {
                rotation.Value = 0;
            }
        }

        void Update()
        {
            if (NetworkManager.Singleton.IsClient)
            {
                if (Input.GetKeyDown("r") && mouseOver && playerObject.myTurn)
                {
                    checkProcServerRpc(1, NetworkManager.Singleton.LocalClientId);
                    rotateServerRpc();
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

        static public void setType(int type)
        {
            Tile.type.Value = type;
        }

        static public void setShape(int shape)
        {
            Tile.shape.Value = shape;
        }

        [ServerRpc(RequireOwnership = false)]
        public void setTSServerRpc(int t, int s)
        {
            Tile.type.Value = t;
            Tile.shape.Value = s;
        }
        static public void setTS(int t, int s)
        {
            Tile.tileBase.setTSServerRpc(t, s);
        }

        public int radialSetCheck()
        {
            int num = 0;
            if (this.above != null) if (this.above.set.Value) num++;
            if (this.upR != null) if (this.upR.set.Value) num++;
            if (this.downR != null) if (this.downR.set.Value) num++;
            if (this.below != null) if (this.below.set.Value) num++;
            if (this.downL != null) if (this.downL.set.Value) num++;
            if (this.upL != null) if (this.upL.set.Value) num++;
            return num;
        }

        (bool, int) checkTiles(int[] ints, Tile tile, int rotation)
        {
            if (ints.Length == 0 || tile == null)
            {
                return (!tile.set.Value, tile.radialSetCheck());
            }
            else
            {
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;

                if (ints[0] == 0) return (!tile.set.Value, tile.radialSetCheck());
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        if (tile.above == null) return (false, 0);
                        tempTile = tile.above;
                        break;
                    case 2:
                        if (tile.upR == null) return (false, 0);
                        tempTile = tile.upR;
                        break;
                    case 3:
                        if (tile.downR == null) return (false, 0);
                        tempTile = tile.downR;
                        break;
                    case 4:
                        if (tile.below == null) return (false, 0);
                        tempTile = tile.below;
                        break;
                    case 5:
                        if (tile.downL == null) return (false, 0);
                        tempTile = tile.downL;
                        break;
                    case 0:
                        if (tile.upL == null) return (false, 0);
                        tempTile = tile.upL;
                        break;
                }
                (bool, int) result = checkTiles(temp, tempTile, rotation);
                return (result.Item1 && !tile.set.Value, tile.radialSetCheck() + result.Item2);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void checkProcServerRpc(int EED, ulong ID)
        {
            if (EED == 0)
            { // OO
                if (!set.Value)
                {
                    (int, int, int, int) nextStep = tilesPositions[(type.Value, shape.Value)];
                    (bool, int) result = checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation.Value);
                    if (result.Item1 && result.Item2 > 1)
                    {
                        colorTilesClientRpc(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, type.Value, rotation.Value);
                    }
                }
            }
            else if (EED == 2)
            { // OMD
                if (!set.Value)
                {
                    (int, int, int, int) nextStep = tilesPositions[(type.Value, shape.Value)];
                    (bool, int) result = checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation.Value);
                    if (result.Item1 && result.Item2 > 1)
                    {
                        setTrueClientRpc(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, type.Value, rotation.Value);
                        NetworkManager.Singleton.ConnectedClients[ID].PlayerObject.GetComponent<tilegrid>().EndTurn();
                    }
                }
            }
            else
            { // OMEx and catch
                if (!set.Value)
                {
                    (int, int, int, int) nextStep = tilesPositions[(type.Value, shape.Value)];
                    (bool, int) result = checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation.Value);
                    if (result.Item1 && result.Item2 > 1)
                    {
                        colorTilesClientRpc(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, -1, rotation.Value);
                    }
                }
            }

        }

        void OnMouseOver()
        {
            if (NetworkManager.Singleton.IsClient)
            {
                if (playerObject.myTurn)
                {
                    mouseOver = true;
                    checkProcServerRpc(0, NetworkManager.Singleton.LocalClientId);
                }
            }
        }
        void OnMouseExit()
        {
            if (NetworkManager.Singleton.IsClient)
            {
                if (playerObject.myTurn)
                {
                    mouseOver = false;
                    checkProcServerRpc(1, NetworkManager.Singleton.LocalClientId);
                }
            }
        }

        void OnMouseDown()
        {
            if (NetworkManager.Singleton.IsClient)
            {
                checkProcServerRpc(2, NetworkManager.Singleton.LocalClientId);
            }
        }

        [ClientRpc]
        void colorTilesClientRpc(int[] ints, int type, int rotation)
        {
            colorTilesServerRpc(ints, type, rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        void colorTilesServerRpc(int[] ints, int type, int rotation)
        {
            if (ints.Length == 0)
            {
                switch (type)
                {
                    case -1:
                        this.setColor.Value = Color.white;
                        break;
                    case 0:
                        this.setColor.Value = Color.yellow;
                        break;
                    case 1:
                        this.setColor.Value = Color.blue;
                        break;
                    case 2:
                        this.setColor.Value = Color.red;
                        break;
                    case 3:
                        this.setColor.Value = Color.black;
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
                        this.setColor.Value = Color.white;
                        break;
                    case 0:
                        this.setColor.Value = Color.yellow;
                        break;
                    case 1:
                        this.setColor.Value = Color.blue;
                        break;
                    case 2:
                        this.setColor.Value = Color.red;
                        break;
                    case 3:
                        this.setColor.Value = Color.black;
                        break;
                }

                if (ints[0] == 0) return;
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        tempTile = this.above;
                        break;
                    case 2:
                        tempTile = this.upR;
                        break;
                    case 3:
                        tempTile = this.downR;
                        break;
                    case 4:
                        tempTile = this.below;
                        break;
                    case 5:
                        tempTile = this.downL;
                        break;
                    case 0:
                        tempTile = this.upL;
                        break;
                }

                tempTile.colorTilesClientRpc(temp, type, rotation);
            }
        }


        [ClientRpc]
        void setTrueClientRpc(int[] ints, int type, int rotation)
        {
            setTrueServerRpc(ints, type, rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        void setTrueServerRpc(int[] ints, int type, int rotation)
        {

            if (ints.Length == 0)
            {
                this.set.Value = true;
            }
            else
            {
                this.set.Value = true;
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
                        tempTile = this.above;
                        break;
                    case 2:
                        tempTile = this.upR;
                        break;
                    case 3:
                        tempTile = this.downR;
                        break;
                    case 4:
                        tempTile = this.below;
                        break;
                    case 5:
                        tempTile = this.downL;
                        break;
                    case 0:
                        tempTile = this.upL;
                        break;
                }
                tempTile.setTrueClientRpc(temp, type, rotation);
            }
        }
    }
}