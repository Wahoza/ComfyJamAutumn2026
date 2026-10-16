using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DraggableHandler : MonoBehaviour
{
    [SerializeField] Transform draggableHolder;

    [SerializeField] RectTransform pageLeft;
    [SerializeField] RectTransform pageRight;

    [SerializeField] JournalComponent journal;
    [SerializeField] GameObject draggableNavigation;

    public static DraggableHandler Instance;

    struct CurrentDraggable
    {
        public DraggableComponent draggable;
        public Vector3 originPos;
        public Vector2 clickOffset;
        public Transform originPage;
    }

    CurrentDraggable currentDraggable;

    private List<DraggableComponent> draggables;
    private List<DraggableSubmitAreaComponent> submitLocations;

    private Vector2 mousePos;
    void Awake()
    {
        currentDraggable = new();
        if (Instance == null)
        {
            Instance = this;

            draggables = new();
            submitLocations = new();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        InputManager.InputActions.Journal.Point.performed += OnPoint;
        InputManager.InputActions.Journal.CloseJournal.performed += OnJournalClose;

        draggableNavigation.gameObject.SetActive(false);
    }
    // Update is called once per frame
    private void Update()
    {
        if (InputManager.InputActions.Journal.Click.WasReleasedThisFrame())
        {
            OnDragCancel();
        }
    }
    private void OnPoint(InputAction.CallbackContext ctx)
    {
        mousePos = ctx.ReadValue<Vector2>();
        if (currentDraggable.draggable != null) {
            currentDraggable.draggable.transform.position = mousePos + currentDraggable.clickOffset;
        }
    }
    private void OnJournalClose(InputAction.CallbackContext ctx)
    {
        OnDragAbort();
    }
    public void OnDraggableHover()
    {
    }
    public void OnDraggableClick(DraggableComponent draggable)
    {
        if (currentDraggable.draggable != null)
        {
            OnDragCancel();
        }

        draggableNavigation.gameObject.SetActive(true);

        currentDraggable.draggable = draggable;
        currentDraggable.originPage = draggable.transform.parent;
        currentDraggable.originPos = draggable.transform.localPosition;
        currentDraggable.clickOffset = (Vector2)draggable.transform.position - mousePos;

        draggable.transform.SetParent(draggableHolder);
    }

    void OnDragCancel()
    {
        draggableNavigation.gameObject.SetActive(false);


        if (currentDraggable.draggable == null)
            return;


        bool isValidDrop = false;

        var pages = journal.GetCurrentPages();
        Rect dragRect = currentDraggable.draggable.GetComponent<RectTransform>().rect;

        foreach (var submitLocation in submitLocations) 
        {
            if (submitLocation.gameObject.activeSelf && submitLocation.nameKey == currentDraggable.draggable.nameKey) 
            {
                var submitRect = GetRectWithObjectPosition(submitLocation.GetComponent<RectTransform>());

                dragRect.position = submitLocation.transform.InverseTransformPoint(currentDraggable.draggable.transform.position);

                if (submitRect.Overlaps(dragRect))
                {
                    isValidDrop = true;

                    currentDraggable.draggable.transform.SetParent(submitLocation.transform);
                    currentDraggable.draggable.transform.position = submitLocation.submitPosition.position;
                    currentDraggable.draggable.transform.rotation = submitLocation.submitPosition.rotation;
                    currentDraggable.draggable.transform.SetAsLastSibling();

                    Blackboard.Instance.SetJournalDictBool(currentDraggable.draggable.nameKey, true);

                    currentDraggable.draggable.isSubmittedCorrectly = true;
                    currentDraggable.draggable = null;

                    return;
                }
            }     
        }

        if (pages.Item1 != null && !isValidDrop)
        {
            var pageRect = GetRectWithObjectPosition(pageLeft);

            dragRect.position = pageLeft.InverseTransformPoint(currentDraggable.draggable.transform.position);

            if (pageRect.Overlaps(dragRect))
            {
                isValidDrop = true;

                currentDraggable.draggable.transform.SetParent(pages.Item1);
                currentDraggable.draggable.transform.SetAsLastSibling();
                currentDraggable.draggable = null;

                return;
            }
        }
        
        if (pages.Item2 != null && !isValidDrop)
        {
            var pageRect = GetRectWithObjectPosition(pageRight);
            dragRect.position = pageRight.InverseTransformPoint(currentDraggable.draggable.transform.position);

            if (pageRect.Overlaps(dragRect))
            {
                isValidDrop = true;

                currentDraggable.draggable.transform.SetParent(pages.Item2);
                currentDraggable.draggable.transform.SetAsLastSibling();
                currentDraggable.draggable = null;

                return;
            }
        }

        if (!isValidDrop)
        {
            OnDragAbort();
            return;
        }
    }
    public Rect GetRectWithObjectPosition(RectTransform rect)
    {
        Rect returnVal = rect.rect;
        //returnVal.position = rect.position;

        return returnVal;
    }
    public void RegisterDraggable(DraggableComponent draggable)
    {
        draggables.Add(draggable);
    }

    void OnDragAbort()
    {
        draggableNavigation.gameObject.SetActive(false);

        if (currentDraggable.draggable == null)
            return;

        currentDraggable.draggable.transform.SetParent(currentDraggable.originPage);
        currentDraggable.draggable.transform.localPosition = currentDraggable.originPos;
        currentDraggable.draggable = null;
    }

    public void RegisterSubmitLocation(DraggableSubmitAreaComponent comp)
    {
        submitLocations.Add(comp);
    }
}
