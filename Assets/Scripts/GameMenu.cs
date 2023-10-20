using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine.UI;

namespace TTD
{
    public class GameMenu : MonoBehaviour
    {
        public GameObject parentofstuff;
        public TMP_InputField color;
        public TMP_InputField ip;
        public TMP_Text connection;
        public TMP_Text errorbox;
        public string locip;
        private void Start()
        {

        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    connection.text = "Connected to: " + ip.ToString();
                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
        public void OnStartHost()
        {
            Color newCol = Color.magenta;
            if (color.text != "#ffffff" && color.text != "#000000" && ColorUtility.TryParseHtmlString(color.text, out newCol) || color.text == "")
            {
                locip = GetLocalIPAddress();
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                locip,  // The IP address is a string
                (ushort)7778, // The port number is an unsigned short
                "0.0.0.0"
            );
                if (newCol == Color.white) newCol = Color.magenta;

                NetworkManager.Singleton.StartHost();
                errorbox.text = "Loading you in...";
                StartCoroutine(SetColor(newCol));
            }
            else
            {
                errorbox.text = "Invalid color.";
            }
        }
        IEnumerator SetColor(Color col)
        {
            yield return new WaitForSeconds(2);
            Tile.addPlayerColor(col);
            parentofstuff.SetActive(false);
        }
        public void OnStartClient()
        {
            Color newCol = Color.magenta;
            if (color.text != "#ffffff" && color.text != "#000000" && ColorUtility.TryParseHtmlString(color.text, out newCol) || color.text == "")
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                ip.text,  // The IP address is a string
                (ushort)7778, // The port number is an unsigned short
                "0.0.0.0"
            );
                if (newCol == Color.white) newCol = Color.magenta;
                if (NetworkManager.Singleton.StartClient())
                {
                    connection.text = "Connected to: " + ip.text;
                    errorbox.text = "Loading you in...";
                    StartCoroutine(SetColor(newCol));
                }
                else
                {
                    errorbox.text = "Failed to Connect!";
                }
            }
            else
            {
                errorbox.text = "Invalid color.";
            }
        }
    }
}