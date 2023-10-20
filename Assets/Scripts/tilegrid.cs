using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using UnityEngine.SceneManagement;

namespace TTD
{
    public class tilegrid : NetworkBehaviour
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private TMP_Text textBox;
        [SerializeField] private GameObject parentOStuff;
        [SerializeField] private TMP_Text turnBox;
        [SerializeField] public Card[] cardPieces;
        Dictionary<(int, int), Tile> tiles;
        Dictionary<int, int> amountOfTiles = new Dictionary<int, int>() { { 0, 3 }, { 1, 3 }, { 2, 3 }, { 3, 15 }, { 4, 15 }, { 5, 15 }, { 6, 8 }, { 7, 8 } };
        static NetworkVariable<bool> generated = new NetworkVariable<bool>();
        public static NetworkList<int> cardPile = new NetworkList<int>();
        public static NetworkList<ulong> players = new NetworkList<ulong>();
        List<Tile> playerTiles = new List<Tile>();
        List<int> cards = new List<int>();
        public int pcount;
        protected NetworkVariable<bool> _myTurn;
        public bool canTurnEnd = true;
        public bool turncheck;
        public bool myTurn
        {
            get
            {
                return _myTurn.Value;
            }
            set
            {
                setMyTurnServerRpc(value);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //cardPile?.Dispose();
            players?.Dispose();
        }
        public void Awake()
        {
            _myTurn = new NetworkVariable<bool>();
        }
        public void Update()
        {
            pcount = players.Count;
            if (turncheck != myTurn)
            {
                turncheck = myTurn;
                if (turncheck)
                {
                    turnBox.text = "Your Turn.";
                }
                else
                {
                    turnBox.text = "Not Your Turn.";
                }
            }
        }
        public void Disconnect()
        {
            DisServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        public void DisServerRpc()
        {
            DisClientRpc();
        }
        [ClientRpc]
        public void DisClientRpc()
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene(0);
        }
        [ServerRpc(RequireOwnership = false)]
        public void ResetGeneratedServerRpc()
        {
            generated.Value = false;
        }
        [ServerRpc(RequireOwnership = false)]
        public void setMyTurnServerRpc(bool val)
        {
            _myTurn.Value = val;
        }
        [ClientRpc]
        public void WinGameClientRpc(int ID)
        {
            EndGameServerRpc(ID);
        }
        [ServerRpc(RequireOwnership = false)]
        public void nextTurnServerRpc(bool monCardPlay)
        {
            if (canTurnEnd && !monCardPlay)
            {
                foreach (ulong puid in players)
                {
                    Debug.Log(puid);
                    if (NetworkManager.Singleton.ConnectedClients[puid].PlayerObject.GetComponent<tilegrid>().myTurn)
                    {
                        myTurn = false;
                        int nextIndex = players.IndexOf(puid) + 1;
                        if (nextIndex >= players.Count) nextIndex = 0;
                        ulong nextID = players[nextIndex];
                        NetworkManager.Singleton.ConnectedClients[nextID].PlayerObject.GetComponent<tilegrid>().myTurn = true;
                        break;
                    }
                }
                canTurnEnd = false;
                StartCoroutine(waitTurnEnd());
            }

        }
        IEnumerator waitTurnEnd()
        {
            yield return new WaitForSeconds(0.5f);
            canTurnEnd = true;
        }
        [ServerRpc(RequireOwnership = false)]
        public void EndGameServerRpc(int indexOfWinner)
        {
            SendEndClientRpc($"Player {indexOfWinner + 1} wins!");
        }
        [ClientRpc]
        public void SendEndClientRpc(string winMessage)
        {
            parentOStuff.SetActive(true);
            textBox.text = winMessage;
        }
        [ClientRpc]
        public void EndTurnClientRpc(ulong id)
        {
            if (NetworkManager.Singleton.LocalClientId == id)
            {
                int MaxActiveCards = 5;
                bool monsterCardUsed = false;
                foreach (Card card in cardPieces)
                {
                    if (card.activeCard)
                    {
                        card.useCard();
                        if (card.cardtype == 7)
                        {
                            MaxActiveCards = 6;
                        }
                        if (card.cardtype < 3)
                        {
                            monsterCardUsed = true;
                            MaxActiveCards = 3;
                        }
                    }
                }
                int activeCards = 0;
                int i = 0;
                System.Random random = new System.Random();
                while (i < 7)
                {
                    if (activeCards < MaxActiveCards)
                    {
                        if (!cardPieces[i].used)
                        {
                            activeCards++;
                        }
                        else
                        {
                            if (cardPile.Count != 0)
                            {
                                int key = random.Next(0, cardPile.Count);
                                cardPieces[i].gameObject.SetActive(true);
                                cardPieces[i].setCardType(cardPile[key]);
                                cardPieces[i].used = false;
                                RemoveFromPileServerRpc(cardPile[key]);
                                activeCards++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!cardPieces[i].used)
                        {
                            cardPieces[i].useCard();
                            cardPieces[i].gameObject.SetActive(false);
                        }
                    }
                    i++;
                }
                if (!monsterCardUsed)
                {
                    nextTurnServerRpc(monsterCardUsed);
                }
            }
        }
        IEnumerator drawstartinghand(ulong ID)
        {
            Debug.Log("Drawing");
            yield return new WaitForSeconds(1);
            if (players.Count == 1 || players.IndexOf(ID) == 0)
            {
                myTurn = true;
            }
            else
            {
                myTurn = false;
            }
            System.Random random = new System.Random();
            for (int i = 0; i < 5; i++)
            {
                int key = random.Next(0, cardPile.Count);
                cards.Add(cardPile[key]);
                RemoveFromPileServerRpc(cardPile[key]);
            }
            for (int i = 0; i < 5; i++)
            {
                cardPieces[i].cardtype = cards[i];
                cardPieces[i].gameObject.SetActive(true);
            }
            turnBox.gameObject.SetActive(true);

        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsClient)
            {
                GenerateGridServerRpc(NetworkManager.Singleton.LocalClientId);
            }
            if (IsOwner)
            {

                StartCoroutine(drawstartinghand(NetworkManager.Singleton.LocalClientId));
            }
            turncheck = myTurn;
            if (turncheck)
            {
                turnBox.text = "Your Turn.";
            }
            else
            {
                turnBox.text = "Not Your Turn.";
            }

        }

        [ServerRpc(RequireOwnership = false)]
        void RemoveFromPileServerRpc(int cardNum)
        {
            cardPile.Remove(cardNum);
        }


        [ServerRpc(RequireOwnership = false)]
        void GenerateGridServerRpc(ulong ID)
        {
            if (!players.Contains(ID))
            {
                players.Add(ID);
            }

            if (!generated.Value)
            {
                System.Random random = new System.Random();
                foreach (KeyValuePair<int, int> kvpair in amountOfTiles)
                {
                    for (int i = 0; i < kvpair.Value; i++)
                    {
                        cardPile.Add(kvpair.Key);
                    }
                }
                tiles = new Dictionary<(int, int), Tile>();
                // very complicated math to make the grid and center it
                for (int x = (-1 * _width) + 1; x < _width + 2; x += 2)
                {
                    int i = 0;
                    for (int y = (int)(-0.5f * _height) + (int)(0.25f * _height); y < (int)(0.75f * _height) - 1; y += 1)
                    {
                        Tile spawnedTile;
                        if (y % 2 == 0)
                        {
                            if (x < _width)
                            {
                                spawnedTile = Instantiate(_tilePrefab, new Vector3(x - .5f, y - i / 2, -.5f), Quaternion.identity);
                                spawnedTile.name = $"Tile {x} {y}";
                                spawnedTile.setXY(x, y);
                                spawnedTile.setPUIDServerRpc(6666);
                                //spawnedTile.transform.parent = gameObject.transform;
                                tiles[(x, y)] = spawnedTile;
                                spawnedTile.GetComponent<NetworkObject>().Spawn();
                                if ((x == -14 && y == -8) || (x == -14 && y == -7) || (x == -14 && y == 20) || (x == -14 && y == 19) || (x == 14 && y == -8) || (x == 16 && y == -7) || (x == 14 && y == 20) || (x == 16 && y == 19))
                                {
                                    spawnedTile.setSetClientRpc();
                                }
                                else if (x == 0 && y == 6)
                                {
                                    spawnedTile.setWinClientRpc();
                                }
                                if ((x == -14 && y == -8) || (x == -14 && y == 20) || (x == 14 && y == -8) || (x == 14 && y == 20))
                                {
                                    playerTiles.Add(spawnedTile);
                                }
                            }
                        }
                        else
                        {
                            spawnedTile = Instantiate(_tilePrefab, new Vector3(x - 1.5f, y - i / 2 - 0.5f, -.5f), Quaternion.identity);
                            spawnedTile.name = $"Tile {x} {y}";
                            spawnedTile.setXY(x, y);
                            spawnedTile.setPUIDServerRpc(6666);
                            //spawnedTile.transform.parent = gameObject.transform;
                            tiles[(x, y)] = spawnedTile;
                            spawnedTile.GetComponent<NetworkObject>().Spawn();
                            if ((x == -14 && y == -8) || (x == -14 && y == -7) || (x == -14 && y == 20) || (x == -14 && y == 19) || (x == 14 && y == -8) || (x == 16 && y == -7) || (x == 14 && y == 20) || (x == 16 && y == 19))
                            {
                                spawnedTile.setSetClientRpc();
                            }
                            else if (x == 0 && y == 6)
                            {
                                spawnedTile.setWinClientRpc();
                            }
                            if ((x == -14 && y == -8) || (x == -14 && y == 20) || (x == 14 && y == -8) || (x == 14 && y == 20))
                            {
                                playerTiles.Add(spawnedTile);
                            }
                        }
                        i++;
                    }
                }
                foreach (KeyValuePair<(int x, int y), Tile> tkvp in tiles)
                {
                    if (tkvp.Key.y % 2 == 0)
                    {
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y + 2)))
                        {
                            tkvp.Value.setAbove(tiles[(tkvp.Key.x, tkvp.Key.y + 2)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y - 2)))
                        {
                            tkvp.Value.setBelow(tiles[(tkvp.Key.x, tkvp.Key.y - 2)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y + 1)))
                        {
                            tkvp.Value.setUpL(tiles[(tkvp.Key.x, tkvp.Key.y + 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x + 2, tkvp.Key.y + 1)))
                        {
                            tkvp.Value.setUpR(tiles[(tkvp.Key.x + 2, tkvp.Key.y + 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y - 1)))
                        {
                            tkvp.Value.setDownL(tiles[(tkvp.Key.x, tkvp.Key.y - 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x + 2, tkvp.Key.y - 1)))
                        {
                            tkvp.Value.setDownR(tiles[(tkvp.Key.x + 2, tkvp.Key.y - 1)]);
                        }
                    }
                    else
                    {
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y + 2)))
                        {
                            tkvp.Value.setAbove(tiles[(tkvp.Key.x, tkvp.Key.y + 2)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y - 2)))
                        {
                            tkvp.Value.setBelow(tiles[(tkvp.Key.x, tkvp.Key.y - 2)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x - 2, tkvp.Key.y + 1)))
                        {
                            tkvp.Value.setUpL(tiles[(tkvp.Key.x - 2, tkvp.Key.y + 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y + 1)))
                        {
                            tkvp.Value.setUpR(tiles[(tkvp.Key.x, tkvp.Key.y + 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x - 2, tkvp.Key.y - 1)))
                        {
                            tkvp.Value.setDownL(tiles[(tkvp.Key.x - 2, tkvp.Key.y - 1)]);
                        }
                        if (tiles.ContainsKey((tkvp.Key.x, tkvp.Key.y - 1)))
                        {
                            tkvp.Value.setDownR(tiles[(tkvp.Key.x, tkvp.Key.y - 1)]);
                        }
                    }
                }
                generated.Value = true;
            }
            Tile tile = playerTiles[players.Count - 1];
            tile.setPUIDClientRpc(ID);
        }
    }
}