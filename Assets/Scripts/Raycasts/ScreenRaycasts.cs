using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Raycasts
{
    public static class RaycastUtilities
    {
        public static GameObject RaycastUI(this GraphicRaycaster GRc, ref Vector3 position, LayerMask ignoreMask)
        {
            ignoreMask = ~ignoreMask;
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current); //Set up the new Pointer Event       
            pointerEventData.position = position; //Set the Pointer Event Position to that of the mouse position         
            List<RaycastResult> results = new List<RaycastResult>(); //Create a list of Raycast Results     
            List<GameObject> HitObjects = new List<GameObject>();

            GameObject go = null;
            GRc.Raycast(pointerEventData, results);  //Raycast using the Graphics Raycaster and mouse click position  
            int length = results.Count;
            for (int i = 0; i < length; i++)
            {
                go = results[i].gameObject;
                if (go != null && (ignoreMask == (ignoreMask | (1 << go.layer))))
                {
                    Image image = go.GetComponent<Image>();
                    if (image != null && image.raycastTarget) // Body blocked scenario.
                        return go;
                }
            }
            return null;
        }

        public static GameObject RaycastScene(this Camera camera, ref Vector3 pos, LayerMask ignoreMask)
        {
            Ray ray = camera.ScreenPointToRay(pos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, ~ignoreMask))
            {
                pos = hit.point;
                return hit.collider.gameObject;
            }
            return null;
        }
    }
}
