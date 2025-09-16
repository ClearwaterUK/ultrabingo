using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class MapPool
{
    public int MapPoolId;
    public string MapPoolName;
    public string MapPoolDescription;
    public int MapPoolLevelCount;
}

public class MapPoolResponse : MessageResponse
{
    public List<MapPool> mapPools;
}

public static class MapPoolResponseHandler
{
    public static void handle(MapPoolResponse response)
    {
        BingoMapPoolSelection.setupMapPools(response.mapPools);
    }
}