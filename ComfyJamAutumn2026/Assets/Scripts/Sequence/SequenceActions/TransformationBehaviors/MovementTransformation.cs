using UnityEngine;

namespace Sequences
{
    public partial class TransformCurveSequenceItem
    {
        bool MoveToTransform(float deltaTime, float elapsed)
        {
            if (_bSpeedBased && _ModTransform) 
            {
                Vector3 directionToTarget = _ModTransform.position - owner.transform.position;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.position += normToTarget * _fSpeed * deltaTime;

                Vector3 newDirectionToTarget = _ModTransform.position - owner.transform.position;

                if (Vector3.SqrMagnitude(directionToTarget) < Vector3.SqrMagnitude(newDirectionToTarget) || Vector3.SqrMagnitude(directionToTarget) < 0.001f)
                {
                    owner.transform.position = _ModTransform.position;
                    return true;
                }

                return false;
            }
            else if(_ModTransform)
            {
                var pointInCurve = (elapsed / _fDuration);
                float valueInCurve = _movementCurve.Evaluate(pointInCurve);

                Vector3 path = _ModTransform.position - _vInitPosition;

                Vector3 completed = valueInCurve * path;

                owner.transform.position = _vInitPosition + completed;

                return false;
            }

            return true;
        }

        bool MoveToVector(float deltaTime, float elapsed) 
        {
            if (_bSpeedBased)
            {
                Vector3 directionToTarget = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector) + _vInitPosition - owner.transform.position;

                Vector3 normToTarget = Vector3.Normalize(directionToTarget);

                owner.transform.position += normToTarget * _fSpeed * deltaTime;

                Vector3 newDirectionToTarget = (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector) + _vInitPosition - owner.transform.position;

                if (Vector3.SqrMagnitude(directionToTarget) < Vector3.SqrMagnitude(newDirectionToTarget) || Vector3.SqrMagnitude(directionToTarget) < 0.001f)
                {
                    owner.transform.position = _vInitPosition + (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);
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

                owner.transform.position = _vInitPosition + completed;

                return true;
            }
        }

        void OnQuitCompleteMoveToTransform() 
        {
            if(_ModTransform)
                owner.transform.position = _ModTransform.position;
        }

        void OnQuitCompleteMoveToVector()
        {
            owner.transform.position = _vInitPosition + (_bLocalOperation ? owner.InverseTransformDirection(_vModVector) : _vModVector);
        }
    }
}
