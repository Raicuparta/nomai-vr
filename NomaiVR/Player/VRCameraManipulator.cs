using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public class VRCameraManipulator : MonoBehaviour
    {
        private IObservable _lastObservable;
		private Collider _lastHitCollider;

		private RaycastHit ProcessRaycast()
        {
			if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, 75f, OWLayerMask.blockableInteractMask))
			{
				if (hitInfo.collider != _lastHitCollider)
				{
					_lastHitCollider = hitInfo.collider;
					if (_lastObservable != null)
					{
						_lastObservable.LoseFocus();
					}
					_lastObservable = _lastHitCollider.GetComponent<IObservable>();
				}
			}
			else
			{
				_lastHitCollider = null;
				if (_lastObservable != null)
				{
					_lastObservable.LoseFocus();
					_lastObservable = null;
				}
			}

			return hitInfo;
		}

		private void LateUpdate()
		{
			var hitInfo = ProcessRaycast();

			if (_lastObservable != null)
				_lastObservable.Observe(hitInfo, base.transform.position);
		}
	}
}
