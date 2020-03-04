using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace NomaiVR {
    class SolarSystemMap: MonoBehaviour {
        void Start () {
            var mapCameraTransform = Locator.GetRootTransform().Find("MapCamera");

            var cameraCopy = Instantiate(mapCameraTransform.gameObject);
            foreach (Transform child in cameraCopy.transform) {
                DestroyImmediate(child);
            }
            Destroy(cameraCopy.GetComponent<MapController>());
            cameraCopy.transform.parent = mapCameraTransform;

            Destroy(mapCameraTransform.GetComponent<Camera>());
            Destroy(mapCameraTransform.GetComponent<OWCamera>());
            Destroy(mapCameraTransform.GetComponent<FlashbackScreenGrabImageEffect>());

            var mapController = mapCameraTransform.GetComponent<MapController>();
            mapController.SetValue("_mapCamera", cameraCopy.GetComponent<OWCamera>());
        }

        void CopyClassValues<T> (T source, GameObject targetObject) where T : Behaviour {
            var target = targetObject.AddComponent<T>();

            FieldInfo[] sourceFields = source
                .GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (var i = 0; i < sourceFields.Length; i++) {
                var value = sourceFields[i].GetValue(source);
                sourceFields[i].SetValue(target, value);
            }
        }
    }
}
