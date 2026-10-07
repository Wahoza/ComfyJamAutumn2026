using UnityEngine;

namespace Sequences
{
    public partial class TransformCurveSequenceItem
    {
        bool RotateToTransform(float deltaTime, float elapsed)
        {
            if (_bSpeedBased && _ModTransform)
            {
                Vector3 directionToTarget = _ModTransform.eulerAngles - owner.transform.eulerAngles;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.eulerAngles += normToTarget * _fSpeed * deltaTime;

                if (Vector3.SqrMagnitude(directionToTarget) < 1f)
                {
                    owner.transform.eulerAngles = _ModTransform.eulerAngles;
                    return true;
                }

                return false;
            }
            else if (_ModTransform)
            {
                var pointInCurve = (elapsed / _fDuration);
                float valueInCurve = _effectCurve.Evaluate(pointInCurve);

                Vector3 path = _ModTransform.eulerAngles - _vInitRotation;

                Vector3 completed = valueInCurve * path;

                owner.transform.eulerAngles = _vInitRotation + completed;

                return false;
            }

            return true;
        }

        bool RotateToVector(float deltaTime, float elapsed)
        {
            if (_bSpeedBased)
            {
                Vector3 directionToTarget = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector) + _vInitRotation - owner.transform.eulerAngles;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.eulerAngles += normToTarget * _fSpeed * deltaTime;

                Vector3 newDirectionToTarget = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector) + _vInitRotation - owner.transform.eulerAngles;

                if (Vector3.SqrMagnitude(directionToTarget) < 1f)
                {
                    owner.eulerAngles = _vInitRotation + (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);
                    return true;
                }

                return false;
            }
            else
            {
                var pointInCurve = (elapsed / _fDuration);
                float valueInCurve = _effectCurve.Evaluate(pointInCurve);

                Vector3 path = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);

                Vector3 completed = valueInCurve * path;

                owner.transform.eulerAngles = _vInitPosition + completed;

                return true;
            }
        }

        void OnQuitCompleteRotateToTransform()
        {
            if (_ModTransform)
                owner.transform.eulerAngles = _ModTransform.eulerAngles;
        }

        void OnQuitCompleteRotateToVector()
        {
            owner.transform.eulerAngles = _vInitRotation + _effectCurve.Evaluate(1) * (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);
        }
    }
}
