using System;
using UnityEngine;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugSpinner : MonoBehaviour
    {
        [SerializeField]
        private float speed = 360;
        
        private void Update()
        {
            transform.Rotate(Vector3.back,Time.deltaTime*speed);
        }
    }
}