using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ShipStationManager : MonoBehaviour {
    public static UnityEvent OnStationMoveIn;
    public static UnityEvent OnStationMoveOut;
    
    [SerializeField] private float moveOutDuration;   // 5f
    [SerializeField] private float moveInDuration;    // 5f

    private GameObject _shipStationObject;
    private Vector3 _startPosition;
    private Vector3 _inPosition;
    private Vector3 _outPosition;


    private void Init() {
        this._shipStationObject = this.gameObject;
        
        this._startPosition = new Vector3(0, 0, 0);
        this._inPosition = new Vector3(0, 30, 0);
        this._outPosition = new Vector3(0, -30, 0);
        
        //OnStationMoveIn.AddListener(MoveIn);
        //OnStationMoveOut.AddListener(MoveOut);
    }
    
    private void Awake() {
        Init();
    }

    private void Start() {
        if (this._shipStationObject == null) {  // NULL Check
            return;
        }

        SetPosition(this._startPosition);
    }
    
    private void MoveOut() {
        SetPosition(this._outPosition);
        StartCoroutine(
            MoveToPosition( // Ease-Out Cubic OFF
                this._shipStationObject.transform.position, this._outPosition, this.moveOutDuration, false)); 
    }
    
    private void MoveIn() {
        SetPosition(this._inPosition);
        StartCoroutine(
            MoveToPosition( // Ease-out Cubic ON
                this._shipStationObject.transform.position, this._startPosition, this.moveInDuration, true));
    }
    
    private void SetPosition(Vector3 targetPosition) {
        this._shipStationObject.transform.position = targetPosition;
    }

    private IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration, bool decelerate) {
        var elapse = 0f;
        
        while (elapse < duration) {
            elapse += Time.deltaTime;
            var normalizedTime = (elapse / duration);   // 0 ~ 1
            
            if (decelerate) {
                normalizedTime = 1 - Mathf.Pow(1 - normalizedTime, 3); // Ease-out cubic
            }

            this._shipStationObject.transform.position = Vector3.Lerp(from, to, normalizedTime); // Pos. Interpolation
            yield return null;
        }

        this._shipStationObject.transform.position = to;
    }
}