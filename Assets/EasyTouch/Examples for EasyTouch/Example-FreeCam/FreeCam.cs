using UnityEngine;
using System.Collections;

public class FreeCam : MonoBehaviour {

	private float rotationX;
	private float rotationY;
	private Camera cam;
	bool moveF = true;
	int mode = 3;
	//1: mouse
	//2: touch
	//3: sensor
	GameObject vrroot;
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_Swipe += On_Swipe;
	}
	
	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_TouchDown -= On_TouchDown;
		EasyTouch.On_Swipe -= On_Swipe;	
	}
		
	void OnGUI()
	{
		if (mode == 2)
		{
			string text = "Move Backward";
			if (moveF == true)
			{
				text = "Move Forward";
			}
			
			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 120,100,100),text))
			{
				moveF = !moveF;
			}
		}
		
		string touchableText = "";
		
		if (mode == 1)
			touchableText = "To Touch";
		else if (mode == 3)
			touchableText = "To Mouse";
		else if (mode == 2)
			touchableText = "To Sensor";
		
		if (GUI.Button(new Rect(Screen.width - 230, Screen.height - 120,100,40),touchableText))
		{
			mode++;
			if (vrroot == null)
			{
				vrroot = GameObject.Find("VRRootNode");
			}
			
			if (mode == 3)
			{
				vrroot.SetActive(true);
			}
			else 
			{
				vrroot.SetActive(false);
			}
			if (mode > 3)
				mode = 1;
		}
	}
	
	void Start(){
	
		cam = Camera.mainCamera;
	}
	
	
	void On_TouchDown(Gesture gesture){
		if (mode == 2)
		{
			if (gesture.touchCount==1 && moveF){
				cam.transform.Translate(new  Vector3(0,0,15f) * Time.deltaTime);
			}
			
			 if (gesture.touchCount==1 && !moveF){
				cam.transform.Translate( new Vector3(0,0,-15f) * Time.deltaTime);
			}	
		}
	}
	
	void Update()
	{
		
	}
	
	void On_Swipe( Gesture gesture){
		if (mode == 2)
		{
			rotationX += gesture.deltaPosition.x;
			rotationY += gesture.deltaPosition.y;
				
			cam.transform.localRotation = Quaternion.AngleAxis (rotationX, Vector3.up); 
			
			cam.transform.localRotation *= Quaternion.AngleAxis (rotationY, Vector3.left);
		}
	}
}
