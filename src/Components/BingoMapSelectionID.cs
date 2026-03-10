using Newtonsoft.Json;
using UnityEngine;

namespace UltraBINGO.Components;

[JsonObject(MemberSerialization.OptIn)]
public class BingoMapSelectionID : MonoBehaviour
{
    [JsonProperty] public string levelName = "";
    [JsonProperty] public string levelId = "";
    [JsonProperty] public BingoLevelType levelType = BingoLevelType.Campaign;
    [JsonProperty] public string UltraEditorLevelData = null;
    [JsonProperty] public string UltraEditorImageURL = null;
    [JsonProperty] public string UltraEditorImageName = null;
    [JsonProperty] public string angryBundleId = "";
    [JsonProperty] public string thumbnailPath = "";
}