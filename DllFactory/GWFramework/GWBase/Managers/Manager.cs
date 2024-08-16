using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    public abstract IEnumerator Kickstart();
}
