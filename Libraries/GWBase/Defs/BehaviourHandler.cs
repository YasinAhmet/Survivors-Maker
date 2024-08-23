using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
namespace GWBase {

public class BehaviourHandler<T>
    {
        public T ownedThing;

        public object[] GetObjectsByRequests(ParameterRequest[] requests)
        {
            List<object> objects = new();

            foreach (var request in requests)
            {
                var foundObj = GetObjectByRequest(request.requestedType);
                objects.Add(foundObj);
            }

            return objects.ToArray();
        }

        public object GetObjectByRequest(string request)
        {

            switch (request)
            {
                case "GetPlayerController":
                    return PlayerController.playerController;

                case "GetOwnedObject":
                    return ownedThing;

                default:
                    return null;
            }

        }
    }
}