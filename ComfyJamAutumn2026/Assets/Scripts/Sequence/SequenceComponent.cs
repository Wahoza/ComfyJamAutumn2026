using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes.Test;
using System;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace Sequences
{
    [RequireComponent(typeof(SequenceBlackboardComponent))]
    public class SequenceComponent : MonoBehaviour
    {
        [SerializeField] public List<DataForUnityEditor> serializedDataForEditor = new List<DataForUnityEditor>() { };

        [SerializeField] public List<TransformCurveSequenceItem> serializedTransformSequenceItems = new List<TransformCurveSequenceItem>() { };
        [SerializeField] public List<int> transformSequenceItemIndexes = new List<int>() { };

        [SerializeField] public List<EventSequenceItem> serializedEventSequenceItems = new List<EventSequenceItem>() { };
        [SerializeField] public List<int> serializedEventSequenceItemIndexes = new List<int>() { };

        [SerializeField] public List<WaitSequenceItem> serializedWaitSequenceItems = new List<WaitSequenceItem>() { };
        [SerializeField] public List<int> serializedWaitSequenceItemIndexes = new List<int>() { };
        //[HideInInspector][SerializeField] public List<AnimationCurveSequenceItem> serializedAnimSequenceItems = new List<AnimationCurveSequenceItem>() { };
        //[HideInInspector][SerializeField] public List<int> animSequenceItemIndexes = new List<int>() { };

        private List<List<int>> indexingLists;

        public Transform targetToAnim = null;
        public Queue<int> insertionIndexQueue = new();

        ISequenceItem startingItem;

        public ISequenceItem currentItem;

        public void Awake()
        {
            PrepSequence();
        }

        /// <summary>
        /// Loads the animation Sequence. This occurs automatically on awake. Ideally run asynchronously.
        /// </summary>
        public void PrepSequence()
        {
            if (serializedDataForEditor != null)
            {
                ISequenceItem lastItem = null;

                for (int i = serializedDataForEditor.Count - 1; i >= 0; i--)
                {

                    if (transformSequenceItemIndexes.Contains(i))
                    {
                        int index = transformSequenceItemIndexes.IndexOf(i);
                        serializedTransformSequenceItems[(int)index].nextSequence = lastItem;
                        serializedTransformSequenceItems[(int)index].SetOwnerComponent(this);

                        lastItem = serializedTransformSequenceItems[(int)index];
                        Debug.Log("Transform");
                    }

                    if (serializedEventSequenceItemIndexes.Contains(i))
                    {
                        int index = serializedEventSequenceItemIndexes.IndexOf(i);
                        serializedEventSequenceItems[(int)index].nextSequence = lastItem;
                        serializedEventSequenceItems[(int)index].SetOwnerComponent(this);

                        lastItem = serializedEventSequenceItems[(int)index];
                        Debug.Log("Event");
                    }

                    if (serializedWaitSequenceItemIndexes.Contains(i))
                    {
                        int index = serializedWaitSequenceItemIndexes.IndexOf(i);
                        serializedWaitSequenceItems[(int)index].nextSequence = lastItem;
                        serializedWaitSequenceItems[(int)index].SetOwnerComponent(this);

                        lastItem = serializedWaitSequenceItems[(int)index];
                        Debug.Log("Scale");
                    }
                }

                startingItem = lastItem;
            }
        }

        /// <summary>
        /// Plays a prepared sequence.
        /// </summary>
        public void PlaySequence()
        {
            if (startingItem == null)
            {
                Debug.LogWarning("Could not find Starting Item");
            }
            startingItem?.Start();
        }

        /// <summary>
        /// Plays a prepared sequence, starting at a certain depth.
        /// </summary>
        public void StartSequneceFromIndex(int index)
        {
            ISequenceItem next = null;
            if (startingItem != null)
            {
                next = startingItem;
                for (int i = 0; i < index; i++)
                {
                    if(next == null)
                    {
                        return;
                    }

                    next = next.GetNext();
                }
            }

            next.Start();
        }

        /// <summary>
        /// Forcibly end the sequence.
        /// </summary>
        /// <param name="complete">Complete all sequence items</param>
        public void Quit(bool complete)
        {
            currentItem.Quit(complete);
        }

#region List Operations
        public void AddNew()
        {
            if (serializedDataForEditor == null)
            {
                serializedDataForEditor = new();
                serializedTransformSequenceItems = new();
                transformSequenceItemIndexes = new();
            }

            serializedDataForEditor.Add((new()));

            insertionIndexQueue.Enqueue(0);
        }

        public void InsertNewAt(int index)
        {
            insertionIndexQueue.Enqueue(index);
        }

        public void RemoveAt(int index)
        {
            if (serializedDataForEditor == null)
            {
                serializedDataForEditor = new();
                serializedTransformSequenceItems = new();
            }

            for (int i = 0; i < transformSequenceItemIndexes.Count; i++)
            {
                if(transformSequenceItemIndexes[i] == index)
                {
                    transformSequenceItemIndexes.RemoveAt(i);
                    serializedTransformSequenceItems.RemoveAt(i);
                }

                if (transformSequenceItemIndexes[i] > index)
                    transformSequenceItemIndexes[i]--;
            }

            serializedDataForEditor.RemoveAt(index);
        }

        public ISequenceItem GetAnimAtIndex(int index)
        {

            if(transformSequenceItemIndexes.Contains(index))
            {
                int i = transformSequenceItemIndexes.IndexOf(index);
                return serializedTransformSequenceItems[i];
            }
            return null;
        }



        public void OnBeforeSerialize()
        {
        }

        //public void OnAfterDeserialize()
        //{
        //    for (int i = 0; i < serializedDataForEditor.Count; i++)
        //    {
        //        ModifyTypeAtIndex(i, serializedDataForEditor[i].type);
        //    }
        //}
        #endregion


    }

}


