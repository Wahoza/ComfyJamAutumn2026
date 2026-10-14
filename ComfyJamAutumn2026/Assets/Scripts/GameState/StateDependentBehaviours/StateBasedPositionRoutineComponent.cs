using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StateBasedPositionRoutineComponent : MonoBehaviour, IGameStateListener
{
    [SerializeField] List<IGameState> enabledStates;
    [SerializeField] Transform disabledPosition;
    [SerializeField] Transform enabledPosition;
    [SerializeField] GameObject target;
    [SerializeField] float moveDuration;
    [SerializeField] AnimationCurve moveCurve;
    [SerializeField] UnityEvent eventOnDisable;
    [SerializeField] UnityEvent eventOnEnable;


    private Coroutine moveRoutine;
    void Start()
    {
        (this as IGameStateListener).Subscribe();

        if (enabledStates.Contains(GameStateManager.Instance.GetGameState())){
            target.transform.position = enabledPosition.position;
            eventOnEnable.Invoke();
        }
        else
        {
            target.transform.position = disabledPosition.position;
            eventOnDisable.Invoke();
        }
    }
    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        if (enabledStates.Contains(newState) && !enabledStates.Contains(oldState))
            Enable();
        else if (!enabledStates.Contains(newState) && enabledStates.Contains(oldState))
            Disable();
    }
    
    void Enable() 
    {
        if(moveRoutine != null)
            StopCoroutine(moveRoutine);

        eventOnEnable.Invoke();
        moveRoutine = StartCoroutine(EnableRoutine());
    }
    void Disable() 
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        Vector3 initPos = target.transform.position;
        Vector3 direction = disabledPosition.transform.position - initPos;

        float elapsed = 0;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            var evaluated = moveCurve.Evaluate(elapsed / moveDuration);

            target.transform.position = initPos + evaluated * direction;
            yield return null;
        }

        target.transform.position = disabledPosition.transform.position;
        eventOnDisable.Invoke();
    }

    private IEnumerator EnableRoutine()
    {
        Vector3 initPos = target.transform.position;
        Vector3 direction = enabledPosition.transform.position - initPos;

        float elapsed = 0;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            var evaluated = moveCurve.Evaluate(elapsed / moveDuration);

            target.transform.position = initPos + evaluated * direction;
            yield return null;
        }

        target.transform.position = enabledPosition.transform.position;
    }
}
