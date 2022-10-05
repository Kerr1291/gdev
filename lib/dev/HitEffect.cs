using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv.Game
{
    //TODO: move "hit effect" and "death effect" into a shared base class?
    public class HitEffect : MonoBehaviour
    {
        public bool playAtCenter = true;
        public Vector3 effectOffset;
        
        public AudioClipSet effectSound;
        public ParticleSystem effectParticles;

        public virtual Vector3 GetHitFXPosition( Bounds ownerBounds, Bounds hitZone )
        {
            if( playAtCenter )
            {
                return ownerBounds.center + effectOffset;
            }
            else
            {
                ownerBounds.Expand( 0.1f );
                
                //TODO: this version is for a 2D collision for overlapping rects.... needs to be updated for 3D
                hitZone.GetRect().GetIntersectionRect( ownerBounds.GetRect(), out Rect rect );
                return rect.center.ToVector3() + effectOffset;
            }

            //return transform.position + effectOffset;
        }

        public void Play( Bounds ownerBounds, Bounds hitZone )
        {
            if( effectSound?.Clips.Count > 1 )
                effectSound?.PlayRandom( canPlayPreviousClip: false );
            else
                effectSound?.Play();

            if( effectParticles != null )
            {
                ParticleSystem hitFX = GameObject.Instantiate( effectParticles );
                Vector3 hitPos = GetHitFXPosition( ownerBounds, hitZone );
                hitFX.transform.position = hitPos;
                hitFX.gameObject.SetActive( true );
                Destroy( hitFX.gameObject, hitFX.main.duration );
            }
        }
    }
}