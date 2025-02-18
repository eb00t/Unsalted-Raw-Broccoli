using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class globalStatsScript : MonoBehaviour
{
    public float deathCounter;
    public TextMeshProUGUI totalDeaths;
    //public GameObject deathScreen;

    void OnEnable()
    {
        oneDeath();
        loadData();
       // Debug.Log("AWAKE CALLED");
    }

    void Start()
    {
        loadData();

        if (PlayerPrefs.GetFloat("DeathCounter") != null)
        {
            deathCounter = PlayerPrefs.GetFloat("DeathCounter");
        }

        totalDeaths.text = "Total Deaths = " + deathCounter;
    }

    void Update()
        {
              saveData();
        }

    public void oneDeath()
    {
        //Adds a death to the counter
        deathCounter += 1;
        totalDeaths.text = "Total Deaths = " + deathCounter;
    }

    public void saveData()
        {
            //Saves the current number that is dispalyed
            PlayerPrefs.SetFloat("DeathCounter", deathCounter);
        }

    public void loadData()
        {
            //Loads the counter value that is saved
            PlayerPrefs.GetFloat("DeathCounter");
        }

    public void deleteData()
        {
            PlayerPrefs.DeleteKey("DeathCounter");
        }

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