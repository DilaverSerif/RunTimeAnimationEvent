using RunTimeAnimationEvent;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _animator.AddAnimationEvent("Move",0.15f,Test);
        _animator.AddAnimationEvent("MoveTest",1f,TestTwo,ClipSearchType.ByStateName);
        _animator.AddAnimationEventNormalizedTime("Move",0.5f,TestThree);
    }

    private void Test()
    {
        Debug.Log("Test");
    }
    
    private void TestTwo()
    {
        Debug.Log("TestTwo");
    }
    
    private void TestThree()
    {
        Debug.Log("TestThree");
    }
}
