
using UnityEngine;

public class cameraController : MonoBehaviour {

	// Use this for initialization
	public float panSpeed = 500f;
	public float scrollSpeed =100f;
	public float panBoardThickness = 0.3f;
	public float xLimitMax=415  ,xLimitMin=0,  zLimitMin=20 ,zLimitMax=444,yLimitMax=150  ,yLimitMin=7;
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
      	if (/*Input.GetKey("w") ||*/ Input.mousePosition.y >= Screen.height ){
                pos.z += panSpeed * Time.deltaTime;
            }
            if (/*Input.GetKey("s") || */ Input.mousePosition.y <=  0){
                pos.z -= panSpeed * Time.deltaTime;
            }
            if (/*Input.GetKey("d") || */Input.mousePosition.x >= Screen.width - panBoardThickness ){
                pos.x += panSpeed * Time.deltaTime;
            }
            if (/*Input.GetKey("a")|| */ Input.mousePosition.x <=  0){
                pos.x -= panSpeed * Time.deltaTime;
            }
        float scroll = Input.GetAxis ("Mouse ScrollWheel");


		pos.y -= (scroll * scrollSpeed * Time.deltaTime*750f);

		pos.x = Mathf.Clamp(pos.x, xLimitMin, xLimitMax);
		pos.y = Mathf.Clamp(pos.y, yLimitMin, yLimitMax);
		pos.z = Mathf.Clamp(pos.z, zLimitMin, zLimitMax);
		transform.position = pos;
	}
}
