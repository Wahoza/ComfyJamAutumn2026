using UnityEngine;

namespace Sequences
{
    public partial class TransformCurveSequenceItem
    {
        bool ScaleToTransform(float deltaTime, float elapsed)
        {
            if (_bSpeedBased && _ModTransform)
            {
                Vector3 directionToTarget = _ModTransform.localScale - owner.transform.localScale;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.localScale += normToTarget * _fSpeed * deltaTime;

                Vector3 newDirectionToTarget = _ModTransform.localScale - owner.transform.localScale;

                if (Vector3.SqrMagnitude(directionToTarget) < Vector3.SqrMagnitude(newDirectionToTarget) || Vector3.SqrMagnitude(directionToTarget) < 0.01f)
                {
                    owner.transform.localScale = _ModTransform.localScale;
                    return true;
                }

                return false;
            }
            else if (_ModTransform)
            {
                var pointInCurve = (elapsed / _fDuration);
                float valueInCurve = _movementCurve.Evaluate(pointInCurve);

                Vector3 path = _ModTransform.localScale - _vInitScale;

                Vector3 completed = valueInCurve * path;

                owner.transform.localScale = _vInitScale + completed;

                return false;
            }

            return true;
        }

        bool ScaleToVector(float deltaTime, float elapsed)
        {
            if (_bSpeedBased)
            {
                Vector3 directionToTarget = _vModVector + _vInitScale - owner.transform.localScale;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.localScale += normToTarget * _fSpeed * deltaTime;

                Vector3 newDirectionToTarget = _vModVector + _vInitScale - owner.transform.localScale;

                if (Vector3.SqrMagnitude(directionToTarget) < Vector3.SqrMagnitude(newDirectionToTarget) || Vector3.SqrMagnitude(directionToTarget) < 0.001f)
                {
                    owner.transform.localScale = _vModVector + _vInitScale;
                    return true;
                }

                return false;
            }
            else
            {
                var pointInCurve = (elapsed / _fDuration);
                float valueInCurve = _movementCurve.Evaluate(pointInCurve);

                Vector3 path = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);

                Vector3 completed = valueInCurve * path;

                owner.transform.localScale = _vInitPosition + completed;

                return true;
            }
        }

        void OnQuitCompleteScaleToTransform()
        {
            if (_ModTransform)
                owner.transform.localScale = _ModTransform.localScale;
        }

        void OnQuitCompleteScaleToVector()
        {
            owner.transform.localScale = _vInitScale + _vModVector;

        }
    }
}
