using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {

	private BazarController sourceBazaar;

	public GameObject bazaarPrefab;

	int currentGUIMode; // 37 = build new bazaar, 38 = confirm new bazaar, 39 = cancel new bazaar

	private GameObject currentlyBuildingBazaar;


	// Used for ok or cancel when building bazaars
	private Rect GuiButtonAreaA;
	private Rect GuiButtonAreaB;

	// Used to assign textures for GUI elements
	public Texture2D yesicon;
	public Texture2D noicon;


	// Use this for initialization
	void Start () {
		currentGUIMode = 0;
		GuiButtonAreaA = new Rect(1,1,1,1);
		GuiButtonAreaB = new Rect(1,1,1,1);

	}
	
	// Update is called once per frame
	void Update () {

		if (currentGUIMode==37) {
						
			if (Input.GetButtonDown ("Fire1")) 
			{
				Plane plane;
				plane = new Plane(Vector3.up, Vector3.zero);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float fDist = 0.0f;
				plane.Raycast (ray, out fDist); 
				Vector3 pointedpoint = ray.GetPoint (fDist);


				bool mouseInside = false;

				// we need to invert the y axis for some stupid reason
				Vector3 tmppos = Input.mousePosition;
				tmppos.y = Screen.height - tmppos.y;

				// don't reposition if we push one of the GUI buttons!
				if (GuiButtonAreaA.Contains(tmppos)) 
				{
					mouseInside = true;
				}
				if (GuiButtonAreaB.Contains(tmppos)) 
				{
					mouseInside = true;
				}

				if (currentlyBuildingBazaar == null) {
					currentlyBuildingBazaar = (GameObject)Instantiate(bazaarPrefab, pointedpoint, Quaternion.identity);				
					Component[] renderers = currentlyBuildingBazaar.GetComponentsInChildren<Renderer>();
					foreach (Renderer rendu in renderers) { //TODO: gör det här effektivare DOY
						if(rendu.name == "tent_exterior") {
							rendu.material.color = Color.blue;
						}
					}	
					GuiButtonAreaA = new Rect(tmppos.x-65,tmppos.y-60,40,40);
					GuiButtonAreaB = new Rect(tmppos.x+25,tmppos.y-60,40,40);
				} else {
					if (mouseInside == false) {
						currentlyBuildingBazaar.transform.position = pointedpoint;
						GuiButtonAreaA = new Rect(tmppos.x-65,tmppos.y-60,40,40);
						GuiButtonAreaB = new Rect(tmppos.x+25,tmppos.y-60,40,40);
					}
				}
			}
		}
		if (currentGUIMode==38) {
			//TODO call BazarController's setColor (but hoooow I need to cast a class or something)
			Component[] renderers = currentlyBuildingBazaar.GetComponentsInChildren<Renderer>();
			foreach (Renderer rendu in renderers) { //TODO: gör det här effektivare DOY
				if(rendu.name == "tent_exterior") {
					rendu.material.color = Color.white;
				}
			}	
			currentlyBuildingBazaar = null; //This creates our previously temporary object!
			currentGUIMode = 0;
			GuiButtonAreaA = new Rect(1,1,1,1);
			GuiButtonAreaB = new Rect(1,1,1,1);
		}

		if (currentGUIMode==39){
			Destroy(currentlyBuildingBazaar);
			currentGUIMode = 0;
			GuiButtonAreaA = new Rect(1,1,1,1);
			GuiButtonAreaB = new Rect(1,1,1,1);
		}
	}

	public void selectBazaar (GameObject newSourceGameObject) {
		if (currentGUIMode==0) {
			//Let's start by de-selecting the previously selected object 
			if (sourceBazaar != null) {
				Component[] renderers = sourceBazaar.GetComponentsInChildren<Renderer>();
				foreach (Renderer rendu in renderers) { //TODO: gör det här effektivare DOY
					if(rendu.name == "tent_exterior") {
						rendu.material.color = Color.white;
					}
				}		
			}


			if (newSourceGameObject != null) {
				sourceBazaar = newSourceGameObject.GetComponent<BazarController>();

				Component[] renderers = newSourceGameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer rendu in renderers) { //TODO: gör det här effektivare DOY
					if(rendu.name == "tent_exterior") {
						rendu.material.color = Color.red;
					}
				}

			} else {
				sourceBazaar = null;
			}
		}
	}

	void OnGUI () {
		//Create Build Menu
		int buttonWidth = Screen.width/5;
		int buttonHeight = Screen.width/12;
		int buttonHeightMargin = 20;
		int buttonWidthMargin = 2;

		GUI.Box(new Rect(0,buttonHeightMargin+buttonHeight*5,buttonWidth,buttonHeight*2), "Build Menu");

		if (GUI.Button(new Rect(buttonWidthMargin, 2*buttonHeightMargin+buttonHeight*5, buttonWidth-2*buttonWidthMargin, buttonHeight), "Build Bazaar")) {
			// do other stuff when button clicked.
			currentGUIMode = 37;
		}

		// Upgrade Menu 
		GUI.Box(new Rect(0,0,buttonWidth,buttonHeight*5), "Upgrade Menu");

		if (sourceBazaar != null) {
			if (GUI.Button(new Rect(buttonWidthMargin, buttonHeightMargin, buttonWidth-2*buttonWidthMargin, buttonHeight), "Upgrade Merchant")) {
				// do other stuff when button clicked.
				sourceBazaar.incrNumberOfMerchants (1);
			}
			
			
			if (GUI.Button(new Rect(buttonWidthMargin, buttonHeightMargin+buttonHeight, buttonWidth-2*buttonWidthMargin, buttonHeight), "Upgrade Sellprice")) {
				// do other stuff when button clicked.
				sourceBazaar.incrCostOfItems(1);
			}

			if (GUI.Button(new Rect(buttonWidthMargin, buttonHeightMargin+buttonHeight*2, buttonWidth-2*buttonWidthMargin, buttonHeight), "Upgrade Radius")) {
				// do other stuff when button clicked.
				sourceBazaar.incrRadius(1);
			}
			
			if (GUI.Button(new Rect(buttonWidthMargin, buttonHeightMargin+buttonHeight*3, buttonWidth-2*buttonWidthMargin, buttonHeight), "Close")) {
				// do other stuff when button clicked.
				selectBazaar(null); // turns off button.
			}
			
		}

		// Build Bazaar Menu
		if (currentGUIMode==37) {
			//GUI.Box(new Rect(100,100,120,200), "Buildz Menuz");
		

			if (GUI.Button( GuiButtonAreaA, yesicon)) {
				// do other stuff when button clicked.
				currentGUIMode = 38;
			}
			
			
			if (GUI.Button( GuiButtonAreaB, noicon)) {
				// do other stuff when button clicked.
				currentGUIMode = 39;
			}

		}

	}
}
