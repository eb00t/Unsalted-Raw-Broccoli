using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class globalStatsScript : MonoBehaviour
{
    public float deathCounter;
    public TextMeshProUGUI totalDeaths;
  //  public GameObject deathScreen;

    void Start()
    {
        if (PlayerPrefs.GetFloat("DeathCounter") != null)
        {
            deathCounter = PlayerPrefs.GetFloat("DeathCounter");
        }

        totalDeaths.text = "Total Deaths = " + deathCounter;
    }

    void Update()
    {
      //  saveData();
    }

    public void saveData()
    {
        PlayerPrefs.SetFloat("DeathCounter", deathCounter);
    }

    public void loadData()
    {
        PlayerPrefs.GetFloat("DeathCounter");
    }

    public void oneDeath()
    {
        deathCounter++;
        totalDeaths.text = "Total Deaths = " + deathCounter;
    }

    public void deleteData()
    {
        PlayerPrefs.DeleteKey("DeathCounter");
    }

    /*
    // Start is called before the first frame update
    void Start()
    {
       if(PlayerPrefs.GetFloat("DeathCounter") != null)
        {
            deathCounter = PlayerPrefs.GetFloat("DeathCounter");
        }

        totalDeaths.text = "Total Deaths = " + deathCounter;
    }

    // Update is called once per frame
    void Update()
    {
        if(deathScreen.activeInHierarchy == true)
        {
            deathCounter++;
            PlayerPrefs.SetFloat("DeathCounter", deathCounter);
        }
    }
    */
}
