using UnityEngine;

namespace NomaiVR.Helpers
{
    public static class MathHelper
    {
        // Stolen from here: https://gist.github.com/maxattack/4c7b4de00f5c1b95a33b
        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            // account for double-cover
            var dot = Quaternion.Dot(rot, target);
            var multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;
            // smooth damp (nlerp approx)
            var result = new Vector4(
                SmoothDamp(rot.x, target.x, ref deriv.x, time),
                SmoothDamp(rot.y, target.y, ref deriv.y, time),
                SmoothDamp(rot.z, target.z, ref deriv.z, time),
                SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;
            // compute deriv
            var dtInv = 1f / Time.unscaledDeltaTime;
            deriv.x = (result.x - rot.x) * dtInv;
            deriv.y = (result.y - rot.y) * dtInv;
            deriv.z = (result.z - rot.z) * dtInv;
            deriv.w = (result.w - rot.w) * dtInv;
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        public static float SmoothDamp(
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime)
        {
            return Mathf.SmoothDamp(
                current,
                target,
                ref currentVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }
        
        public static Vector3 SmoothDamp(
            Vector3 current,
            Vector3 target,
            ref Vector3 currentVelocity,
            float smoothTime)
        {
            return Vector3.SmoothDamp(
                current,
                target,
                ref currentVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }
    }
}