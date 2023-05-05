using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public int totalEnemyUnits = 100;
    public int totalFriendlyUnits = 100;

    public int maximum = 100;
    public int currentEnemy;
    public int currentFriendly;
    public Image enemyProgMask;
    public Image friendlyProgMask;

    public int control;
    public PlayerMapAreas PMA;

    // Start is called before the first frame update
    void Start()
    {
       // totalFriendlyUnits = 0;
        


        

    }

    // Update is called once per frame
    void Update()
    {
        //if (totalFriendlyUnits <= 0)
        //{
        //    SceneManager.LoadScene("End Enemy");
        //}
        //if (totalEnemyUnits <= 0)
        //{
        //    SceneManager.LoadScene("End Friendly");
        //}

        control = PMA.control;
        if (control < 0)
        {
            control *= -1;
            currentEnemy = control;
        }
        else
        {
            currentFriendly = control;
        }
        GetCurrentFill();
    }
    void GetCurrentFill()
    {
        float fillAmountFriendly = (float)currentFriendly / (float)maximum;
        float fillAmountEnemy = (float)currentEnemy / (float)maximum;

        friendlyProgMask.fillAmount = fillAmountFriendly;
        enemyProgMask.fillAmount = fillAmountEnemy;


    }
}
