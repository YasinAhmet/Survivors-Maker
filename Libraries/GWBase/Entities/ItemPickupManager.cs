using System;
using System.Collections.Generic;
using UnityEngine;

namespace GWBase
{
    public class ItemPickupManager : MonoBehaviour
    {
        public GameObj owner;
        public bool autoGrab = true;
        public float rangeToGrab;
        private void OnTriggerEnter2D(Collider2D other)
        {
            IGrabbable grabbable = other.gameObject.GetComponent<IGrabbable>();

            if (grabbable == null) return;
            if (autoGrab && rangeToGrab > Vector2.Distance(transform.position, other.gameObject.transform.position))
            {
                grabbable.GrabRequest(owner);
            }
        }
    }
}