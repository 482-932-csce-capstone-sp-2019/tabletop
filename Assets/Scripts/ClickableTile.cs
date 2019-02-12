using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickableTile : MonoBehaviour {

	public int tileX;
	public int tileY;
	public TileMap map;

	void OnMouseUp()
	{
		//Debug.Log ("Click!");

		if(EventSystem.current.IsPointerOverGameObject())
			return;

		if(map.isUserOnTile(tileX,tileY) && map.selectedUnit == null)
		{
			map.selectUnit(tileX,tileY);
		}
		if(map.selectedUnit != null)
		{
			map.GeneratePathTo(tileX, tileY);
		}
	}

}
