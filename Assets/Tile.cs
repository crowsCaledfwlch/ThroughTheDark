using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;

namespace TTD
{
    public class Tile : NetworkBehaviour
    {
        public Tile above, upR, downR, below, downL, upL; // 1 2 3 4 5 6. 1->3->6->4->5->2->1
        private int x, y;
        [SerializeField] private NetworkVariable<ulong> uidOnTile = new NetworkVariable<ulong>();
        public NetworkVariable<bool> set = new NetworkVariable<bool>();
        public NetworkVariable<bool> win = new NetworkVariable<bool>();

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
        private SpriteRenderer spriteRenderer;
        private NetworkVariable<Color> setColor = new NetworkVariable<Color>();
        bool mouseOver = false;
        void Start()
        {
            tileBase = this;
            if (NetworkManager.Singleton.IsServer)
            {
                setColor.Value = Color.white;
                spriteRenderer = GetComponent<SpriteRenderer>();
                set.Value = false;
                win.Value = false;
                type.Value = 0;
                shape.Value = 0;
                rotation.Value = 0;
            }
            if (NetworkManager.Singleton.IsClient) playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<tilegrid>();
        }

        void FixedUpdate()
        {

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer.color != setColor.Value) spriteRenderer.color = setColor.Value;
            if (uidOnTile.Value != 666) { spriteRenderer.color = Color.magenta; }
        }
        public void setXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        [ClientRpc]
        public void setPUIDClientRpc(ulong uid)
        {
            setPUIDServerRpc(uid);
        }
        [ServerRpc(RequireOwnership = false)]
        public void setPUIDServerRpc(ulong uid)
        {
            if (this.uidOnTile.Value != 666 && uid == 6666)
            {
                this.uidOnTile.Value = 666;
            }
            else if (uid != 6666)
            {
                this.uidOnTile.Value = uid;
            }
        }
        [ClientRpc]
        public void setSetClientRpc()
        {
            setSetExternalServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        public void setInternalServerRpc()
        {
            this.set.Value = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void setColorServerRpc(Color color)
        {
            this.setColor.Value = color;
        }
        [ServerRpc(RequireOwnership = false)]
        public void setSetExternalServerRpc()
        {
            StartCoroutine(starterSet());
        }
        IEnumerator starterSet()
        {
            yield return new WaitForSeconds(.5f);
            this.setColor.Value = Color.yellow;
            this.set.Value = true;
        }
        [ClientRpc]
        public void setWinClientRpc()
        {
            setWinExternalServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        public void setWinExternalServerRpc()
        {
            StartCoroutine(starterWin());
        }
        IEnumerator starterWin()
        {
            yield return new WaitForSeconds(.5f);
            this.setColor.Value = Color.green;
            this.win.Value = true;
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
                if (Input.GetMouseButton(1) && mouseOver && playerObject.myTurn)
                {
                    checkProcServerRpc(3, NetworkManager.Singleton.LocalClientId);
                }
            }
        }

        SpriteRenderer GetSpriteRenderer() => spriteRenderer;

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
        public bool radialWinCheck()
        {
            bool r = false;
            if (this.above != null) if (this.above.win.Value) r = true;
            if (this.upR != null) if (this.upR.win.Value) r = true;
            if (this.downR != null) if (this.downR.win.Value) r = true;
            if (this.below != null) if (this.below.win.Value) r = true;
            if (this.downL != null) if (this.downL.win.Value) r = true;
            if (this.upL != null) if (this.upL.win.Value) r = true;
            return r;
        }

        public bool setOrWin()
        {
            return this.set.Value || this.win.Value;
        }

        (bool, int) checkTiles(int[] ints, Tile tile, int rotation)
        {
            if (ints.Length == 0 || tile == null)
            {
                return (!tile.setOrWin(), tile.radialSetCheck());
            }
            else
            {
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;

                if (ints[0] == 0) return (!tile.setOrWin(), tile.radialSetCheck());
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
                return (result.Item1 && !tile.setOrWin(), tile.radialSetCheck() + result.Item2);
            }
        }

        (bool, int, bool) checkTiles(int[] ints, Tile tile, int rotation, ulong ID)
        {
            if (ints.Length == 0 || tile == null)
            {
                return (!tile.setOrWin(), tile.radialSetCheck(), tile.radialWinCheck());
            }
            else
            {
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;

                if (ints[0] == 0) return (!tile.setOrWin(), tile.radialSetCheck(), tile.radialWinCheck());
                switch ((ints[0] + rotation) % 6)
                {
                    case 1:
                        if (tile.above == null) return (false, 0, false);
                        tempTile = tile.above;
                        break;
                    case 2:
                        if (tile.upR == null) return (false, 0, false);
                        tempTile = tile.upR;
                        break;
                    case 3:
                        if (tile.downR == null) return (false, 0, false);
                        tempTile = tile.downR;
                        break;
                    case 4:
                        if (tile.below == null) return (false, 0, false);
                        tempTile = tile.below;
                        break;
                    case 5:
                        if (tile.downL == null) return (false, 0, false);
                        tempTile = tile.downL;
                        break;
                    case 0:
                        if (tile.upL == null) return (false, 0, false);
                        tempTile = tile.upL;
                        break;
                }
                (bool, int, bool) result = checkTiles(temp, tempTile, rotation, ID);
                return (result.Item1 && !tile.setOrWin(), tile.radialSetCheck() + result.Item2, tile.radialWinCheck() || result.Item3);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void checkProcServerRpc(int EED, ulong ID)
        {
            tilegrid tgrid = NetworkManager.Singleton.ConnectedClients[ID].PlayerObject.GetComponent<tilegrid>();
            if (tgrid.myTurn)
            {
                if (EED == 0)
                { // OO
                    if (!set.Value && type.Value != -1)
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
                    if (!set.Value && type.Value != -1)
                    {
                        (int, int, int, int) nextStep = tilesPositions[(type.Value, shape.Value)];
                        (bool, int, bool) result = checkTiles(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, this, rotation.Value, ID);
                        if (result.Item1 && result.Item2 > 1)
                        {

                            if (type.Value != 3 || !result.Item3)
                            {

                                colorTilesClientRpc(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, type.Value, rotation.Value);
                                setTrueClientRpc(new int[] { nextStep.Item1, nextStep.Item2, nextStep.Item3, nextStep.Item4 }, type.Value, rotation.Value);
                                tgrid.EndTurnClientRpc();
                                type.Value = -1;
                            }
                        }
                    }
                }
                else if (EED == 3)
                {
                    (ulong, bool, Tile) result = getPath(0);
                    if (result.Item2)
                    {
                        result.Item3.setPUIDServerRpc(666);
                        setPUIDServerRpc(ID);
                        if (win.Value == true)
                        {
                            tgrid.WinGameClientRpc((int)ID);
                        }
                    }
                }
                else
                { // OMEx and catch
                    if (!set.Value && type.Value != -1)
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

        [ServerRpc(RequireOwnership = false)]
        void colorTilesServerRpc(int[] ints, int type, int rotation)
        {
            colorTilesClientRpc(ints, type, rotation);
        }

        [ClientRpc]
        void colorTilesClientRpc(int[] ints, int type, int rotation)
        {
            if (ints.Length == 0)
            {
                switch (type)
                {
                    case -1:
                        setColorServerRpc(Color.white);
                        break;
                    case 0:
                        setColorServerRpc(Color.yellow);
                        break;
                    case 1:
                        setColorServerRpc(Color.blue);
                        break;
                    case 2:
                        setColorServerRpc(Color.red);
                        break;
                    case 3:
                        setColorServerRpc(Color.black);
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
                        setColorServerRpc(Color.white);
                        break;
                    case 0:
                        setColorServerRpc(Color.yellow);
                        break;
                    case 1:
                        setColorServerRpc(Color.blue);
                        break;
                    case 2:
                        setColorServerRpc(Color.red);
                        break;
                    case 3:
                        setColorServerRpc(Color.black);
                        break;
                }
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponent<SpriteRenderer>();
                }
                spriteRenderer.color = setColor.Value;

                if (ints[0] == 0) return;
                try
                {
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
                catch (Exception e)
                {

                }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        void setTrueServerRpc(int[] ints, int type, int rotation)
        {
            setTrueClientRpc(ints, type, rotation);
        }

        [ClientRpc]
        void setTrueClientRpc(int[] ints, int type, int rotation)
        {

            if (ints.Length == 0)
            {
                setInternalServerRpc();
            }
            else
            {
                setInternalServerRpc();
                int[] temp = new int[ints.Length - 1];
                for (int i = 1; i < ints.Length; i++)
                {
                    temp[i - 1] = ints[i];
                }
                Tile tempTile = null;
                if (ints[0] == 0) return;
                try
                {
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
                catch (Exception e)
                {

                }
            }
        }
        public (ulong, bool, Tile) getPath(int steps)
        {
            (ulong, bool, Tile) result = (this.uidOnTile.Value, false, this);
            if (steps < 3)
            {
                if (set.Value || win.Value)
                {
                    if (uidOnTile.Value != 666)
                    {
                        result.Item1 = this.uidOnTile.Value;
                        result.Item2 = true;
                        result.Item3 = this;
                    }
                    else
                    {
                        if (this.above != null)
                        {
                            result.Item2 = result.Item2 || this.above.getPath(steps + 1).Item2;
                            if (this.above.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.above.getPath(steps + 1).Item3;
                            }
                        }
                        if (this.downR != null)
                        {
                            result.Item2 = result.Item2 || this.downR.getPath(steps + 1).Item2;
                            if (this.downR.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.downR.getPath(steps + 1).Item3;
                            }
                        }
                        if (this.downL != null)
                        {
                            result.Item2 = result.Item2 || this.downL.getPath(steps + 1).Item2;
                            if (this.downL.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.downL.getPath(steps + 1).Item3;
                            }
                        }
                        if (this.upR != null)
                        {
                            result.Item2 = result.Item2 || this.upR.getPath(steps + 1).Item2;
                            if (this.upR.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.upR.getPath(steps + 1).Item3;
                            }
                        }
                        if (this.upL != null)
                        {
                            result.Item2 = result.Item2 || this.upL.getPath(steps + 1).Item2;
                            if (this.upL.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.upL.getPath(steps + 1).Item3;
                            }
                        }
                        if (this.below != null)
                        {
                            result.Item2 = result.Item2 || this.below.getPath(steps + 1).Item2;
                            if (this.below.getPath(steps + 1).Item2)
                            {
                                result.Item1 = this.uidOnTile.Value;
                                result.Item3 = this.below.getPath(steps + 1).Item3;
                            }
                        }
                    }
                }
                else
                {
                    result.Item2 = false;
                }
            }
            return result;
        }
    }
}