using System;
using System.Collections.Generic;
using UnityEngine;

namespace GWBase
{
    public class ItemPickupManager : MonoBehaviour
    {
        public GameObj owner;
        public bool autoGrab = false;
        public float rangeToGrab;
        public bool setupped = false;

        private void Awake()
        {
            if(!setupped)owner.OnPosesssed += Setup;
        }

        public void Setup()
        {
            setupped = true;
            rangeToGrab = owner.GetPossessed().GetStatValueByName("GrabRange");
            transform.localScale = new Vector3(rangeToGrab, rangeToGrab, 1);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!setupped) return;
            
            IGrabbable grabbable = other.gameObject.GetComponent<IGrabbable>();

            if (grabbable == null) return;
            if (autoGrab && rangeToGrab > Vector2.Distance(transform.position, other.gameObject.transform.position))
            {
                grabbable.GrabRequest(owner);
            }
        }
    }
}