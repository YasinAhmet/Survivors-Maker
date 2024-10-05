using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
namespace GWBase {

public class GameObject_Area : MonoBehaviour {
    public List<Collider2D> collidersInside = new();
    public string faction;
    public Transform ownedTransform;

    public struct ClosestObject {
        public Collider2D closest;
        public float distance;
    }

    private void Start()
    {
        ownedTransform = transform;
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<GameObj>(out GameObj gameObj)) {
            collidersInside.Add(other);
        }
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
            float distance = Vector2.Distance(ownedTransform.position, collider.transform.position);
            if(distance < closestObject.distance) {
                closestObject.closest = collider;
                closestObject.distance = distance;
            }
        }

        return closestObject;
    }
}
}