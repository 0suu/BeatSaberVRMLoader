using System;
using UnityEngine;

namespace VRMLoader
{
	public class Utility
    {
        public static Vector3 StringToVector3(string s)
        {
            s = s.Trim(new char[] { '(', ')' }).Replace(" ", "");
            string[] sArray = s.Split(',');
            return new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));
        }

        public static Quaternion StringToQuaternion(string eulerString)
        {
            eulerString = eulerString.Trim(new char[] { '(', ')' }).Replace(" ", "");
            string[] values = eulerString.Split(',');
            if (values.Length != 3)
            {
                Debug.LogError("Invalid Euler angles string format. Expected format: 'x,y,z'");
                return Quaternion.identity;
            }

            float x, y, z;
            if (!float.TryParse(values[0], out x) ||
                !float.TryParse(values[1], out y) ||
                !float.TryParse(values[2], out z))
            {
                Debug.LogError("Error parsing Euler angles string components to float.");
                return Quaternion.identity;
            }

            return Quaternion.Euler(x, y, z);
        }
    }
}
