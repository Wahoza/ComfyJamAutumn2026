using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine; 

namespace Sequences
{
    [System.Serializable]
    public partial class TransformCurveSequenceItem : ISequenceItem
    {
        public SequenceEnumAlloc.SequenceType type;

        public float _fDuration = 1;
        public float _fSpeed = 1;
        public bool _bSpeedBased = false;
        public bool _bLocalOperation = false;

        public AnimationCurve _movementCurve = AnimationCurve.Linear(0,0, 1, 1);
        public bool _bRunInParallel = false;

        public Transform owner;
        public SequenceComponent ownerAnimComp;

        public Vector3 _vModVector;
        public Transform _ModTransform;
        
        [NonSerialized] public ISequenceItem nextSequence;
        private bool _bQuitAtStart;

        private Vector3 _vInitPosition;
        private Vector3 _vInitRotation;
        private Vector3 _vInitScale;

        private Coroutine updateRoutine;
        public virtual void Start()
        {
            updateRoutine = ownerAnimComp.StartCoroutine(DurationUpdateRoutine());
        }

        public virtual void Quit(bool complete)
        {
            if(updateRoutine != null)
                ownerAnimComp.StopCoroutine(updateRoutine); 
            
            if (complete)
                switch (type)
                {
                    case SequenceEnumAlloc.SequenceType.MovementToTarget:
                        OnQuitCompleteMoveToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.MovementFromVector:
                        OnQuitCompleteMoveToVector();
                        break;
                    case SequenceEnumAlloc.SequenceType.RotationToTarget:
                        OnQuitCompleteRotateToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.RotationFromEuler:
                        OnQuitCompleteRotateToVector();
                        break;
                    case SequenceEnumAlloc.SequenceType.ScaleToTarget:
                        OnQuitCompleteScaleToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.ScaleToVector:
                        OnQuitCompleteScaleToVector();  
                        break;
                }
                nextSequence?.Start();
        }



