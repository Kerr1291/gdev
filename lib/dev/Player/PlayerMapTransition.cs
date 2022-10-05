using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nv.Game
{
    public class PlayerMapTransition : MonoBehaviour
    {
        public Rigidbody player = null;

        //TODO: should this be done in update?
        //also, this script needs to get disabled by something to avoi any more checks until the new map has been loaded
        //probably use the coroutine stuff
        private void FixedUpdate()
        {
            if(!IsGrounded())
            {
                //PlayerManager.Instance.MovePlayerToSpawnPoint();
                //GameMap.Instance.UpdateMapContent();
            }
        }

        //TODO: Move to a more general player class (probably not player controller)
        //and ifx it cause it is a bit hacky
        bool IsGrounded()
        {
            
            return Physics.Raycast(transform.position+ transform.up, -transform.up, 10f);
        }
   
 }
};
