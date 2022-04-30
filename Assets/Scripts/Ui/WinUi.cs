using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinUi : MonoBehaviour
{
    public Text winText;
    public bool activated = false;

    private void Start()
    {
        winText.transform.parent.gameObject.SetActive(false);
    }

    public void SetWinnerText(string _winText)
    {
        winText.transform.parent.gameObject.SetActive(true);
        winText.text = _winText.ToUpper();
        activated = true;
    }
}
