using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace TTD
{
    public class InstMenu : MonoBehaviour
    {
        public GameObject back;
        public GameObject next;
        int pageNum = 0;
        public GameObject[] pages;
        void Awake()
        {
            pages[pageNum].SetActive(true);
            pages[pageNum].GetComponent<Page>().SetPageNum(pageNum + 1);
        }
        public void OnBackButton()
        {
            SceneManager.LoadScene(0);
        }
        public void OnNextButton()
        {
            pages[pageNum].SetActive(false);
            pageNum++;
            if (pageNum >= pages.Length)
            {
                pageNum = 0;
            }
            if (pageNum > 0)
            {
                back.SetActive(true);
            }
            if (pageNum == pages.Length - 1)
            {
                next.SetActive(false);
            }
            pages[pageNum].SetActive(true);
            pages[pageNum].GetComponent<Page>().SetPageNum(pageNum + 1);

        }
        public void OnPrevButton()
        {
            pages[pageNum].SetActive(false);
            pageNum--;
            if (pageNum < 0)
            {
                pageNum = pages.Length - 1;
            }
            if (pageNum == 0)
            {
                back.SetActive(false);
            }
            if (pageNum < pages.Length - 1)
            {
                next.SetActive(true);
            }
            pages[pageNum].SetActive(true);
            pages[pageNum].GetComponent<Page>().SetPageNum(pageNum + 1);
        }
    }
}