        private IEnumerator DurationUpdateRoutine()
        {
            float elapsed = 0;
            float trueDuration = _fDuration;

            _vInitPosition = owner.position;
            _vInitRotation = owner.eulerAngles;
            _vInitScale = owner.localScale;

            yield return null;

            if (_bRunInParallel)
                nextSequence?.Start();

            if (!_bSpeedBased)
            {
                while (elapsed < _fDuration)
                {
                    elapsed += Time.deltaTime;

                    switch (type)
                    {
                        case SequenceEnumAlloc.SequenceType.MovementToTarget:
                            MoveToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.MovementFromVector:
                            MoveToVector(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.RotationToTarget:
                            RotateToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.RotationFromEuler:
                            RotateToVector(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.ScaleToTarget:
                            ScaleToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.ScaleToVector:
                            ScaleToVector(Time.deltaTime, elapsed);
                            break;
                    }

                    yield return null;
                }

                switch (type)
                {
                    case SequenceEnumAlloc.SequenceType.MovementToTarget:
                        OnQuitCompleteMoveToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.MovementFromVector:
                        OnQuitCompleteMoveToVector();
                        break;
                    case SequenceEnumAlloc.SequenceType.RotationToTarget:
                        OnQuitCompleteRotateToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.RotationFromEuler:
                        OnQuitCompleteRotateToVector();
                        break;
                    case SequenceEnumAlloc.SequenceType.ScaleToTarget:
                        OnQuitCompleteScaleToTransform();
                        break;
                    case SequenceEnumAlloc.SequenceType.ScaleToVector:
                        OnQuitCompleteScaleToVector();
                        break;
                }

                if (!_bQuitAtStart)
                {
                    nextSequence?.Start();
                }
            }
            else
            {
                bool complete = false;

                while (!complete)
                {
                    switch (type)
                    {
                        case SequenceEnumAlloc.SequenceType.MovementToTarget:
                            complete = MoveToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.MovementFromVector:
                            complete = MoveToVector(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.RotationToTarget:
                            complete = RotateToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.RotationFromEuler:
                            complete = RotateToVector(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.ScaleToTarget:
                            complete = ScaleToTransform(Time.deltaTime, elapsed);
                            break;
                        case SequenceEnumAlloc.SequenceType.ScaleToVector:
                            complete = ScaleToVector(Time.deltaTime, elapsed);
                            break;
                    }

                    yield return null;

                    if (complete)
                    {
                        if (!_bQuitAtStart)
                        {
                            nextSequence?.Start();
                        }
                        yield break;
                    }
                }
            }
        }

        #region serialization
        public static void DrawGUI(SerializedProperty targetObject, SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {

            SerializedProperty durationProp = serializedObject.FindPropertyRelative("_fDuration");
            SerializedProperty speedProp = serializedObject.FindPropertyRelative("_fSpeed");
            SerializedProperty speedBasedProp = serializedObject.FindPropertyRelative("_bSpeedBased");
            SerializedProperty runInParalellProp = serializedObject.FindPropertyRelative("_bRunInParallel");
            SerializedProperty isLocalProp = serializedObject.FindPropertyRelative("_bLocalOperation");

            SerializedProperty movementCurveProp = serializedObject.FindPropertyRelative("_movementCurve");

            SerializedProperty ownerProp = serializedObject.FindPropertyRelative("owner");


            SerializedProperty modVectorProp = serializedObject.FindPropertyRelative("_vModVector");
            SerializedProperty modTransformProp = serializedObject.FindPropertyRelative("_ModTransform");

            SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");

            typeProp.enumValueIndex = Convert.ToInt32(type);

            ownerProp.objectReferenceValue = targetObject.objectReferenceValue;

            runInParalellProp.boolValue = EditorGUILayout.Toggle("Run in Parallel", runInParalellProp.boolValue);

            speedBasedProp.boolValue = EditorGUILayout.Toggle("Speed Based", speedBasedProp.boolValue);


            //Note: Unity only supports local level rescaling
            if ((SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex != SequenceEnumAlloc.SequenceType.ScaleToTarget 
                && (SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex != SequenceEnumAlloc.SequenceType.ScaleToVector 
                && !typeToEditorData[(SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex].Item1 )
            {
                isLocalProp.boolValue = EditorGUILayout.Toggle("Local", isLocalProp.boolValue);
            }


            if (speedBasedProp.boolValue)
            {
                speedProp.floatValue = EditorGUILayout.FloatField("Speed", speedProp.floatValue);
            }
            else
            {
                durationProp.floatValue = EditorGUILayout.FloatField("Duration", durationProp.floatValue);
            }

            if (typeToEditorData.ContainsKey((SequenceEnumAlloc.SequenceType) typeProp.enumValueIndex))
            {
                if (typeToEditorData[(SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex].Item1)
                {
                    modTransformProp.objectReferenceValue = EditorGUILayout.ObjectField($"{typeToEditorData[(SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex].Item2}", modTransformProp.objectReferenceValue, typeof(Transform), true);
                }
                else
                {
                    modVectorProp.vector3Value = EditorGUILayout.Vector3Field($"{typeToEditorData[(SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex].Item2}", modVectorProp.vector3Value);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Something went wrong");
            }

            if (!speedBasedProp.boolValue)
            {
                movementCurveProp.animationCurveValue = EditorGUILayout.CurveField("Progression", movementCurveProp.animationCurveValue);
            }
        }

        public static void DefaultInitialize(SerializedProperty targetObject, SequenceComponent ownerAnimComp, SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            SerializedProperty durationProp = serializedObject.FindPropertyRelative("_fDuration");
            SerializedProperty speedProp = serializedObject.FindPropertyRelative("_fSpeed");
            SerializedProperty speedBasedProp = serializedObject.FindPropertyRelative("_bSpeedBased");
            SerializedProperty runInParalellProp = serializedObject.FindPropertyRelative("_bRunInParallel");
            SerializedProperty isLocalProp = serializedObject.FindPropertyRelative("_bLocalOperation");

            SerializedProperty movementCurveProp = serializedObject.FindPropertyRelative("_movementCurve");

            SerializedProperty ownerAnimCompProp = serializedObject.FindPropertyRelative("ownerAnimCompProp");
            SerializedProperty ownerProp = serializedObject.FindPropertyRelative("owner");


            SerializedProperty modVectorProp = serializedObject.FindPropertyRelative("_vModVector");
            SerializedProperty modTransformProp = serializedObject.FindPropertyRelative("_ModTransform");

            SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");

            TransformCurveSequenceItem item = new();

            isLocalProp.boolValue = item._bLocalOperation;
            durationProp.floatValue = item._fDuration;
            speedProp.floatValue = item._fSpeed;
            speedBasedProp.boolValue = item._bSpeedBased;
            runInParalellProp.boolValue = item._bRunInParallel;
            movementCurveProp.animationCurveValue = item._movementCurve;
            ownerProp.objectReferenceValue = targetObject.objectReferenceValue;
            modVectorProp.vector3Value = item._vModVector;
        }

        public void SetOwnerComponent(SequenceComponent component)
        {
            ownerAnimComp = component;
        }

        public ISequenceItem GetNext()
        {
            return nextSequence;
        }

        //Type, use Transform?, DescripitveText
        private static Dictionary<SequenceEnumAlloc.SequenceType, (bool, string)> typeToEditorData = new()
        {
            {SequenceEnumAlloc.SequenceType.MovementFromVector, (false, "Movement Vector") },
            {SequenceEnumAlloc.SequenceType.MovementToTarget, (true, "Move To") },
            {SequenceEnumAlloc.SequenceType.RotationFromEuler, (false, "Euler Angles") },
            {SequenceEnumAlloc.SequenceType.RotationToTarget, (true, "Match Rotation To") },
            {SequenceEnumAlloc.SequenceType.ScaleToVector, (false, "Scale Vector") },
            {SequenceEnumAlloc.SequenceType.ScaleToTarget, (true, "Match Scale To") }
        };
        #endregion
    }
}

