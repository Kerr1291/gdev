using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv.Game
{
    //TODO: also make xml serializable??? (just a note as a reminder)
    //class that contains info about game content to be generated
    [Serializable]
    public class GameContent
    {
        public string type;

        public GameContent(string type = "null")
        {
            this.type = type;
        }
    }
}