using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine; 

namespace Sequences
{
    [System.Serializable]
    public class TransformCurveSequenceItem : ISequenceItem
    {
        public SequenceEnumAlloc.SequenceType type;

        public float duration = 1;
        public float speed = 1;
        public bool speedBased = false;
        public bool isLocalOperation = false;

        public AnimationCurve movementCurve = AnimationCurve.Linear(0,0, 1, 1);
        public bool runInParallel = false;

        public Transform owner;
        public SequenceComponent ownerAnimComp;

        public Vector3 modVector;
        public Transform modTransform;

        [NonSerialized] public ISequenceItem nextSequence;
        
        public virtual void Start()
        {
            ownerAnimComp.StartCoroutine(DurationUpdateRoutine());

            if (runInParallel)
                nextSequence?.Start();
        }

        public virtual void Quit(bool complete)
        {
            if (!runInParallel)
                nextSequence?.Start();
        }

        public virtual void Update(float elapsedTime)
        {
        }

        private IEnumerator DurationUpdateRoutine()
        {
            float elapsed = 0;

            while(elapsed < duration)
            {
                Update(elapsed);
                elapsed += Time.deltaTime;

                yield return null;
            }

            if (!runInParallel)
            {
                Quit(true);
            }
        }

        public static void DrawGUI(SerializedProperty targetObject, SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {

            SerializedProperty durationProp = serializedObject.FindPropertyRelative("duration");
            SerializedProperty speedProp = serializedObject.FindPropertyRelative("speed");
            SerializedProperty speedBasedProp = serializedObject.FindPropertyRelative("speedBased");
            SerializedProperty runInParalellProp = serializedObject.FindPropertyRelative("runInParallel");
            SerializedProperty isLocalProp = serializedObject.FindPropertyRelative("isLocalOperation");

            SerializedProperty movementCurveProp = serializedObject.FindPropertyRelative("movementCurve");

            SerializedProperty ownerAnimCompProp = serializedObject.FindPropertyRelative("ownerAnimCompProp");
            SerializedProperty ownerProp = serializedObject.FindPropertyRelative("owner");


            SerializedProperty modVectorProp = serializedObject.FindPropertyRelative("modVector");
            SerializedProperty modTransformProp = serializedObject.FindPropertyRelative("modTransform");

            SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");

            typeProp.enumValueIndex = Convert.ToInt32(type);

            ownerProp.objectReferenceValue = targetObject.objectReferenceValue;

            isLocalProp.boolValue = EditorGUILayout.Toggle("Local", isLocalProp.boolValue);

            runInParalellProp.boolValue = EditorGUILayout.Toggle("Run in Parallel", runInParalellProp.boolValue);

            speedBasedProp.boolValue = EditorGUILayout.Toggle("Speed Based", speedBasedProp.boolValue);

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

            movementCurveProp.animationCurveValue = EditorGUILayout.CurveField("Completion Curve", movementCurveProp.animationCurveValue);
        }

        public static void DefaultInitialize(SerializedProperty targetObject, SequenceComponent ownerAnimComp, SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            SerializedProperty durationProp = serializedObject.FindPropertyRelative("duration");
            SerializedProperty speedProp = serializedObject.FindPropertyRelative("speed");
            SerializedProperty speedBasedProp = serializedObject.FindPropertyRelative("speedBased");
            SerializedProperty runInParalellProp = serializedObject.FindPropertyRelative("runInParallel");
            SerializedProperty isLocalProp = serializedObject.FindPropertyRelative("isLocalOperation");

            SerializedProperty movementCurveProp = serializedObject.FindPropertyRelative("movementCurve");

            SerializedProperty ownerAnimCompProp = serializedObject.FindPropertyRelative("ownerAnimCompProp");
            SerializedProperty ownerProp = serializedObject.FindPropertyRelative("owner");


            SerializedProperty modVectorProp = serializedObject.FindPropertyRelative("modVector");
            SerializedProperty modTransformProp = serializedObject.FindPropertyRelative("modTransform");

            SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");

            TransformCurveSequenceItem item = new();

            isLocalProp.boolValue = item.isLocalOperation;
            durationProp.floatValue = item.duration;
            speedProp.floatValue = item.speed;
            speedBasedProp.boolValue = item.speedBased;
            runInParalellProp.boolValue = item.runInParallel;
            movementCurveProp.animationCurveValue = item.movementCurve;
            ownerProp.objectReferenceValue = targetObject.objectReferenceValue;
            modVectorProp.vector3Value = item.modVector;
        }

        public void SetOwnerComponent(SequenceComponent component)
        {
            ownerAnimComp = component;
        }
        //public void SetDataFromOtherCurveItem(AnimationCurveSequenceItem other)
        //{
        //    duration = other.duration;
        //    speed = other.speed;
        //    movementCurve = other.movementCurve;
        //    runInParallel = other.runInParallel;
        //    speedBased = other.speedBased;
        //    owner = other.owner;
        //    ownerAnimComp = other.ownerAnimComp;
        //}

        //Type, use Transform?, DescripitveText
        private static Dictionary<SequenceEnumAlloc.SequenceType, (bool, string)> typeToEditorData = new()
        {
            {SequenceEnumAlloc.SequenceType.MovementFromVector, (false, "Movement Vector") },
            {SequenceEnumAlloc.SequenceType.MovementToTarget, (true, "Move To") },
            {SequenceEnumAlloc.SequenceType.RotationFromEuler, (true, "Euler Angles") },
            {SequenceEnumAlloc.SequenceType.RotationToTarget, (true, "Match Rotation To") },
            {SequenceEnumAlloc.SequenceType.ScaleToVector, (true, "Scale Vector") },
            {SequenceEnumAlloc.SequenceType.ScaleToTarget, (true, "Match Scale To") }
        };
    }
}

