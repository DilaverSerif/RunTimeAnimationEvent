using RunTimeAnimationEvent;
using UnityEngine;
using System.Collections;
public class TestCube : MonoBehaviour
{
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private IEnumerator Start()
    {
        _animator.AddAnimationEvent("Move",0.15f,Test);
        _animator.AddAnimationEvent("MoveTest",1f,TestTwo,ClipSearchType.ByStateName);
        _animator.AddAnimationEventNormalizedTime("Move",0.5f,TestThree);
        _animator.AddAnimationEvent("Move",0.3f,TestStatic);
        yield return new WaitForSeconds(3f);
        _animator.RemoveAnimationEvent(Test,0.15f);
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

    private static void TestStatic()
    {
        Debug.Log("TestFour");
    }
}
