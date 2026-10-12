using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메인 화면의 스테이지 선택창. Main 씬의 Canvas에 붙어 있고,
// 기존 Start 버튼을 복제해서 같은 모양의 버튼을 만든다.
public class StageSelectMenu : MonoBehaviour
{
    [System.Serializable]
    public struct StageEntry
    {
        public string sceneName;
        public string title;     // 버튼 큰 글씨
        public string subtitle;  // 버튼 작은 글씨 (핵심 기믹)
    }

    public StageEntry[] stages =
    {
        new StageEntry { sceneName = "Stage1",      title = "1",      subtitle = "Body & Soul" },
        new StageEntry { sceneName = "Stage2",      title = "2",      subtitle = "Switches" },
        new StageEntry { sceneName = "Stage3",      title = "3",      subtitle = "Box & Bars" },
        new StageEntry { sceneName = "Stage4",      title = "4",      subtitle = "No Undo" },
        new StageEntry { sceneName = "Stage5",      title = "5",      subtitle = "Possess" },
        new StageEntry { sceneName = "Stage6",      title = "6",      subtitle = "Lift" },
        new StageEntry { sceneName = "Stage7",      title = "7",      subtitle = "Phase Pillar" },
        new StageEntry { sceneName = "Stage8",      title = "8",      subtitle = "Timed Gate" },
        new StageEntry { sceneName = "Stage9",      title = "9",      subtitle = "Soul Freeze" },
        new StageEntry { sceneName = "Stage10",     title = "10",     subtitle = "Soul Leash" },
        new StageEntry { sceneName = "Stage11",     title = "11",     subtitle = "Soul Record" },
        new StageEntry { sceneName = "ButtonStage", title = "Button", subtitle = "One Step" },
        new StageEntry { sceneName = "FinalStage",  title = "Final",  subtitle = "Last Escape" },
    };

    public int columns = 5;
    public Vector2 cellSize = new Vector2(160, 100);
    public Vector2 spacing = new Vector2(16, 16);

    // 스테이지에서 Esc로 돌아왔을 때 선택창을 바로 연다
    static bool openOnLoad;

    GameObject panel;
    readonly List<GameObject> menuItems = new List<GameObject>(); // 선택창이 열리면 숨길 메인 화면 요소

