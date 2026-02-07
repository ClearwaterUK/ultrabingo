using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

    [HarmonyPatch(typeof(CheatsController), "ProcessInput")]
    public class CheatCombinationDisabler
    {
        [HarmonyPostfix]
        //Explode the player if they try to activate the Konami code in a bingo game
        public static void disableCheatingAttempts(GameObject ___consentScreen, int ___sequenceIndex)
        {
            if(GameManager.IsInBingoLevel && !GameManager.TriedToActivateCheats)
            {
                if(___consentScreen.activeSelf)
                {
                    MonoSingleton<OptionsManager>.Instance.UnPause();
                    ___consentScreen.SetActive(false);
                    
                    GameObject deathScreen = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"), "BlackScreen"), "YouDiedText");
                    //Need to disable the TextOverride component.
                    Component[] test = deathScreen.GetComponents(typeof(Component));
                    Behaviour bhvr = (Behaviour)test[3];
                    bhvr.enabled = false;

                    Text youDiedText = GetTextfromGameObject(deathScreen);
                    youDiedText.text = "NUH UH" + "\n\n\n\n\n" + "NOW DON'T DO IT AGAIN";
                    
                    Shotgun playerShotgun = GameObject.Find("Player/Main Camera/Guns/Shotgun Pump(Clone)").GetComponentInChildren<Shotgun>();
                    NewMovement playerLogic = GameObject.Find("Player").GetComponentInChildren<NewMovement>();
                    playerLogic.hp = 1;
                    
                    GameObject deathSplosion = UnityEngine.Object.Instantiate<GameObject>(playerShotgun.explosion, playerShotgun.transform.position,playerShotgun.transform.rotation);
                    
                    GameManager.HumiliateSelf();
                    GameManager.TriedToActivateCheats = true;
                }
            }
        }
    }