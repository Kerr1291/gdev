using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv.Game
{
    //TODO: move "hit effect" and "death effect" into a shared base class?
    public class DeathEffect : MonoBehaviour
    {
        public AudioClipSet effectSound;
        public ParticleSystem effectParticles;       
        
        //TOOD: replace the effect particles with this?
        //public SpawnerPoint spawnerPoint = default;

        //where to "play the effect" at
        public virtual Vector3 GetHitFXPosition()
        {
            return transform.position;
        }

        //if the death effect should be delayed then this logic should move into a coroutine in this class and that will be started here
        public void Play()
        {
            if( effectSound?.Clips.Count > 1 )
                effectSound?.PlayRandom( canPlayPreviousClip: false );
            else
                effectSound?.Play();

            //TODO: see note at reference of spawnerPoint
            //spawnerPoint?.Spawn( body.gameObject, 1f );
            
            if( effectParticles != null )
            {
                ParticleSystem hitFX = GameObject.Instantiate( effectParticles );
                Vector3 hitPos = GetHitFXPosition();
                hitFX.transform.position = hitPos;
                hitFX.gameObject.SetActive( true );
                Destroy( hitFX.gameObject, hitFX.main.duration );
            }
        }
    }
}