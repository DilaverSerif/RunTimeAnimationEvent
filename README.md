#### What does it do?
It serves the purpose of adding Animation Events to runtime animations in Unity.

##### Example usage:
```csharp
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
```

You can use NormalizedTime when setting the animation time. **(AddAnimationEventNormalizedTime)**

You can use Clip name or state name if you want. For this, you can set *ClipSearchType* according to yourself.
```csharp
public static void AddAnimationEvent(this Animator animator, string clipName, float time, Action action,ClipSearchType clipSearchType = ClipSearchType.ByClipName)
```
You can also see the active events on the inspector.

![](https://i.imgur.com/3Ctwr7s.png)

if you want a remove event you can use the method
```csharp
 _animator.RemoveAnimationEvent(Test,0.15f);
```
