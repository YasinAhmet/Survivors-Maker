using System;
using System.Collections.Generic;
using UnityEngine;

namespace GWBase
{
    public class ItemPickupManager : MonoBehaviour
    {
        public delegate void ItemInPickup(GameObject item, ItemPickupManager pickupManager);
        public event ItemInPickup itemInPickup;
        
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
            if (transform.localScale.x <= 0 || transform.localScale.y <= 0)
            {
                Destroy(GetComponent<Rigidbody2D>());
                Destroy(GetComponent<Collider2D>());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!setupped) return;
            var othergm = other.gameObject;
            IGrabbable grabbable = othergm.GetComponent<IGrabbable>();
            if (grabbable == null) return;
            itemInPickup?.Invoke(othergm, this);
            
            if (autoGrab && rangeToGrab > Vector2.Distance(transform.position, othergm.transform.position))
            {
                grabbable.GrabRequest(owner);
            }
        }
    }
}