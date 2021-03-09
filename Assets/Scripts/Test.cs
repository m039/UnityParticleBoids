using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP4 {

    public class Test : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.LogError("DebugConfig.Test = " + GP4.DebugConfig.Instance.Test);
        }

    }

}
