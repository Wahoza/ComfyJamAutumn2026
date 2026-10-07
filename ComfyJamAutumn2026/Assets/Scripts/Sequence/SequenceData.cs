using System.Collections.Generic;
using System;
using UnityEngine;

namespace Sequences
{
    public static class SequenceEnumAlloc
    {

        public enum SequenceType
        {
            None,
            MovementToTarget,
            MovementFromVector,
            RotationToTarget,
            RotationFromEuler,
            ScaleToTarget,
            ScaleToVector,
            BroadcastEvent,
            WaitForSeconds,
            WaitForBooleanOnBlackboard
        }

        public static readonly Dictionary<SequenceType, System.Type> TypeToClassDictionary = new()
        {
            {SequenceType.MovementToTarget, typeof(TransformCurveSequenceItem) },
            {SequenceType.MovementFromVector, typeof(TransformCurveSequenceItem) },

            {SequenceType.RotationToTarget, typeof(TransformCurveSequenceItem) },
            {SequenceType.RotationFromEuler, typeof(TransformCurveSequenceItem) },

            {SequenceType.ScaleToTarget, typeof(TransformCurveSequenceItem) },
            {SequenceType.ScaleToVector, typeof(TransformCurveSequenceItem) },

            {SequenceType.BroadcastEvent, typeof(EventSequenceItem) },

            {SequenceType.WaitForSeconds, typeof(WaitSequenceItem) },
            {SequenceType.WaitForBooleanOnBlackboard, typeof(WaitSequenceItem) },

            {SequenceType.None, null },
        };
    }

    [Serializable]
    public struct DataForUnityEditor
    {
        public bool isFoldedOut;
        public bool overrideTarget;
        public Transform target;
        public SequenceEnumAlloc.SequenceType type;
    }

}