    void Start()
    {
        GameObject startButton = GameObject.Find("Start Button");
        GameObject endButton = GameObject.Find("End Button");
        if (startButton == null)
        {
            Debug.LogWarning("StageSelectMenu: Start Button을 찾지 못했습니다.");
            return;
        }

        // 메인 화면에 Stage Select 버튼 추가 (Start와 End 사이)
        RectTransform startRect = (RectTransform)startButton.transform;
        Button selectButton = CloneButton(startButton, startButton.transform.parent, "Stage Select Button", 0, "Stage Select");
        RectTransform selectRect = (RectTransform)selectButton.transform;
        float gap = endButton != null
            ? startRect.anchoredPosition.y - ((RectTransform)endButton.transform).anchoredPosition.y
            : startRect.sizeDelta.y + 40;
        selectRect.anchoredPosition = startRect.anchoredPosition - new Vector2(0, gap);
        selectButton.onClick.AddListener(Open);
        if (endButton != null)
        {
            RectTransform endRect = (RectTransform)endButton.transform;
            endRect.anchoredPosition -= new Vector2(0, gap);
            // 아래로 밀린 End 버튼이 화면 밖으로 나가지 않도록 전체를 위로 올린다
            float overflow = endRect.sizeDelta.y * 0.5f + 16 - endRect.anchoredPosition.y;
            if (overflow > 0)
            {
                startRect.anchoredPosition += new Vector2(0, overflow);
                selectRect.anchoredPosition += new Vector2(0, overflow);
                endRect.anchoredPosition += new Vector2(0, overflow);
            }
        }

        menuItems.Add(startButton);
        menuItems.Add(selectButton.gameObject);
        if (endButton != null) menuItems.Add(endButton);
        GameObject title = GameObject.Find("Title");
        if (title != null) menuItems.Add(title);

        panel = BuildPanel(startButton);
        SetPanelOpen(openOnLoad);
        openOnLoad = false;
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open() { SetPanelOpen(true); }
    public void Close() { SetPanelOpen(false); }

    void SetPanelOpen(bool open)
    {
        if (panel == null) return;
        panel.SetActive(open);
        foreach (GameObject item in menuItems)
            item.SetActive(!open);
    }

    GameObject BuildPanel(GameObject buttonTemplate)
    {
        // 화면 전체를 덮는 어두운 배경
        GameObject root = new GameObject("Stage Select Panel", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(transform, false);
        root.transform.SetAsLastSibling();
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 1f);

        TMP_Text templateLabel = buttonTemplate.GetComponentInChildren<TMP_Text>();

        // 제목
        TMP_Text heading = CreateText(root.transform, "Heading", "Select Stage", 56, templateLabel);
        RectTransform headingRect = heading.rectTransform;
        headingRect.anchorMin = headingRect.anchorMax = new Vector2(0.5f, 1);
        headingRect.pivot = new Vector2(0.5f, 1);
        headingRect.anchoredPosition = new Vector2(0, -40);
        headingRect.sizeDelta = new Vector2(800, 80);

        // 스테이지 버튼 격자
        GameObject grid = new GameObject("Stage Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(root.transform, false);
        int rows = Mathf.CeilToInt(stages.Length / (float)columns);
        RectTransform gridRect = (RectTransform)grid.transform;
        gridRect.anchorMin = gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(
            columns * cellSize.x + (columns - 1) * spacing.x,
            rows * cellSize.y + (rows - 1) * spacing.y);
        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        layout.cellSize = cellSize;
        layout.spacing = spacing;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;
        layout.childAlignment = TextAnchor.UpperCenter;

        foreach (StageEntry stage in stages)
        {
            string sceneName = stage.sceneName;
            Button button = CloneButton(buttonTemplate, grid.transform, sceneName + " Button", 24,
                $"<size=150%>{stage.title}</size>\n<size=70%>{stage.subtitle}</size>");
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
            else
                button.interactable = false; // Build Settings에 없는 씬
        }

        // 뒤로가기
        Button back = CloneButton(buttonTemplate, root.transform, "Back Button", 0, "Back");
        RectTransform backRect = (RectTransform)back.transform;
        backRect.anchorMin = backRect.anchorMax = new Vector2(0.5f, 0);
        backRect.pivot = new Vector2(0.5f, 0);
        backRect.anchoredPosition = new Vector2(0, 40);
        back.onClick.AddListener(Close);

        TMP_Text hint = CreateText(root.transform, "Hint", "Esc: back to menu", 24, templateLabel);
        RectTransform hintRect = hint.rectTransform;
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(1, 0);
        hintRect.pivot = new Vector2(1, 0);
        hintRect.anchoredPosition = new Vector2(-24, 16);
        hintRect.sizeDelta = new Vector2(400, 40);
        hint.alignment = TextAlignmentOptions.BottomRight;
        hint.color = new Color(1, 1, 1, 0.6f);

        return root;
    }

    // fontSize가 0이면 원본 크기를 최대로 자동 맞춤, 아니면 고정 크기 (격자 버튼끼리 글자 크기를 맞추기 위해)
    static Button CloneButton(GameObject template, Transform parent, string name, float fontSize, string label)
    {
        GameObject clone = Instantiate(template, parent, false);
        clone.name = name;
        Button button = clone.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent(); // 원본의 StartGame 연결 제거
        TMP_Text text = clone.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.margin = new Vector4(8, 4, 8, 4);
            if (fontSize > 0)
            {
                text.enableAutoSizing = false;
                text.fontSize = fontSize;
            }
            else
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = 12;
                text.fontSizeMax = text.fontSize;
            }
        }
        return button;
    }

    static TMP_Text CreateText(Transform parent, string name, string value, float size, TMP_Text style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (style != null)
        {
            text.font = style.font;
            text.color = Color.white;
        }
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    // 스테이지 안에서 Esc를 누르면 메인으로 돌아와 선택창을 연다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallEscapeListener()
    {
        GameObject listener = new GameObject("StageSelect Escape Listener");
        listener.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(listener);
        listener.AddComponent<EscapeToStageSelect>();
    }

    class EscapeToStageSelect : MonoBehaviour
    {
        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (SceneManager.GetActiveScene().name == "Main") return;
            openOnLoad = true;
            SceneManager.LoadScene("Main");
        }
    }
}
