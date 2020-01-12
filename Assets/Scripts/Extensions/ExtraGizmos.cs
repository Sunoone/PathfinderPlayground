using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public class ExtraGizmos : MonoBehaviour
    {

        public static void DrawWireBox(Vector3 origin, Vector3 size, Color color)
        {
            Color defaultColor = Gizmos.color;
            Gizmos.color = color;
            Vector3[] bottomCorners = HorizontalCorners(origin, size, true);
            Vector3[] topCorners = HorizontalCorners(origin, size, false);
            Vector3[] frontCorners = VerticalCorners(origin, size, true);
            Vector3[] backCorners = VerticalCorners(origin, size, false);

            DrawWireShape(bottomCorners);
            DrawWireShape(topCorners);
            DrawWireShape(frontCorners);
            DrawWireShape(backCorners);
            Gizmos.color = defaultColor;
        }

        private static Vector3[] HorizontalCorners(Vector3 origin, Vector3 size, bool bottom)
        {
            int sign = (bottom) ? -1 : 1;
            Vector3 halfSize = size / 2 * sign;
            int cornerCount = 4;
            Vector3[] corners = new Vector3[cornerCount];
            corners[0] = origin + halfSize;
            corners[1] = corners[0];
            corners[1].x -= size.x * sign;
            corners[2] = corners[1];
            corners[2].z -= size.z * sign;
            corners[3] = corners[0];
            corners[3].z -= size.z * sign;
            return corners;
        }

        private static Vector3[] VerticalCorners(Vector3 origin, Vector3 size, bool front)
        {
            int sign = (front) ? -1 : 1;
            Vector3 halfSize = size / 2 * sign;
            int cornerCount = 4;
            Vector3[] corners = new Vector3[cornerCount];
            corners[0] = origin + halfSize;
            corners[1] = corners[0];
            corners[1].y -= size.y * sign;
            corners[2] = corners[1];
            corners[2].x -= size.x * sign;
            corners[3] = corners[0];
            corners[3].x -= size.x * sign;
            return corners;
        }


        public static void DrawWireShape(Vector3[] positions)
        {
            int length = positions.Length;
            for (int i = 0; i < length; i++)
            {
                int j = (i + 1 < length) ? i + 1 : 0;
                Gizmos.DrawLine(positions[i], positions[j]);
            }
        }
    }
}
