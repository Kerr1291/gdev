using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    ///
    //TODO:
    //will contain references to a collection of MapRenderers and be responsible for positioning them in the world
    //ideas for implementations:
    // <link missing, need to upload the file myself later...>
    // https://forum.unity.com/threads/seamless-tiled-infinite-terrain-simple-script-for-easy-fix-of-seams-and-edit.260188/ <-- an addon for infiniteterrain.cs?
    // https://forum.unity.com/threads/infinite-terrain-free-project-source.68807/ <-- not sure
    // https://gist.github.com/bsimser/5a07ca1cbc7634c5c566 <-- also worth a look ?
    //
    // 
    //
    //
    ///

    public class GameMap : GameSingleton<GameMap>
    {
        //for now, we are only having one map renderer
        public MapRenderer mapRenderer = null;

        //TODO: temporary, just to test some basic functionality for now

        public List<ProcGenMap> procGenMaps = new List<ProcGenMap>();

        //TODO: temporary index to test basic functionality
        int currentIndex = 0;
        public IEnumerator LoadGameMap()
        {
            //TODO: get the center of the game map (which is the area the player is in) and generate the map 
            //      using data pulled from the game designer

            if (procGenMaps.Count == 0)
                yield break;

            yield return mapRenderer.UpdateMapWithData(procGenMaps[currentIndex]);
        }

        [ContextMenu("Replace map content test")]
        public void UpdateMapContent()
        {
            currentIndex = (currentIndex + 1) % procGenMaps.Count;
            StartCoroutine(LoadGameMap());
        }

    }
}