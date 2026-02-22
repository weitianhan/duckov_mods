using UnityEngine;

namespace MyMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private void Start()
        {
            Debug.Log("[MyMod] ModBehaviour.Start() loaded!");
        }

        private void Update()
        {
            // Keep Update lightweight; avoid spam logging.
        }
    }
}
