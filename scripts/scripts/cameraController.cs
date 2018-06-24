
using UnityEngine;

public class cameraController : MonoBehaviour {
    public  int TeamNo;

    public float panSpeed = 500f;
    private int dir=1;
    public float scrollSpeed = 85f;
    public float panBoardThickness = -30f;
    public float xLimitMax = 1200, xLimitMin = -100, zLimitMin = -50, zLimitMax = 1800, yLimitMax = 0, yLimitMin = 0;

    void Start()
    {
      //  TeamNo = Client.getMyplayerHandler().teamNo;

    }
    
    // Use this for initialization
  
	// Update is called once per frame
	void Update () {
        
      if (Client.getMyplayerHandler() != null)  {
   

              if (Client.getMyConId() != 1)
                  dir = -1;
          }
      
          Vector3 pos = transform.position;
          if (/*Input.GetKey("w") ||*/
        Input.mousePosition.y >= Screen.height - panBoardThickness -25&& Input.mousePosition.y <= Screen.height)
        {
            pos.z +=( panSpeed * Time.deltaTime * dir);
            }
            if (/*Input.GetKey("s") || */ Input.mousePosition.y >=  0 && Input.mousePosition.y < panBoardThickness + 25)
        {
            pos.z -= (panSpeed * Time.deltaTime * dir);
            }
            if (/*Input.GetKey("d") || */Input.mousePosition.x >= Screen.width - panBoardThickness -60 && Input.mousePosition.x < Screen.width)
        {
            pos.x += (panSpeed * Time.deltaTime * dir);
            }
           if (/*Input.GetKey("a")|| */ Input.mousePosition.x >= 0  && Input.mousePosition.x <= panBoardThickness + 70)
        {
            pos.x -= (panSpeed * Time.deltaTime * dir) ; 
            }
        float scroll = Input.GetAxis ("Mouse ScrollWheel");
      //  Input.mousePosition


        pos.y -= (scroll * scrollSpeed * Time.deltaTime*200f);

		pos.x = Mathf.Clamp(pos.x, xLimitMin, xLimitMax);
		pos.y = Mathf.Clamp(pos.y, yLimitMin, yLimitMax);
		pos.z = Mathf.Clamp(pos.z, zLimitMin, zLimitMax);
		transform.position = pos;
	}

    public  void TurnCanvas()
    {
        this.transform.Find("strCam").gameObject.transform.Find("BuildM").gameObject.SetActive(true);
    }

}
