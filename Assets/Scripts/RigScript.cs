using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class RigScript : MonoBehaviour
{
    public VRMBlendShapeProxy blendShapeProxy;
    public Animator animator;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.GetBoneTransform(HumanBodyBones.Head).Rotate(Vector3.right, 10.5f);
    }
}
