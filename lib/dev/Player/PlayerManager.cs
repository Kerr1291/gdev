using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    //responsible for managing the game's player(s)
    public class PlayerManager : GameSingleton<PlayerManager>
    {
        //TODO: review the proper way to spawn the player, as spawnerPoint is a bit hacky atm
        
        
        [Header("Spawn Player parameters")]
        public SpawnerPoint spawnerPoint = default;
        public PlayerController player = default;
        public float spanwDelay = 1.0f;

        [Header("Other references")]
        public PlayerMapTransition playerMapTransition = default;

        public IEnumerator LoadPlayerAtStart()
        {
            //TODO: get the start location from the game designer and place the player there

            yield break;
        }

        public IEnumerator SpawnPlayer()
        {
            //TODO: spawn the player and "enable" game play
            playerMapTransition.enabled = false;
            yield return spawnerPoint.SpawnEnumerator(player.gameObject,spanwDelay);

            playerMapTransition.enabled=true;
        }

        public void MovePlayerToSpawnPoint()
        {
            player.transform.position = spawnerPoint.transform.position;
        }
    }
}