using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputParser : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private int moveBufferSize;
    [SerializeField] private int inputBufferSize;
    [SerializeField] private int doubleTapRange;
    [SerializeField] private int fullCharge;
    [SerializeField] private int chargeDecayThreshold;
    Vector2Int[] directionBuffer;
    int backCharge, downCharge, downDecay, backDecay;
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

        // update pointers for new frame
        directionPointer = (directionPointer + 1) % moveBufferSize;
        inputPointer = (inputPointer + 1) % inputBufferSize;
            
        // add direction for this frame
        directionBuffer[directionPointer] = new Vector2Int(Mathf.RoundToInt(input.dir.x), Mathf.RoundToInt(input.dir.y));

        // generate button inputs for this frame
        List<Input> frameInputs = new List<Input>();

        ReadButton(input.light1, Button.L1, ref frameInputs);
        ReadButton(input.light2, Button.L2, ref frameInputs);
        ReadButton(input.heavy1, Button.H1, ref frameInputs);
        ReadButton(input.heavy2, Button.H2, ref frameInputs);
        ReadButton(input.meter, Button.R, ref frameInputs);
        ReadButton(input.dash, Button.D, ref frameInputs);

        // insert inputs or null if no inputs occurred
        if (frameInputs.Count > 0)
            inputBuffer[inputPointer] = frameInputs;
        else 
            inputBuffer[inputPointer] = null;
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

    public bool DashMacro() {
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

    public void BindInputHandler(InputHandler input) {
        this.input = input;
    }

    public bool DashStart(int facing) {
        if (DashMacro() && !Backward(facing) && !Crouch())
            return true;

        int frames = 0;
        bool searching = false;
        for (int i = 0; i < moveBufferSize; i++) {
            int j = (directionPointer - i + moveBufferSize) % moveBufferSize;

            if (searching) {
                if (frames != 0) {
                    if (directionBuffer[j].x == facing) {
                        Debug.Log("Double tap spotted");
                        return true;
                    }



                    frames--;
                    if (frames == 0)
                        return false;
                } else {
                    if (directionBuffer[j].x == 0)
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

    public Vector2Int Vector() {
        return directionBuffer[directionPointer];
    }

    public bool VectorChanged() {
        return directionBuffer[directionPointer] != directionBuffer[(directionPointer - 1 + moveBufferSize) % moveBufferSize];
    }

    public List<Motion> GetMotions(int facing) {

        List<Motion> motions = new();
        motions.Add(Motion.M5);
        if (Crouch())
            motions.Insert(0, Motion.M2);
        if (Forward(facing))
            motions.Insert(0, Motion.M6);
        if (QCF(facing))
            motions.Insert(0, Motion.M236);
        if (QCB(facing))
            motions.Insert(0, Motion.M214);
        if (BFCharge(facing))
            motions.Insert(0, Motion.M46);
        if (DUCharge(facing))
            motions.Insert(0, Motion.M28);
        if (DQCF(facing))
            motions.Insert(0, Motion.M236236);
        if (DQCB(facing))
            motions.Insert(0, Motion.M214214);

        
        return motions;
    }

    private bool QCF(int facing) {
        int stage = 0;
        for (int i = 0; i < moveBufferSize; i++) {
            Vector2Int dir = directionBuffer[(directionPointer - i + moveBufferSize) % moveBufferSize];

            if (stage == 0 && dir.x == facing && dir.y != -1)
                stage++;
            else if (stage == 1 && dir.x == facing && dir.y == -1)
                stage++;
            else if (stage == 2 && dir.x == 0 && dir.y == -1)
                stage++;

            if (stage == 3)
                return true;
        }
        return false;
    }

    private bool QCB(int facing) {
        int stage = 0;
        for (int i = 0; i < moveBufferSize; i++) {
            Vector2Int dir = directionBuffer[(directionPointer - i + moveBufferSize) % moveBufferSize];

            if (stage == 0 && dir.x == -facing && dir.y != -1)
                stage++;
            else if (stage == 1 && dir.x == -facing && dir.y == -1)
                stage++;
            else if (stage == 2 && dir.x == 0 && dir.y == -1)
                stage++;

            if (stage == 3)
                return true;
        }
        return false;
    }

    private bool BFCharge(int facing) {
        return Forward(facing) && backCharge >= fullCharge;
    }

    private bool DUCharge(int facing) {
        return Jump() && downCharge >= fullCharge;
    }

    private bool DQCF(int facing) {
        int stage = 0;
        for (int i = 0; i < moveBufferSize; i++) {
            Vector2Int dir = directionBuffer[(directionPointer - i + moveBufferSize) % moveBufferSize];

            if (stage == 0 && dir.x == facing && dir.y != -1)
                stage++;
            else if (stage == 1 && dir.x == facing && dir.y == -1)
                stage++;
            else if (stage == 2 && dir.x == 0 && dir.y == -1)
                stage++;
            else if (stage == 3 && dir.x == facing && dir.y != -1)
                stage++;
            else if (stage == 4 && dir.x == facing && dir.y == -1)
                stage++;
            else if (stage == 5 && dir.x == 0 && dir.y == -1)
                stage++;

            if (stage == 6)
                return true;
        }
        return false;
    }

    private bool DQCB(int facing) {
        int stage = 0;
        for (int i = 0; i < moveBufferSize; i++) {
            Vector2Int dir = directionBuffer[(directionPointer - i + moveBufferSize) % moveBufferSize];

            if (stage == 0 && dir.x == -facing && dir.y != -1)
                stage++;
            else if (stage == 1 && dir.x == -facing && dir.y == -1)
                stage++;
            else if (stage == 2 && dir.x == 0 && dir.y == -1)
                stage++;
            else if (stage == 3 && dir.x == -facing && dir.y != -1)
                stage++;
            else if (stage == 4 && dir.x == -facing && dir.y == -1)
                stage++;
            else if (stage == 5 && dir.x == 0 && dir.y == -1)
                stage++;

            if (stage == 6)
                return true;
        }
        return false;
    }

    public bool ButtonPressed() {
        if (inputBuffer[inputPointer] != null) {
            foreach (var input in inputBuffer[inputPointer])
                if (input.state == ButtonState.PRESSED)
                    return true;
        }
        return false;
    }

    public List<Button> GetButtonThisFrame() {
        List<Button> buttons = new();

        if (inputBuffer[inputPointer] != null) {
            foreach (var input in inputBuffer[inputPointer])
                if (input.state == ButtonState.PRESSED)
                    buttons.Add(input.button);
        }

        return buttons;
    }

    public void CheckCharge(int facing) {
        if (backDecay > 0)
            backDecay--;
        else
            backCharge = 0;

        if (Backward(facing)) {
            if (directionBuffer[(directionPointer - 1 + moveBufferSize) % moveBufferSize].x != -facing)
                backCharge = 0;
            backCharge++;
            backDecay = chargeDecayThreshold;
        }



        if (downDecay > 0)
            downDecay--;
        else
            downCharge = 0;

        if (Crouch()) {
            if (directionBuffer[(directionPointer - 1 + moveBufferSize) % moveBufferSize].y != -1)
                downCharge = 0;
            downCharge++;
            downDecay = chargeDecayThreshold;
        }
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
        L1, L2, L, H1, H2, H, R, LH, LL, HH, RLL, RHH
    }
}
