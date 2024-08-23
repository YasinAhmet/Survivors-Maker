using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;

namespace GWBase
{
    [Serializable]
    public class WorldUIObjectPool : LightObjectPool
    {
        public override GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction)
        {
            var gameobj = base.ObtainSlotForType(creature, location, rotation, faction);
            UIManager.uiManager.AttachObjectToWorldCanvas(gameobj);
            return gameobj;
        }
    }
}