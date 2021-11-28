using UnityEngine;

namespace NomaiVR.Player
{
    public class VRCameraManipulator : MonoBehaviour
    {
		private IObservable lastObservable;
		private Collider lastHitCollider;

		private RaycastHit ProcessRaycast()
		{
			if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 75f, OWLayerMask.blockableInteractMask))
			{
				if (hitInfo.collider != lastHitCollider)
				{
					lastHitCollider = hitInfo.collider;
					if (lastObservable != null)
					{
						lastObservable.LoseFocus();
					}
					lastObservable = lastHitCollider.GetComponent<IObservable>();
				}
			}
			else
			{
				lastHitCollider = null;
				if (lastObservable != null)
				{
					lastObservable.LoseFocus();
					lastObservable = null;
				}
			}

			return hitInfo;
		}

		private void LateUpdate()
		{
			var hitInfo = ProcessRaycast();

			if (lastObservable != null)
				lastObservable.Observe(hitInfo, transform.position);
		}
	}
}
