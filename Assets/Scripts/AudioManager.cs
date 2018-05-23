using UnityEngine;

namespace Assets.Scripts
{
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance;    

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }
    }
}
