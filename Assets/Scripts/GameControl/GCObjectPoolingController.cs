using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCObjectPoolingController : GCSingletonImplementer<GCObjectPoolingController> {
    [SerializeField] private uint initPoolSize;
    [SerializeField] private GCPooledObject objectToPool; // 청사진
    
    private Stack<GCPooledObject> _poolStack; // 오브젝트 풀
    private GCPooledObject _instance;         // 풀에 들어갈 오브젝트
    
    
    private void Init() {
        SetupPool();
    }

    private void Awake() {
        Init();
    }
    
    private void SetupPool() {
        this._poolStack = new();
        this._instance = null;

        for (var i = 0; i < this.initPoolSize; i++) {
            this._instance = Instantiate(this.objectToPool);
            this._instance.PoolingController = this; // 풀 안에 있는 오브젝트에 컨트롤러 참조 연결
            
            this._instance.gameObject.SetActive(false);
            this._poolStack.Push(this._instance);
        }
    }

    public GCPooledObject Get() {
        if (this._poolStack.Count == 0) {   // 풀 고갈
            var newInstance = Instantiate(this.objectToPool);
            newInstance.PoolingController = this;

            return newInstance;
        }
        else {  // 풀 여유
            var gotInstance = this._poolStack.Pop();
            gotInstance.gameObject.SetActive(true);

            return gotInstance;
        }
    }

    public void ReturnToPool(GCPooledObject gcPooledObject) {
        
    }
}
