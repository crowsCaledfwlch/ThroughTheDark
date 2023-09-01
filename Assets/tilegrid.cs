using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace TTD
{
    public class tilegrid : NetworkBehaviour
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        Dictionary<(int, int), Tile> tiles;

        bool generated = false;

        public override void OnNetworkSpawn()
        {
            GenerateGridServerRpc();
        }

        [ServerRpc]
        void GenerateGridServerRpc()
        {
            if (!generated)
            {
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
                                spawnedTile = Instantiate(_tilePrefab, new Vector3(x - .5f, y - i / 2, -1), Quaternion.identity);
                                spawnedTile.GetComponent<NetworkObject>().Spawn();
                                spawnedTile.name = $"Tile {x} {y}";
                                //spawnedTile.transform.parent = gameObject.transform;
                                tiles[(x, y)] = spawnedTile;
                            }
                        }
                        else
                        {
                            spawnedTile = Instantiate(_tilePrefab, new Vector3(x - 1.5f, y - i / 2 - 0.5f, -1), Quaternion.identity);
                            spawnedTile.GetComponent<NetworkObject>().Spawn();
                            spawnedTile.name = $"Tile {x} {y}";
                            //spawnedTile.transform.parent = gameObject.transform;
                            tiles[(x, y)] = spawnedTile;
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

        // Update is called once per frame
        void Update()
        {

        }
    }
}