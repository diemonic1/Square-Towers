using UnityEngine;

namespace Services.Monobehs
{
    public class StandaloneInputHandler : MonoBehaviour
    {
#if PLATFORM_STANDALONE
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
#endif
    }
}