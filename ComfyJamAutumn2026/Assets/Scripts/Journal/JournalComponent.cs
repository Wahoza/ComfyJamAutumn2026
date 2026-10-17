using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JournalComponent : MonoBehaviour
{
    [Header("Setup")]
    [SerializedDictionary("JournalTab", "JournalChapterObject")]
    [SerializeField] SerializedDictionary<string, JournalChapterObject> journalChapterDictionary;

    [SerializeField] string initSelectedTab;

    [SerializeField] RectTransform leftPageHolder;
    [SerializeField] RectTransform rightPageHolder;

    private Dictionary<string, ChapterData> instancedJournalData = new();

    [Header("Navigation")]
    [SerializedDictionary("JournalChapterObject", "TabButton")]
    [SerializeField] SerializedDictionary<string, Button> journalTabChapterBookmarks;
    [SerializeField] Transform originXBookmark;
    [SerializeField] Transform targetXBookmark;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;

    private int _iIndex;
    private Button currentBookmark;

    private ChapterData _currentChapter;

    [Header("PageAnimation")]
    [SerializeField] RectTransform contentHolder;
    [SerializeField] RectTransform animationHolder;
    [SerializeField] RectTransform leftPageAnimationHolder;
    [SerializeField] RectTransform leftPageOrigin;
    [SerializeField] RectTransform leftPageTarget;
    [SerializeField] RectTransform rightPageAnimationHolder;
    [SerializeField] RectTransform rightPageTarget;
    [SerializeField] RectTransform rightPageOrigin;

    [SerializeField] float durationFromNavigationInput;
    [SerializeField] float durationWithInformationDrag;

    [SerializeField] AnimationCurve pageAnimCurve;

    [Header("Bookmark Animation")]
    [SerializeField] float bookmarkMoveDuration;
    [SerializeField] AnimationCurve bookmarkAnimCurve;

    private Coroutine pageFlippingRoutine;
    private Coroutine bookmarkMovingRoutine;
    private int flipDirection = 0;

    List<GameObject> _shownPages = new();
    List<int> _currentPages = new();
    void Start()
    {
        SetupJournal();
    }
    void SetupJournal()
    {
        foreach(var item in journalChapterDictionary)
        {
            if (!instancedJournalData.ContainsKey(item.Key))
            {
                instancedJournalData.Add(item.Key, SetupChapter(item.Value, item.Key));
            }
            else
            {
                instancedJournalData[item.Key] = SetupChapter(item.Value, item.Key);
            }
        }
        ShowPagesAt(initSelectedTab, 0);

        rightButton.onClick.AddListener(() => GoToNextPage(durationFromNavigationInput));
        leftButton.onClick.AddListener(() => GoToPreviousPage(durationFromNavigationInput));

        InputManager.InputActions.Journal.NavigateJournal.performed += OnNavigationInput;

        foreach(var item in journalTabChapterBookmarks)
        {
            item.Value.onClick.AddListener(() => OnBookmarkClick(item.Key));
        }

        SelectBookmarkFromString(initSelectedTab);
        DetermineEnabledButtons();
    }

    ChapterData SetupChapter(JournalChapterObject chapter, string chaptername)
    {
        ChapterData chapterData = new ChapterData();
        chapterData.instancedPages = new();
        chapterData.name = chaptername;

        int iterator = 0;
        
        foreach (var kvp in chapter.filledPagePrefabCondition)
        {
            if(kvp.Value != null)
            {
                RectTransform pageHolder = iterator % 2 == 0 ? leftPageHolder : rightPageHolder;
                GameObject instancedGO = Instantiate(kvp.Key, pageHolder);
                RectTransform instancedRect = instancedGO.GetComponent<RectTransform>();

                instancedRect.anchorMin = pageHolder.anchorMin;
                instancedRect.anchorMax = pageHolder.anchorMax;

                instancedGO.GetComponent<PageComponent>().SetPageIndicator($"{iterator + 1} - {chaptername}");
                instancedGO.GetComponent<PageComponent>().Initalize();

                instancedGO.SetActive(false);

                chapterData.instancedPages.Add(instancedGO);
            }

            iterator++;
        }

        return chapterData;
    }

    #region Page Selection
    public void GoNextPageFromHover()
    {
        GoToNextPage(durationWithInformationDrag);
    }
    public void GoPreviousPageFromHover()
    {
        GoToPreviousPage(durationWithInformationDrag);
    }
    public void GoToNextPage(float duration)
    {
        EndAnimationRoutine();

        if (_currentChapter.instancedPages.Count <= _iIndex + 2)
            return;

        animationHolder.SetAsLastSibling();

        if(_shownPages.Count == 0)
            return;

        //_shownPages[0].transform.parent = leftPageAnimationHolder;

        if(_shownPages.Count > 1)
        _shownPages[1].transform.SetParent(rightPageAnimationHolder);

        _currentChapter.instancedPages[_iIndex + 2].transform.SetParent(leftPageAnimationHolder);
        _currentChapter.instancedPages[_iIndex + 2].gameObject.SetActive(true);
        _shownPages.Add(_currentChapter.instancedPages[_iIndex + 2]);

        _currentPages = new() { _iIndex + 2 };

        if (_currentChapter.instancedPages.Count > _iIndex + 3)
        {
            _currentChapter.instancedPages[_iIndex + 3].gameObject.SetActive(true);

            _shownPages.Add(_currentChapter.instancedPages[_iIndex + 3]);
            _currentPages.Add(_iIndex + 3);
        }


        flipDirection = 1;
        pageFlippingRoutine = StartCoroutine(GoToNextPageRoutine(duration));
    }
    private IEnumerator GoToNextPageRoutine(float duration)
    {
        float elapsed = 0;

        Vector3 initLeftRotation = leftPageOrigin.eulerAngles;
        Vector3 targetLeftRotation = leftPageTarget.eulerAngles;
        Vector3 leftDirection = initLeftRotation - targetLeftRotation;

        Vector3 initRightRotation = rightPageOrigin.eulerAngles;
        Vector3 targetRightRotation = rightPageTarget.eulerAngles;
        Vector3 rightDirection = targetRightRotation - initRightRotation;

        rightPageAnimationHolder.eulerAngles = initRightRotation;
        leftPageAnimationHolder.eulerAngles = initLeftRotation;

        leftPageAnimationHolder.gameObject.SetActive(false);
        rightPageAnimationHolder.gameObject.SetActive(true);

        while (elapsed < duration / 2)
        {
            float eval = pageAnimCurve.Evaluate(elapsed / (duration / 2));
            rightPageAnimationHolder.eulerAngles = initRightRotation + eval * rightDirection;

            yield return null;
            elapsed += Time.deltaTime;
        }

        rightPageAnimationHolder.gameObject.SetActive(false);
        leftPageAnimationHolder.gameObject.SetActive(true);

        elapsed = 0;


        while (elapsed < duration/2) 
        {
            float eval = pageAnimCurve.Evaluate(1 - (elapsed / (duration / 2)));
            leftPageAnimationHolder.eulerAngles = initLeftRotation - eval * leftDirection;

            yield return null;
            elapsed += Time.deltaTime;
        }

        EndAnimationRoutine();
    }
    void EndAnimationRoutine()
    {
        if (pageFlippingRoutine != null)
        {
            StopCoroutine(pageFlippingRoutine);
            pageFlippingRoutine = null;
        }


        leftPageAnimationHolder.eulerAngles = leftPageOrigin.eulerAngles;
        rightPageAnimationHolder.eulerAngles = rightPageOrigin.eulerAngles;

        _iIndex += flipDirection * 2;
        flipDirection = 0;
        ShowPagesAt(_currentChapter.name, _iIndex);
    }
    public void GoToPreviousPage(float duration)
    {
        EndAnimationRoutine();

        if (_iIndex < 2)
            return;

        animationHolder.SetAsLastSibling();

        if (_shownPages.Count == 0)
            return;

        //_shownPages[0].transform.parent = leftPageAnimationHolder;

        _currentChapter.instancedPages[_iIndex].transform.SetParent(leftPageAnimationHolder);
        _currentChapter.instancedPages[_iIndex].gameObject.SetActive(true);

        _currentChapter.instancedPages[_iIndex - 2].gameObject.SetActive(true);
        _shownPages.Add(_currentChapter.instancedPages[_iIndex - 2]);

        _currentChapter.instancedPages[_iIndex - 1].SetActive(true);
        //_currentData.instancedPages[_iIndex - 1].transform.parent = rightPageAnimationHolder;

        _currentPages = new() { _iIndex - 1, _iIndex - 2 };

        flipDirection = -1;
        pageFlippingRoutine = StartCoroutine(GoPreviousPageRoutine(duration));
    }
    private IEnumerator GoPreviousPageRoutine(float duration)
    {
        float elapsed = 0;

        Vector3 initLeftRotation = leftPageOrigin.eulerAngles;
        Vector3 targetLeftRotation = leftPageTarget.eulerAngles;
        Vector3 leftDirection = targetLeftRotation - initLeftRotation;

        Vector3 initRightRotation = rightPageOrigin.eulerAngles;
        Vector3 targetRightRotation = rightPageTarget.eulerAngles;
        Vector3 rightDirection = initRightRotation - targetRightRotation;

        rightPageAnimationHolder.eulerAngles = initRightRotation;
        leftPageAnimationHolder.eulerAngles = initLeftRotation;

        rightPageAnimationHolder.gameObject.SetActive(false);
        leftPageAnimationHolder.gameObject.SetActive(true);

        while (elapsed < duration / 2)
        {
            float eval = pageAnimCurve.Evaluate(elapsed / (duration / 2));
            leftPageAnimationHolder.eulerAngles = initLeftRotation + eval * leftDirection;

            yield return null;
            elapsed += Time.deltaTime;
        }

        elapsed = 0;

        leftPageAnimationHolder.gameObject.SetActive(false);
        rightPageAnimationHolder.gameObject.SetActive(true);

        while (elapsed < duration / 2)
        {
            float eval = pageAnimCurve.Evaluate(1 - (elapsed / (duration / 2)));
            rightPageAnimationHolder.eulerAngles = initRightRotation - eval * rightDirection;

            yield return null;
            elapsed += Time.deltaTime;
        }

        EndAnimationRoutine();
    }
    public void DetermineEnabledButtons()
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        if(_iIndex > 1)
        {
            leftButton.gameObject.SetActive(true);
        }
        if (_iIndex + 2 < _currentChapter.instancedPages.Count) 
        { 
            rightButton.gameObject.SetActive(true);
        }
    }
    public void ShowPagesAt(string chapter, int page)
    {
        contentHolder.SetAsLastSibling();

        foreach(var oldPage in _shownPages)
        {
            oldPage.SetActive(false);
        }

        _shownPages.Clear();

        if (journalChapterDictionary.ContainsKey(chapter))
        {
            if (page < instancedJournalData[chapter].instancedPages.Count)
            {
                _shownPages.Add(instancedJournalData[chapter].instancedPages[page]);
                instancedJournalData[chapter].instancedPages[page].transform.SetParent(leftPageHolder);
                _currentPages = new() { page };
            }

            if (page + 1 < instancedJournalData[chapter].instancedPages.Count)
            {
                _shownPages.Add(instancedJournalData[chapter].instancedPages[page + 1]);
                instancedJournalData[chapter].instancedPages[page + 1].transform.SetParent(rightPageHolder);
                _currentPages.Add(page + 1);
            }
        }
        foreach (var newPage in _shownPages)
        {
            newPage.SetActive(true);
        }

        _currentChapter = instancedJournalData[chapter];
        _iIndex = page;

        DetermineEnabledButtons();
    }

    public void OnNavigationInput(InputAction.CallbackContext ctx)
    {
        var dir = ctx.ReadValue<Vector2>();
        if(dir.x > 0)
        {
            GoToNextPage(durationFromNavigationInput);
        }
        else
        {
            GoToPreviousPage(durationFromNavigationInput);
        }
    }

    public (Transform, Transform) GetCurrentPages()
    {
        (Transform, Transform) returnVal = (null, null);

        if(_currentPages.Count > 0)
        {
            returnVal.Item1 = _currentChapter.instancedPages[_currentPages[0]].transform;
        }

        if(_currentPages.Count > 1)
        {
            returnVal.Item2 = _currentChapter.instancedPages[_currentPages[1]].transform;
        }

        return returnVal;
    }
    #endregion

    #region Bookmark Selection
    public void OnBookmarkClick(string chapter)
    {
        if(chapter == _currentChapter.name)
            return;

        if (!journalTabChapterBookmarks.ContainsKey(chapter))
            return;
        
        if (bookmarkMovingRoutine != null)
            StopCoroutine(bookmarkMovingRoutine);

        ShowPagesAt(chapter, 0);

        bookmarkMovingRoutine = StartCoroutine(MoveBookmarks(journalTabChapterBookmarks[chapter]));
    }
    private IEnumerator MoveBookmarks(Button selectedBookmark)
    {
        List<Vector3> origins = new();
        List<Vector3> directions = new();
        List<Button> buttons = new();
        foreach(var tab in journalTabChapterBookmarks)
        {
            if(tab.Value == selectedBookmark)
            {
                Vector3 origin = selectedBookmark.transform.position;
                Vector3 target = new Vector3(targetXBookmark.position.x, origin.y, origin.z);
                origins.Add(origin);
                directions.Add(target - origin);
            }
            else
            {
                Vector3 origin = tab.Value.transform.position;
                Vector3 target = new Vector3(originXBookmark.position.x, origin.y, origin.z);
                origins.Add(origin);
                directions.Add(target - origin);
            }

            buttons.Add(tab.Value);
        }
        float elapsed = 0;
        while (elapsed < bookmarkMoveDuration)
        {
            elapsed += Time.deltaTime;
            float evaluated = bookmarkAnimCurve.Evaluate(elapsed/bookmarkMoveDuration);

            for (int i = 0; i < buttons.Count; i++) 
            {
                buttons[i].transform.position = evaluated * directions[i] + origins[i];
            }

            yield return null;
        }

        SelectBookmark(selectedBookmark);
    }
    private void SelectBookmarkFromString(string selectedBookmark)
    {
        if(journalTabChapterBookmarks.ContainsKey(selectedBookmark))
            SelectBookmark(journalTabChapterBookmarks[selectedBookmark]);
    }

    private void SelectBookmark(Button selectedBookmark)
    {
        foreach(var bookmark in journalTabChapterBookmarks)
        {
            Vector3 pos = bookmark.Value.transform.position;
            bookmark.Value.transform.position = new Vector3(originXBookmark.position.x, pos.y, pos.z);
        }

        if(selectedBookmark == null)
            return;

        Vector3 selectedPos = selectedBookmark.transform.position;

        selectedBookmark.transform.position = new Vector3(targetXBookmark.position.x, selectedPos.y, selectedPos.z);
    }

    #endregion
    private class ChapterData
    {
        public string name;
        public List<GameObject> instancedPages;
    }
}
