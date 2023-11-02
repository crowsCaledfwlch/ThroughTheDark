using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace TTD
{
    public class Card : MonoBehaviour
    {
        [SerializeField]
        Sprite[] sprite;
        public int cardtype;
        public static Card lastCard;
        public bool used = false;
        public bool activeCard = false;
        tilegrid playerObject;
        void Start()
        {
            setCardType(cardtype);
            if (NetworkManager.Singleton.IsClient) playerObject = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<tilegrid>();
        }

        public void useCard()
        {
            used = true;
            activeCard = false;
            transform.position = new Vector3(transform.position.x, -10, transform.position.z);
        }

        public void setCardType(int cardtype)
        {
            this.cardtype = cardtype;
            GetComponent<SpriteRenderer>().sprite = sprite[cardtype];
        }

        void OnMouseOver()
        {
            if (transform.position.y < -7)
            {

                transform.position = new Vector3(transform.position.x, transform.position.y + .1f, transform.position.z);
            }
        }
        public void OnMouseExit()
        {
            transform.position = new Vector3(transform.position.x, -10, transform.position.z);
        }

        void OnMouseDown()
        {
            if (playerObject.myTurn)
            {
                int t, s;
                t = 0;
                s = 0;
                switch (cardtype)
                {
                    case 0:
                        t = 2;
                        s = 0;
                        break;
                    case 1:
                        t = 2;
                        s = 1;
                        break;
                    case 2:
                        t = 2;
                        s = 2;
                        break;
                    case 3:
                        t = 0;
                        s = 0;
                        break;
                    case 4:
                        t = 0;
                        s = 2;
                        break;
                    case 5:
                        t = 0;
                        s = 1;
                        break;
                    case 6:
                        t = 3;
                        s = 0;
                        break;
                    case 7:
                        t = 1;
                        s = 0;
                        break;
                }
                Tile.setTS(t, s);
                if (lastCard != null)
                {
                    if (lastCard.used != true)
                    {
                        lastCard.gameObject.SetActive(true);
                        lastCard.OnMouseExit();
                        lastCard.activeCard = false;
                    }
                }
                lastCard = this;
                activeCard = true;
                gameObject.SetActive(false);
            }
        }
    }
}