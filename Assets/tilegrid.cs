using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

namespace TTD
{
    public class tilegrid : NetworkBehaviour
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private TMP_Text textBox;
        [SerializeField] public Card[] cardPieces;
        Dictionary<(int, int), Tile> tiles;
        Dictionary<int, int> amountOfTiles = new Dictionary<int, int>() { { 0, 3 }, { 1, 3 }, { 2, 3 }, { 3, 15 }, { 4, 15 }, { 5, 15 }, { 6, 8 }, { 7, 8 } };
        bool generated = false;
        List<int> cards = new List<int>();
        NetworkList<int> cardPile = new NetworkList<int>();
        NetworkList<ulong> players = new NetworkList<ulong>();
        public bool myTurn;
        [ClientRpc]
        public void WinGameClientRpc()
        {
            EndGameServerRpc(players.IndexOf(NetworkManager.Singleton.LocalClientId));
        }
        [ServerRpc(RequireOwnership = false)]
        public void EndGameServerRpc(int indexOfWinner)
        {
            SendEndClientRpc($"Player {indexOfWinner + 1} wins!");
        }
        [ClientRpc]
        public void SendEndClientRpc(string winMessage)
        {
            textBox.text = winMessage;
        }
        [ClientRpc]
        public void EndTurnClientRpc()
        {
            foreach (Card card in cardPieces)
            {
                if (card.activeCard)
                {
                    card.useCard();
                }
            }
            myTurn = false;
            int activeCards = 0;
            int i = 0;
            System.Random random = new System.Random();
            while (activeCards < 5)
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
                i++;
            }
            setNextTurnServerRpc(players.IndexOf(NetworkManager.Singleton.LocalClientId) + 1 % players.Count);
        }
        IEnumerator drawstartinghand()
        {
            yield return new WaitForSeconds(1);
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

        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (players.Count == 0)
            {
                myTurn = true;
            }
            GenerateGridServerRpc(NetworkManager.Singleton.LocalClientId);
            if (IsOwner) StartCoroutine(drawstartinghand());
        }

        [ServerRpc(RequireOwnership = false)]
        void setNextTurnServerRpc(int ID)
        {
            NetworkManager.Singleton.ConnectedClients[players[ID]].PlayerObject.GetComponent<tilegrid>().setTurnOnClientRpc();
        }
        [ClientRpc]
        void setTurnOnClientRpc()
        {
            myTurn = true;
        }

        [ServerRpc(RequireOwnership = false)]
        void RemoveFromPileServerRpc(int cardNum)
        {
            cardPile.Remove(cardNum);
        }


        [ServerRpc(RequireOwnership = false)]
        void GenerateGridServerRpc(ulong ID)
        {
            players.Add(ID);
            if (!generated)
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
                            }
                        }
                        else
                        {
                            spawnedTile = Instantiate(_tilePrefab, new Vector3(x - 1.5f, y - i / 2 - 0.5f, -.5f), Quaternion.identity);
                            spawnedTile.name = $"Tile {x} {y}";
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
                generated = true;
            }

        }
    }
}