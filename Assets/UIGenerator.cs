using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{
    [SerializeField] UITextBlock txtBlockPrefab;
    [Space]
    [SerializeField] Canvas canvasLeft;
    [SerializeField] Image whiteBg;

    public async UniTask DrawSchematicText(List<VisualElement> allElements) 
    {
        List<UITextBlock> txtBlocks = new List<UITextBlock>();
        foreach (var element in allElements)
        {
            if (element.SchematicItem.element._writePartOnDoc)
            {
                txtBlocks.Add(DrawText(element));
            }
        }

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        for (int i = 0; i < txtBlocks.Count - 1; i++)
        {
            txtBlocks[i].OverlapText(txtBlocks[i + 1]);
        }
    }

    public UITextBlock DrawText(VisualElement element) 
    {
        UITextBlock txtBlock = Instantiate(txtBlockPrefab, canvasLeft.transform);
        txtBlock.gameObject.name = $"TXT - {element.SchematicItem.ToString()}";
        txtBlock.SetupText(element);
        return txtBlock;
    }

    public void SetBGHeight(float height) 
    {
        whiteBg.transform.localScale += new Vector3(0, height, 0);
    }
}
