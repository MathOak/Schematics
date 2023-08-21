using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{
    [SerializeField] UITextBlock txtBlockLeftPrefab;
    [SerializeField] UITextBlock txtBlockRightPrefab;
    [Space]
    [SerializeField] RectTransform canvasLeft;
    [SerializeField] RectTransform canvasRight;
    [SerializeField] Image whiteBg;

    public async UniTask DrawSchematicText(List<VisualElement> allElements) 
    {
        List<UITextBlock> txtBlocks = new List<UITextBlock>();
        foreach (var element in allElements)
        {
            if (element.SchematicItem.element._writePartOnDoc)
            {
                txtBlocks.Add(DrawText(element, txtBlockLeftPrefab, canvasLeft, true));
                DrawText(element, txtBlockRightPrefab, canvasRight, false);
            }
        }

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        for (int i = 0; i < txtBlocks.Count - 1; i++)
        {
            txtBlocks[i].OverlapText(txtBlocks[i + 1]);
        }

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();
    }

    public UITextBlock DrawText(VisualElement element, UITextBlock txtPrefab, RectTransform parent, bool setPosition) 
    {
        UITextBlock txtBlock = Instantiate(txtPrefab, parent);
        txtBlock.gameObject.name = $"TXT - {element.SchematicItem.ToString()}";
        txtBlock.SetupText(element, setPosition);
        return txtBlock;
    }

    public void SetBGHeight(float height) 
    {
        whiteBg.transform.localScale += new Vector3(0, height, 0);
    }
}
