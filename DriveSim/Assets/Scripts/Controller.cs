using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Transform cityObj; // Reference to the roads
    public Transform panelObj; // Reference to the UI
    public Transform indicatorObj; // Reference to the intersection lines (Objects indicating legal position in intersection
    public Text feedbackText; // Reference to the text containing the feedback
    public Transform cam; // Reference to the camera
    public GameObject blocker; // Reference to the UI blocker that disables UI interaction during animation

    [Space(10)]

    public Transform car1; // Reference to the accelerating car
    public Transform car2; // Reference to the decelerating car
    public Transform carDef; // Reference to the car with constant speed

    private InputField[] inputs = new InputField[6]; // Contains all the input fields

    private float carWidth = 1.2f; // Car width for plaing "indicatorObj" in the correct place
    private bool playing = false; // True if the animation is playing
    private bool accelerate = true; // True if acceleration option is selected
    private PhysInfo ph; // Contains all the physics information of an object

    private float time = 0; // Timer indicating current time (starts whenever animation begins)
    private bool colChanged = false;

    private void Awake()
    {
        ph = new PhysInfo(KmphToMps(20), 7, 7, 2, 1, -3); // Give minimum values to the physics object

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
        float value = float.Parse(inputs[id].text, CultureInfo.InvariantCulture); // Get the input value from inputfield

        // Check value range and correct if wrong
        if (value < min)
            value = min;
        else if (value > max)
            value = max;

        inputs[id].text = value.ToString();

        // Switch between the UI elements
        switch (id)
        {
            case 0:
                ph.v0 = KmphToMps(value);
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
        // Update the road position and scale
        cityObj.localScale = new Vector3(ph.L, 1, ph.L);
        indicatorObj.localScale = new Vector3(1, 1, ph.L);

        // Update the black lines in the intersection, their distance from the intersection is the cars width / 2
        indicatorObj.GetChild(0).transform.position = new Vector3((ph.L / 2) - carWidth, -0.45f, 0);
        indicatorObj.GetChild(1).transform.position = new Vector3((-ph.L / 2) + carWidth, -0.45f, 0);

        // Update Car Info UI in the scene depending on intersection size and initial distance from it
        if (ph.L + ph.d0 > 40)
        {
            car1.GetChild(0).GetComponent<TextMesh>().characterSize = 4;
            car2.GetChild(0).GetComponent<TextMesh>().characterSize = 4;
            carDef.GetChild(0).GetComponent<TextMesh>().characterSize = 4;
        }
        else if (ph.L + ph.d0 > 70)
        {
            car1.GetChild(0).GetComponent<TextMesh>().characterSize = 5;
            car2.GetChild(0).GetComponent<TextMesh>().characterSize = 5;
            carDef.GetChild(0).GetComponent<TextMesh>().characterSize = 5;
        }

        UpdateCarDist();
    }

    // Update the car distance from the intersection
    private void UpdateCarDist()
    {
        // Update car positions based on the intersection size and distance from it
        car1.transform.position = new Vector3((ph.L / 2) + ph.d0, 0.5f, ph.L * 2.8f / 7);
        car2.transform.position = new Vector3((ph.L / 2) + ph.d0, 0.5f, ph.L * 2.8f / 7);
        carDef.transform.position = new Vector3((ph.L / 2) + ph.d0, 0.5f, ph.L * 0.9f / 7);

        // Update camera information based on the intersection size and distance from it
        float camYPos = 25 + ph.L / 3 + ph.d0 / 1.5f;
        cam.position = new Vector3(6, camYPos, 20);
    }


    private float KmphToMps(float value)
    {
        return value * 1000 / 3600;
    }
    // Tells about what can be done in the current situation
    private void GiveFeedback()
    {
        // sa - final position if accelerate, sd - final position if decelerate
        float sa = ph.v0 * ph.Td + ph.aa * Mathf.Pow(ph.Td, 2) / 2;
        float sd = ph.v0 * ph.Td + ph.ad * Mathf.Pow(ph.Td, 2) / 2;

        feedbackText.text = "";

        float D = 4 * Mathf.Pow(ph.v0, 2) + 8 * ph.ad * ph.d0;
        float t = 0;

        if (D > 0)
            t = (-2 * ph.v0 + Mathf.Sqrt(D)) / (2 * ph.ad); // Time to reach the intersection

        // Update text information based on the best decision
        if (sa >= ph.L + ph.d0)
        {
            accelerate = true;
            feedbackText.text = "Accelerate !";
        } // The speed must be 0 whenever the car approaches the intersection
        else if (sd <= ph.d0 && (D < 0 || ph.v0 + ph.ad * t <= 0))
        {
            accelerate = false;
            feedbackText.text = "Decelerate !";
        }
        else
        {
            accelerate = true;
            feedbackText.text = "Accelerate, Ooops";
        }

        /*
        if (feedbackText.text == "")
        {
            if (ph.L + ph.d0 - sa < sd - ph.d0)
            {
                accelerate = true;
                feedbackText.text = "It's better to accelerate as the remaining distance untill the end of " +
                    "intersection is smaller than the distance from the start of intersection";
            }
            else
            {
                accelerate = false;
                feedbackText.text = "It's better to decelerate as the distance from the start of " +
                    "intersection is smaller than the remaining distance untill the end of intersection";
            }
        }   
        */

        // Activate the car that is best fit for the situation
        if (accelerate == true)
        {
            car1.gameObject.SetActive(true);
        }
        else
        {
            car2.gameObject.SetActive(true);
        }
    }

    // Animate the cars while the yellow light is active
    private void Animate()
    {
        if (playing == true)
        {
            if (time <= ph.Td + (ph.v0 / 5))
            {
                if (time >= ph.Td && colChanged == false)
                {
                    feedbackText.transform.parent.GetComponent<Image>().color = new Color32(178, 34, 34, 255);
                    feedbackText.color = new Color32(255, 255, 255, 255);
                    feedbackText.transform.parent.GetChild(0).GetComponent<Text>().color = new Color32(255, 255, 255, 255);
                    colChanged = true;
                }

                time += 1 * Time.deltaTime;

                float initPos = (ph.L / 2) + ph.d0;

                if (accelerate == true)
                {
                    float sa = ph.v0 * time + ph.aa * Mathf.Pow(time, 2) / 2;
                    car1.position = new Vector3(initPos - sa, 0.5f, car1.position.z);
                }
                else
                {
                    float sd = ph.v0 * time + ph.ad * Mathf.Pow(time, 2) / 2;

                    if (ph.v0 + ph.ad * time > 0)
                    {
                        car2.position = new Vector3(initPos - sd, 0.5f, car2.position.z);
                    }
                }

                float defs = ph.v0 * time;
                carDef.position = new Vector3(initPos - defs, 0.5f, carDef.position.z);
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
            car1.gameObject.SetActive(false);
            car2.gameObject.SetActive(false);
            UpdateCarDist();
            blocker.SetActive(false);
            feedbackText.text = "The best decision is to ...";
            feedbackText.transform.parent.GetComponent<Image>().color = new Color32(250, 210, 1, 255);
            feedbackText.color = new Color32(50, 50, 50, 255);
            feedbackText.transform.parent.GetChild(0).GetComponent<Text>().color = new Color32(50, 50, 50, 255);
            time = 0;

            colChanged = false;
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
    public float v0;
    public float d0;
    public float L;
    public float Td;
    public float aa;
    public float ad;

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