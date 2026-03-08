using System;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;

namespace UltraBINGO.Components;

public enum BingoLevelType
{
    Campaign,
    Angry,
    UltraEditor
}

public class BingoLevelData : MonoBehaviour
{
    public bool isClaimed = false;
    public string claimedTeam = "none";
    public string claimedPlayer = "none";
    
    public int row;
    public int column;
    
    public float timeRequirement = 0;
    
    public string levelName = "";
    
    public string angryParentBundle = "";
    public string angryLevelId = "";
    public string ultraEditorLevelData = null;
    
    public BingoLevelType bingoLevelType = BingoLevelType.Campaign;
}