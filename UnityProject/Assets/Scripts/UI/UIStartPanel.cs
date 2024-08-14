using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.Example
{
	public class UIStartPanelData : UIPanelData
	{
	}
	public partial class UIStartPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIStartPanelData ?? new UIStartPanelData();
			// please add init code here
			Button.onClick.AddListener(OnClick);
		}

		private void OnClick()
		{
			Debug.Log("开始游戏");
		}

		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
