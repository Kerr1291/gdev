using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    public class GameMain : GameSingleton<GameMain>
    {
        IEnumerator Start()
        {
            //let's pretend we're loading or setting up or something here
            yield return new WaitForSeconds( .2f );

            //create the data needed for the start of the game
            yield return GameDesigner.Instance.GenerateStart();

            //setup the player
            yield return PlayerManager.Instance.LoadPlayerAtStart();

            //load the map around the player
            yield return GameMap.Instance.LoadGameMap();

            //create player and start the game
            yield return PlayerManager.Instance.SpawnPlayer();
        }
    }
}