using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GWBase
{
    public class ManagerBooter : MonoBehaviour
    {
        public UnityEvent managersBooted = new UnityEvent();
        public UnityEvent bootablesBooted = new UnityEvent();
        public Manager[] linkedManagers = { };
        public GameObject[] linkedBootable = { };
        public bool bootOnStartup = true;
        public bool dontDestroy = false;
        void Start()
        {
            if(dontDestroy) {
                DontDestroyOnLoad(gameObject);
            }
            if(bootOnStartup) {
                StartCoroutine(LoadManagers());
            }
        }
        public IEnumerator LoadManagers()
        {
            foreach (var manager in linkedManagers)
            {
                yield return StartCoroutine(manager.Kickstart());
            }

            managersBooted?.Invoke();
            StartCoroutine(LoadBootables());
        }

        public IEnumerator LoadBootables()
        {
            foreach (var bootableObj in linkedBootable)
            {
                var bootable = bootableObj.GetComponent<IBootable>();
                yield return StartCoroutine(bootable.Boot());
            }

            bootablesBooted?.Invoke();
        }
    }
}