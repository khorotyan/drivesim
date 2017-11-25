using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Transform cityObj;
    public Transform panelObj;
    public Transform indicatorObj;
    public Text feedbackText;
    public Transform cam;
    public GameObject blocker;

    [Space(10)]

    public Transform car1;
    public Transform car2;

    private InputField[] inputs = new InputField[6];

    private float carWidth = 1.2f;
    private bool playing = false;
    private PhysInfo ph;

    private float time = 0;

    private void Awake()
    {
        ph = new PhysInfo(20, 7, 7, 2, 1, -3);

        InitializeUI();
    }

    private void Update()
    {
        Animate();
    }

    // Initialize all inputfields
    private void InitializeUI()
    {
        InitializeItem(0, 20, 80);
        InitializeItem(1, 7, 50);
        InitializeItem(2, 7, 50);
        InitializeItem(3, 2, 4);
        InitializeItem(4, 1, 3);
        InitializeItem(5, -3, -1);
    }

    // Manage an individual inputfield
    private void InitializeItem(int id, int min, int max)
    {
        inputs[id] = panelObj.GetChild(id).GetChild(0).GetComponent<InputField>();
        inputs[id].contentType = InputField.ContentType.DecimalNumber;

        inputs[id].onEndEdit.AddListener(delegate { SetIndValue(id, min, max); });
    }

    // Configure inputfields after finishing typing
    private void SetIndValue(int id, int min, int max)
    {
        float value = float.Parse(inputs[id].text);

        if (value < min)
            value = min;
        else if (value > max)
            value = max;

        inputs[id].text = value.ToString();

        switch (id)
        {
            case 0:
                ph.v0 = value;
                break;
            case 1:
                ph.d0 = value;
                UpdateCarDist();
                break;
            case 2:
                ph.L = value;
                UpdateIntersection();
                break;
            case 3:
                ph.Td = value;
                break;
            case 4:
                ph.aa = value;
                break;
            case 5:
                ph.ad = value;
                break;
        }
    }

    // Update Intersection Info
    private void UpdateIntersection()
    {
        cityObj.localScale = new Vector3(ph.L, 1, ph.L);
        indicatorObj.localScale = new Vector3(1, 1, ph.L);

        indicatorObj.GetChild(0).transform.position = new Vector3((ph.L / 2) - carWidth, -0.45f, 0);
        indicatorObj.GetChild(1).transform.position = new Vector3((-ph.L / 2) + carWidth, -0.45f, 0);

        UpdateCarDist();
    }

    // Update the car distance from the intersection
    private void UpdateCarDist()
    {
        car1.transform.position = new Vector3((ph.L / 2) + ph.d0, 0.5f, ph.L * 2.8f / 7);
        car2.transform.position = new Vector3((ph.L / 2) + ph.d0, 0.5f, ph.L * 0.9f / 7);

        float camYPos = 25 + ph.L / 3 + ph.d0 / 1.5f;
        cam.position = new Vector3(6, camYPos, 20);
    }

    // Tells about what can be done in the current situation
    private void GiveFeedback()
    {
        float sa = ph.v0 * ph.Td + ph.aa * Mathf.Pow(ph.Td, 2) / 2;
        float sd = ph.v0 * ph.Td + ph.ad * Mathf.Pow(ph.Td, 2) / 2;

        feedbackText.text = "";

        if (sa >= ph.L + ph.d0)
        {
            feedbackText.text += "Accelerate ! ";
        }

        if (sd <= ph.d0)
        {
            feedbackText.text += "Decelerate !";
        }

        if (feedbackText.text == "")
        {
            if (ph.L + ph.d0 - sa < sd - ph.d0)
            {
                feedbackText.text = "It's better to accelerate as the remaining distance untill the end of " +
                    "intersection is smaller than the distance from the start of intersection";
            }
            else
            {
                feedbackText.text = "It's better to decelerate as the distance from the start of " +
                    "intersection is smaller than the remaining distance untill the end of intersection";
            }
        }   
    }

    private void Animate()
    {
        if (playing == true)
        {
            if (time <= ph.Td)
            {
                time += 1 * Time.deltaTime;

                float initPos = (ph.L / 2) + ph.d0;

                float sa = ph.v0 * time + ph.aa * Mathf.Pow(time, 2) / 2;
                car1.position = new Vector3(initPos - sa, 0.5f, car1.position.z);
 
                float sd = ph.v0 * time + ph.ad * Mathf.Pow(time, 2) / 2;
                car2.position = new Vector3(initPos - sd, 0.5f, car2.position.z);
            }
        }
    }

    // Start or Reset animation
    public void RestartApp()
    {
        if (playing == false)
        {
            // Start Animation
            GiveFeedback();
            blocker.SetActive(true);

            playing = true;
        }
        else
        {
            // Stop animation and return to edit mode
            UpdateCarDist();
            blocker.SetActive(false);
            feedbackText.text = "The best decision is to ...";
            time = 0;

            playing = false;
        }
    }

    // Exit the application
    public void ExitApp()
    {
        Application.Quit();
    }
}

public class PhysInfo
{
    public float v0 = 20;
    public float d0 = 7;
    public float L = 7;
    public float Td = 2;
    public float aa = 1;
    public float ad = 1;

    public PhysInfo(float v0, float d0, float L, float Td, float aa, float ad)
    {
        this.v0 = v0;
        this.d0 = d0;
        this.L = L;
        this.Td = Td;
        this.aa = aa;
        this.ad = ad;
    }
}