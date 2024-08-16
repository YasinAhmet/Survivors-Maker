using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

public class GameObject_Area : MonoBehaviour {
    [SerializeField] private Rigidbody2D ownedRigidbody2D;
    [SerializeField] private CircleCollider2D ownedCollider2D;
    public List<Collider2D> collidersInside = new();


    public struct ClosestObject {
        public Collider2D closest;
        public float distance;
    }

    
    private void OnTriggerEnter2D(Collider2D other) {
        collidersInside.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other) {
        collidersInside.Remove(other);  
    }

    public ClosestObject GetClosestObject() {
        ClosestObject closestObject = new ClosestObject() {
            closest = null,
            distance = 10000
        };

        foreach (var collider in collidersInside)
        {
            float distance = Vector2.Distance(transform.position, collider.gameObject.transform.position);
            if(distance < closestObject.distance) {
                Debug.Log($"[TARGETTING] Found a better target with {distance}. It's {collider.gameObject.name}");
                closestObject.closest = collider;
                closestObject.distance = distance;
            }
        }

        return closestObject;
    }
}