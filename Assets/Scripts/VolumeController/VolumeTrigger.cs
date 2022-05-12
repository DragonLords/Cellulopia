using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumeStuff
{

    public class VolumeTrigger : MonoBehaviour
    {
        VolumeProfile profile;
        [SerializeField] Volume volume;
        public UnityEngine.Rendering.Universal.Vignette vignette;
        bool active = false;
        float oldVal;
        float newVal = 500f;
        // Start is called before the first frame update
        void Start()
        {
            profile = volume.profile;
            if (!profile)
                throw new System.NullReferenceException(nameof(VolumeProfile));

            if (!profile.TryGet(out vignette))
                throw new System.NullReferenceException(nameof(vignette));
            oldVal = vignette.intensity.value;

        }

        // Update is called once per frame
        void Update()
        {
            var k = Keyboard.current;
            if (k.anyKey.wasPressedThisFrame)
            {
                if (!volume.gameObject.activeSelf)
                {
                    volume.gameObject.SetActive(!volume.gameObject.activeSelf);
                }
                if (volume.gameObject.activeSelf)
                {
                    if (!active)
                    {
                        vignette.active = active;
                        active = !active;
                    }
                    else
                    {
                        vignette.active = active;
                        active = !active;
                    }
                    // foreach(var comp in volume.profile.components){
                    //     Debug.Log(comp.name+" "+comp.GetType());

                    // }
                }
            }
        }


    }

    namespace VolumeStuff.Events
    {
        public class VolumeTriggerEvents
        {

        }
    }
}