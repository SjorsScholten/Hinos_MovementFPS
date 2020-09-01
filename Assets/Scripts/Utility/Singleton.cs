using UnityEngine;

namespace Utility {
    /// <summary>
    ///     Inherit from this base class to create a singleton.
    ///     e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown;
        private static readonly object m_Lock = new object();
        private static T m_Instance;

        public static T Instance {
            // Access singleton instance through this propriety.
            get {
                if (m_ShuttingDown) {
                    Debug.LogWarning($"[Singleton] Instance \'{typeof(T)}\' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock) {
                    if (m_Instance != null) return m_Instance;
                    m_Instance = (T) FindObjectOfType(typeof(T)); // Search for existing instance.
                    if (m_Instance != null) return m_Instance; // Create new instance if one doesn't already exist.
                    var singletonObject = new GameObject(); // Need to create a new GameObject to attach the singleton to.
                    m_Instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";
                    DontDestroyOnLoad(singletonObject); // Make instance persistent.
                    return m_Instance;
                }
            }
        }

        private void OnApplicationQuit() {
            m_ShuttingDown = true;
        }

        private void OnDestroy() {
            m_ShuttingDown = true;
        }
    }
}