using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv.Game
{
    [Serializable]
    public class HitData
    {
        public GameObject owner;

        public int damage;

        public Bounds hitZone;
    }
}