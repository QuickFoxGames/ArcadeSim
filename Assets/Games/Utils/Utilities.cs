using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MGUtilities
{
    [System.Serializable]
    public class FloatTransformKeyValuePair
    {
        public float Key;
        public Transform Value;

        public FloatTransformKeyValuePair(float key, Transform value)
        {
            Key = key;
            Value = value;
        }
    }
    public class Coroutines
    {
        #region Bool
        public static IEnumerator DelayBoolChange(bool startState, bool endState, float waitTime, Action<bool> onUpdate)
        {
            onUpdate?.Invoke(startState);
            yield return new WaitForSeconds(waitTime);
            onUpdate?.Invoke(endState);
        }
        #endregion
        #region Vector3
        public static IEnumerator LerpVector3OverTime(Vector3 start, Vector3 end, float duration, Action<Vector3> onUpdate)
        {
            float timePassed = 0f;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector3 currentValue = Vector3.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        public static IEnumerator LerpVector3ToZeroOverTime(bool state, float duration, Action<Vector3> onUpdate)
        {
            float timePassed = 0f;
            Vector3 start = state ? Vector3.zero : Vector3.one;
            Vector3 end = state ? Vector3.one : Vector3.zero;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector3 currentValue = Vector3.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        public static IEnumerator LerpVector3ToZeroOverTime(bool state, float duration, Vector3 currentScale, Action<Vector3> onUpdate)
        {
            float timePassed = 0f;
            Vector3 start = currentScale; // Start from the current scale
            Vector3 end = state ? Vector3.one : Vector3.zero; // Target scale

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector3 currentValue = Vector3.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end); // Ensure it reaches the exact target
        }
        public static IEnumerator PingPongVector3OverTime(Vector3 start, Vector3 end, float duration, Action<Vector3> onUpdate)
        {
            float timePassed = 0f;
            float d = 0.5f * duration;
            while (timePassed < d)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector3 currentValue = Vector3.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            timePassed = 0f;
            while (timePassed < d)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector3 currentValue = Vector3.Lerp(end, start, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(start);
        }
        public static IEnumerator PingPongVector3OverTime(Vector3 start, Vector3 end, float duration1, float duration2, Action<Vector3> onUpdate)
        {
            float timePassed = 0f;
            while (timePassed < duration1)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration1);
                Vector3 currentValue = Vector3.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            timePassed = 0f;
            while (timePassed < duration2)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration2);
                Vector3 currentValue = Vector3.Lerp(end, start, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(start);
        }
        public static IEnumerator LerpVector3OverPath(Vector3[] positions, Quaternion[] rotations, float duration, bool reverse, Action<(Vector3, Quaternion)> onUpdate)
        {
            if (reverse)
            {
                Array.Reverse(positions);
                Array.Reverse(rotations);
            }
            int segmentCount = positions.Length - 1;
            float segmentDuration = duration / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float timePassed = 0f;
                while (timePassed < segmentDuration)
                {
                    timePassed += Time.deltaTime;
                    float lerpFactor = Mathf.Clamp01(timePassed / segmentDuration);

                    Vector3 currentPos = Vector3.Lerp(positions[i], positions[i + 1], lerpFactor);
                    Quaternion currentRot = Quaternion.Slerp(rotations[i], rotations[i + 1], lerpFactor);

                    onUpdate?.Invoke((currentPos, currentRot));

                    yield return null;
                }
            }
            onUpdate?.Invoke((positions[^1], rotations[^1]));
        }
        public static IEnumerator LerpVector3OverPathPingPong(Vector3[] positions, Quaternion[] rotations, float duration, bool reverse, Action<(Vector3, Quaternion)> onUpdate)
        {
            if (reverse)
            {
                Array.Reverse(positions);
                Array.Reverse(rotations);
            }

            int segmentCount = positions.Length - 1;

            float halfDuration = duration / 2f;
            float segmentDurationForward = halfDuration / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float timePassed = 0f;
                while (timePassed < segmentDurationForward)
                {
                    timePassed += Time.deltaTime;
                    float lerpFactor = Mathf.Clamp01(timePassed / segmentDurationForward);

                    Vector3 currentPos = Vector3.Lerp(positions[i], positions[i + 1], lerpFactor);
                    Quaternion currentRot = Quaternion.Slerp(rotations[i], rotations[i + 1], lerpFactor);
                    onUpdate?.Invoke((currentPos, currentRot));

                    yield return null;
                }
            }
            onUpdate?.Invoke((positions[^1], rotations[^1]));

            float segmentDurationBackward = halfDuration / segmentCount;
            for (int i = segmentCount; i > 0; i--)
            {
                float timePassed = 0f;
                while (timePassed < segmentDurationBackward)
                {
                    timePassed += Time.deltaTime;
                    float lerpFactor = Mathf.Clamp01(timePassed / segmentDurationBackward);

                    Vector3 currentPos = Vector3.Lerp(positions[i], positions[i - 1], lerpFactor);
                    Quaternion currentRot = Quaternion.Slerp(rotations[i], rotations[i - 1], lerpFactor);
                    onUpdate?.Invoke((currentPos, currentRot));

                    yield return null;
                }
            }
            onUpdate?.Invoke((positions[0], rotations[0]));
        }
        #endregion
        #region Vector2
        public static IEnumerator LerpVector2OverTime(Vector2 start, Vector2 end, float duration, Action<Vector2> onUpdate)
        {
            float timePassed = 0f;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector2 currentValue = Vector2.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        public static IEnumerator LerpVector2OverTime(bool state, float duration, Action<Vector2> onUpdate)
        {
            float timePassed = 0f;
            Vector2 start = state ? Vector2.zero : Vector2.one;
            Vector2 end = state ? Vector2.one : Vector2.zero;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector2 currentValue = Vector2.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        public static IEnumerator PingPongVector2OverTime(Vector2 start, Vector2 end, float duration, Action<Vector2> onUpdate)
        {
            float timePassed = 0f;
            float d = 0.5f * duration;
            while (timePassed < d)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector2 currentValue = Vector2.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            timePassed = 0f;
            while (timePassed < d)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Vector2 currentValue = Vector2.Lerp(end, start, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(start);
        }
        public static IEnumerator PingPongVector2OverTime(Vector2 start, Vector2 end, float duration1, float duration2, Action<Vector2> onUpdate)
        {
            float timePassed = 0f;
            while (timePassed < duration1)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration1);
                Vector2 currentValue = Vector2.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            timePassed = 0f;
            while (timePassed < duration2)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration2);
                Vector2 currentValue = Vector2.Lerp(end, start, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(start);
        }
        #endregion
        #region Float
        public static IEnumerator LerpFloatOverTime(float start, float end, float duration, Action<float> onUpdate)
        {
            float timePassed = 0f;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                float currentValue = Mathf.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        #endregion
        #region Color
        public static IEnumerator LerpColorOverTime(Color start, Color end, float duration, Action<Color> onUpdate)
        {
            float timePassed = 0f;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = Mathf.Clamp01(timePassed / duration);
                Color currentValue = Color.Lerp(start, end, lerpFactor);

                onUpdate?.Invoke(currentValue);

                yield return null;
            }
            onUpdate?.Invoke(end);
        }
        #endregion
        #region Scenes
        public static IEnumerator LoadSceneOverTime(int sceneIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneIndex);
        }
        #endregion
    }
    public static class MGFunc
    {
        public static float MapRangeTo01(float value, float x, float y)
        {
            return (value - x) / (y - x);
        }
        public static float CalculateFrictionMultiplier(float theta, float maxSlope, float maxFrictionMulti)
        {
            theta = Mathf.Clamp(theta, 0f, maxSlope);
            return 1f + (theta / maxSlope) * (maxFrictionMulti - 1f);
        }
    }
}