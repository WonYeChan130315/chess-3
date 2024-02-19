using UnityEngine;

public class RemainingTest : MonoBehaviour {
    public int a;
    public int b;

    void Awake() {
        if (a <= 0 || b <= 0)
            return;

        Debug.Log(a % b);
        Debug.Log(a / b);
    }
}