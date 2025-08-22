using System;
using TMPro;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.Components;

public class DominationTimeManager : MonoBehaviour
{
    public static GameObject Panel;
    
    public static TextMeshProUGUI Timer;
    public float timeRemaining;
    
    public bool started = false;
    
    public void Bind(GameObject source)
    { 
        Panel = source;
    }
    
    public void Start()
    {
        if(GameManager.CurrentGame.gameSettingsArray["gamemode"] == 1  && this.gameObject != null)
        {
            if(GameManager.IsInBingoLevel)
            {
                Timer = GetGameObjectChild(Panel,"Timer").GetComponent<TextMeshProUGUI>();
                Panel.SetActive(false); 
            }
            timeRemaining = GameManager.dominationTimer;
        }
    }

    public void Update()
    {
        if(GameManager.IsInBingoLevel && GameManager.CurrentGame.gameSettingsArray["gamemode"] == 1)
        {
            timeRemaining = Mathf.MoveTowards(timeRemaining,0f,Time.unscaledDeltaTime);
            GameManager.dominationTimer = timeRemaining;
        
            float secs = timeRemaining;
            float mins = 0;
            while (secs >= 60f)
            {
                secs -= 60f;
                mins += 1f;
            }
        
            if(GameManager.IsInBingoLevel && Panel != null)
            {
                Timer.text = "<color=orange>"+mins+":"
                             +(secs < 10f ? ((int)secs).ToString("D2") : (int)secs)
                             +"</color>";
                if(MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame || PlayerPrefs.GetInt("LevStaOpe") == 1)
                {
                    Panel.SetActive(true);
                }
                else if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasCanceledThisFrame)
                {
                    Panel.SetActive(false);
                }
            }
        }
    }
}