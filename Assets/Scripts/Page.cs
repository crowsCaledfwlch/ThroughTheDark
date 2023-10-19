using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace TTD
{
    public class Page : MonoBehaviour
    {
        [SerializeField] private TMP_Text tmp;
        private int pageNum = 1;
        public void SetPageNum(int pn)
        {
            pageNum = pn;
            tmp.text = "" + pageNum;
        }
        void Awake()
        {
            tmp.text = "" + pageNum;
        }
    }
}