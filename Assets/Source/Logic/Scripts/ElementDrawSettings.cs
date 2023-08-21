using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementDrawSettings : MonoBehaviour
{
	[SerializeField] List<DrawSettings> drawSettings = new List<DrawSettings>();

	[System.Serializable]
	class DrawSettings
	{
		[Header("ID")]
		public string drawName;
        [FoldoutGroup("Settings", expanded: false)][TextArea] public string description;

		[FoldoutGroup("Settings", expanded: false)] public BaseElement refElement;
        [FoldoutGroup("Settings", expanded: false)] public List<BaseElement> baseElements;

		[Button]
		public void CopyElements() 
		{
			foreach (var element in baseElements)
			{
				element.CopyDrawSettings(refElement);
            }
		}
	}
}
