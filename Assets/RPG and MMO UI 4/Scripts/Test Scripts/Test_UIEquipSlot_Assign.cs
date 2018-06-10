using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Test_UIEquipSlot_Assign : MonoBehaviour {
	
	public UIEquipSlot slot;
	public int assignItem;
	
	void Awake()
	{
		if (this.slot == null)
			this.slot = this.GetComponent<UIEquipSlot>();
	}
	
	void Start()
	{
		if (this.slot == null || UIItemDatabase.Instance == null)
		{
			this.Destruct();
			return;
		}
		
		this.slot.Assign(UIItemDatabase.Instance.GetByID(this.assignItem));
		this.Destruct();
	}
	
	private void Destruct()
	{
		DestroyImmediate(this);
	}
}
