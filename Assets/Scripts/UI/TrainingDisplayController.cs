using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingDisplayController : MonoBehaviour
{
    [SerializeField] private GameObject inputLayoutPrefab;
    [SerializeField] private GameObject motionPrefab;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform inputListParent;
    [SerializeField] private InputParser inputs;
    [SerializeField] private int inputLimit;

    private List<GameObject> activeInputs = new();
    int inputIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < inputLimit; i++) {
            activeInputs.Add(Instantiate(inputLayoutPrefab, inputListParent));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool updateIndex = false;
        if (inputs.VectorChanged() && inputs.Vector() != Vector2Int.zero) {
            activeInputs[inputIndex].SetActive(true);
            activeInputs[inputIndex].transform.SetSiblingIndex(0);
            Transform layout = activeInputs[inputIndex].transform.Find("Layout");
            
            foreach (Transform child in layout) {
                Destroy(child.gameObject);
            }

            GameObject motion = Instantiate(motionPrefab, layout);
            motion.SetActive(true);
            motion.transform.rotation = Quaternion.FromToRotation(Vector3.right, ((Vector3Int)inputs.Vector()));
            
            updateIndex = true;
        }

        if (inputs.ButtonPressed()) {

            Transform layout = activeInputs[inputIndex].transform.Find("Layout");
            if (!updateIndex) {
                activeInputs[inputIndex].SetActive(true);
                activeInputs[inputIndex].transform.SetSiblingIndex(0);
                
                foreach (Transform child in layout) {
                    Destroy(child.gameObject);
                }
            }

            foreach (var pressed in inputs.GetButtonThisFrame()) {
                GameObject button = Instantiate(buttonPrefab, layout);
                Transform bg = button.transform.Find("Background");
                bg.GetComponent<Image>().color = InputToColor(pressed);
                bg.Find("Text").GetComponent<TextMeshProUGUI>().text = pressed.ToString();
                button.SetActive(true);
            }
            
            updateIndex = true;
        }


        if (updateIndex) {
            inputIndex = (inputIndex + 1) % inputLimit;
        }
    }

    private Color InputToColor(InputParser.Button button) {
        switch (button) {
            case InputParser.Button.L1:
                return Color.yellow;
            case InputParser.Button.L2:
                return Color.blue;
            case InputParser.Button.H1:
                return Color.red;
            case InputParser.Button.H2:
                return new Color(0, 0.8f, 0, 1); //green
            case InputParser.Button.R:
                return new (0.1f, 0, 0.3f, 1); //purple
            case InputParser.Button.D:
                return new Color(0.7f, 0.7f, 0, 1); //orange

            default:
                return Color.gray;
        }
    }

    public void TriggerMoveSuccess(InputParser.Motion motion, InputParser.Button button) {

    }
}
