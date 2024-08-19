using UnityEngine;

namespace BlocInBloc {
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        protected static T _instance;

        private static object _lock = new object ();

        public static T safeInstance {
            get {
                if (applicationIsQuitting) {
                    return null;
                }
                return _instance;
            }
        }

        public static T Instance {
            get {
                if (applicationIsQuitting) {
                    return null;
                }

                lock (_lock) {
                    InitInstance ();

                    return _instance;
                }
            }
        }

        protected static void InitInstance () {
            if (_instance == null) {
                Object[] allInstances = FindObjectsOfType (typeof (T), true);
                if (allInstances.Length > 1) {
                    Debug.LogError ("[Singleton] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopening the scene might fix it.");
                } else if (allInstances.Length == 1) {
                    _instance = (T)allInstances[0];
                }
            }
        }

        private static bool applicationIsQuitting = false;

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnApplicationQuit () {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy () {
            lock (_lock) {
                _instance = null;
            }
        }
    }
}