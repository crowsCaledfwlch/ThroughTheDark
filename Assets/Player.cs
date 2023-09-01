using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTD
{
    public class Player
    {
        public int rotation;
        public bool active;
        private static Player _player1;
        public static Player player1
        {
            get
            {
                if (_player1 == null)
                {
                    _player1 = new Player();
                }
                return _player1;
            }
        }
        private static Player _player2;
        public static Player player2
        {
            get
            {
                if (_player2 == null)
                {
                    _player2 = new Player();
                }
                return _player2;
            }
        }
        private static Player _player3;
        public static Player player3
        {
            get
            {
                if (_player3 == null)
                {
                    _player3 = new Player();
                }
                return _player3;
            }
        }
        private static Player _player4;
        public static Player player4
        {
            get
            {
                if (_player4 == null)
                {
                    _player4 = new Player();
                }
                return _player4;
            }
        }
        public static Player activePlayer
        {
            get
            {
                if (player1.active)
                {
                    return player1;
                }
                else if (player2.active)
                {
                    return player2;
                }
                else if (player3.active)
                {
                    return player3;
                }
                else
                {
                    return player4;
                }
            }
        }
    }
}