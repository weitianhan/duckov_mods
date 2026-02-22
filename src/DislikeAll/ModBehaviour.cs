using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MyMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const KeyCode DislikeAllHotkey = KeyCode.K;

        private static readonly FieldInfo? ManualWishlistField = typeof(ItemWishlist).GetField(
            "manualWishList",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo? NotifyRefreshWishlistInfoMethod = typeof(Duckov.UI.ItemHoveringUI).GetMethod(
            "NotifyRefreshWishlistInfo",
            BindingFlags.Static | BindingFlags.NonPublic);

        private void Start()
        {
            Debug.Log("[MyMod] Loaded. Press K to clear manually wishlisted items.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(DislikeAllHotkey))
            {
                ClearManualWishlist();
            }
        }

        private static void ClearManualWishlist()
        {
            if (ItemWishlist.Instance == null)
            {
                Debug.Log("[MyMod] ItemWishlist is not ready yet.");
                return;
            }

            if (ManualWishlistField == null)
            {
                Debug.LogWarning("[MyMod] manualWishList field was not found. Game version may have changed.");
                return;
            }

            if (ManualWishlistField.GetValue(ItemWishlist.Instance) is not List<int> manualWishList)
            {
                Debug.LogWarning("[MyMod] manualWishList field is unavailable or has unexpected type.");
                return;
            }

            if (manualWishList.Count == 0)
            {
                Debug.Log("[MyMod] No manually wishlisted items to clear.");
                return;
            }

            List<int> snapshot = new List<int>(manualWishList);
            int removedCount = 0;
            for (int i = 0; i < snapshot.Count; i++)
            {
                if (ItemWishlist.RemoveFromWishlist(snapshot[i]))
                {
                    removedCount++;
                }
            }

            NotifyRefreshWishlistInfoMethod?.Invoke(null, null);
            Debug.Log($"[MyMod] DislikeAll done. Removed {removedCount} manual wishlist item(s).");
        }
    }
}
