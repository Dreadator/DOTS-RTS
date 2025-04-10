using UnityEngine;

public class UnitSelectionUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;
    [SerializeField] private Canvas selectionAreaRectCanvas;

    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;

        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
            UpdateVisual();
    }

    private void UnitSelectionManager_OnSelectionAreaStart(object sender, System.EventArgs e) 
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }
    private void UnitSelectionManager_OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual() 
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelctionAreaRect();

        float canvasScale = selectionAreaRectCanvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
    }

}
