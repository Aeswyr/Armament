using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputParser : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private int moveBufferSize;
    [SerializeField] private int inputBufferSize;
    [SerializeField] private int doubleTapRange;
    Vector2Int[] directionBuffer;
    List<Input>[] inputBuffer;

    private int directionPointer = 0;
    private int inputPointer = 0;

    void Start() {
        directionBuffer = new Vector2Int[moveBufferSize];
        inputBuffer = new List<Input>[inputBufferSize];
    }

    void FixedUpdate() {
        if (input == null)
            return;
            
        directionBuffer[directionPointer] = new Vector2Int(Mathf.RoundToInt(input.dir.x), Mathf.RoundToInt(input.dir.y));
        List<Input> frameInputs = new List<Input>();

        ReadButton(input.light1, Button.L1, ref frameInputs);
        ReadButton(input.light2, Button.L2, ref frameInputs);
        ReadButton(input.heavy1, Button.H1, ref frameInputs);
        ReadButton(input.heavy2, Button.H2, ref frameInputs);
        ReadButton(input.meter, Button.R, ref frameInputs);
        ReadButton(input.dash, Button.D, ref frameInputs);

        if (frameInputs.Count > 0)
            inputBuffer[inputPointer] = frameInputs;
        else 
            inputBuffer[inputPointer] = null;

        directionPointer = (directionPointer + 1) % moveBufferSize;
        inputPointer = (inputPointer + 1) % inputBufferSize;
    }

    private void ReadButton(InputHandler.ButtonState input, Button b, ref List<Input> frameInputs) {
        if (input.pressed) {
            frameInputs.Add(new Input() {button = b, state = ButtonState.PRESSED});
        } else if (input.released)
            frameInputs.Add(new Input() {button = b, state = ButtonState.RELEASED});
        else if (input.down)
            frameInputs.Add(new Input() {button = b, state = ButtonState.DOWN});
    }


    public bool Forward(int facing) {
        return directionBuffer[directionPointer].x == facing;
    }

    public bool Backward(int facing) {
        return directionBuffer[directionPointer].x == -facing;
    }

    public bool Dash() {
        if (inputBuffer[inputPointer] == null)
            return false;
        foreach (var input in inputBuffer[inputPointer]) {
            if (input.button == Button.D && (input.state == ButtonState.PRESSED || input.state == ButtonState.DOWN))
                return true;
        }

        return false;
    }

    public bool GetButtonState(Button button, ButtonState state) {
        foreach (var frame in inputBuffer)
            if (frame != null)
                foreach (var input in frame)
                    if (input.button == button && input.state == state)
                        return true;


        return false;
    }

    public bool DebugL1Pressed() {
        if (input == null)
            return false;
        return input.light1.pressed;
    }

    public bool DebugH1Pressed() {
        if (input == null)
            return false;
        return input.heavy1.pressed;
    }

    public bool Released(Button button) {

        return false;
    }

    public bool Down(Button button) {

        return false;
    }

    public void BindInputHandler(InputHandler input) {
        this.input = input;
    }

    public bool DashStart(int facing) {
        if (Dash() && !Backward(facing) && !Crouch())
            return true;

        int frames = 0;
        bool searching = false;
        for (int i = 0; i < moveBufferSize; i++) {
            int j = (directionPointer + i) % moveBufferSize;

            if (searching) {
                if (frames != 0) {
                    if (directionBuffer[j].x == facing)
                        return true;



                    frames--;
                    if (frames == 0)
                        searching = false;
                } else {
                    if (directionBuffer[j].x != facing)
                        frames = doubleTapRange;
                }
            } else {
                if (directionBuffer[j].x == facing)
                    searching = true;
            }

        }
        
        return false;
    }

    public bool BackDash(int facing) {
        return false;
    }

    public bool Crouch() {
        return directionBuffer[directionPointer].y == -1;
    }

    public bool Jump() {
        return directionBuffer[directionPointer].y == 1;
    }

    public int Direction() {
        return directionBuffer[directionPointer].x;
    }

    public Motion GetMotion() {
        return Motion.M5;
    }


    public enum Button {
        L1, L2, H1, H2, R, D
    }

    public enum ButtonState {
        PRESSED, DOWN, RELEASED
    }

    private struct Input {
        public Button button;
        public ButtonState state;
    }

    public enum Motion {
        M236, M214, M236236, M214214, M46, M28, M6, M2, M5
    }

    public enum Action {
        L1, L2, H1, H2, R, LH, LL, HH, RLL, RHH
    }
}
