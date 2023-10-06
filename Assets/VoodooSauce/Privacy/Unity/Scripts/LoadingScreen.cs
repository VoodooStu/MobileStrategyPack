
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Image _spinner;
        [SerializeField] private float _speed = 360;
        
        private void Update()
        {
            _spinner.transform.Rotate(Vector3.back,Time.deltaTime*_speed);
        }
    }
}
