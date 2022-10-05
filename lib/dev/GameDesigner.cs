using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    //this class will be used when the game needs to request a kind of a content to be generated
    public class GameDesigner : GameSingleton<GameDesigner>
    {
        public IEnumerator GenerateStart()
        {
            GenerateDemoStart();
            yield break;
        }

        void GenerateDemoStart()
        {
            //TODO:

            //generate dungeon
            GameContent dungeon = new GameContent( "Dungeon" );

            //generate npc?

            //generate town

            //generate player start?


        }
    }
}