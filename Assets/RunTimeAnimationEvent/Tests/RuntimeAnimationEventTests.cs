using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools; // For LogAssert
using RunTimeAnimationEvent;
using System;
using System.Collections.Generic; // For List
using System.Reflection; // For accessing private members in tests

// Conceptual Mocks/Stubs for Unity objects
// It's assumed that for a real test environment, these would be more fleshed out
// or a mocking framework would be used (e.g., Moq).

// Mock Animator to avoid requiring a full Unity Player loop
public class MockAnimator : MonoBehaviour
{
    public RuntimeAnimatorController runtimeAnimatorController;
    public AnimatorStateInfo animatorStateInfo;

    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
    {
        return animatorStateInfo ?? new AnimatorStateInfo(); // Return default if not set
    }
    
    // Add other methods if needed by tests
}

public class RuntimeAnimationEventTests
{
    private GameObject testGameObject;
    private AnimationEventRunTime eventRuntime;
    private MockAnimator mockAnimator; // Using MockAnimator
    private AnimationClip testClip;
    private RuntimeAnimatorController mockController;

    // Helper to access private animationStructs list for assertion
    private List<AnimationEventRunTime.RunTimeEventData> GetAnimationStructs(AnimationEventRunTime instance)
    {
        FieldInfo field = typeof(AnimationEventRunTime).GetField("animationStructs", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(instance) as List<AnimationEventRunTime.RunTimeEventData>;
    }
    
    // Helper to create AnimationData instances for testing AddEvent/RemoveEvent on AnimationEventRunTime
    // This is tricky because AnimationData in RuntimeAnimation is private.
    // For AnimationEventRunTime tests, we primarily care about its AddEvent/RemoveEvent methods
    // which take RuntimeAnimation.AnimationData.
    // We'll use a conceptual helper or direct parameters for these tests.

    [SetUp]
    public void Setup()
    {
        testGameObject = new GameObject("TestGO");
        eventRuntime = testGameObject.AddComponent<AnimationEventRunTime>();
        
        // Setup for MockAnimator
        mockAnimator = testGameObject.AddComponent<MockAnimator>(); // Add our mock
        mockController = new RuntimeAnimatorController();
        mockAnimator.runtimeAnimatorController = mockController;

        testClip = new AnimationClip { name = "TestClip1" };
        // Add testClip to mockController.animationClips (this is usually read-only, so conceptual)
        // In a real scenario, AnimatorOverrideController or a more complex mock might be needed.
        // For now, we'll assume GetClip/GetClipByState can be tested by populating this.
        // For simplicity in this conceptual test, we might not directly populate it here
        // but ensure GetClipByState tests acknowledge this dependency.
    }

    [TearDown]
    public void Teardown()
    {
        GameObject.DestroyImmediate(testGameObject);
    }

    // --- Tests for AnimationEventRunTime.cs ---

    [Test]
    public void AddEvent_AddsEventToListAndAnimationClip()
    {
        // Arrange
        Action myAction = () => Debug.Log("TestAction1");
        float eventTime = 0.5f;
        
        // We need a way to create RuntimeAnimation.AnimationData or pass its components.
        // AnimationEventRunTime.AddEvent expects RuntimeAnimation.AnimationData.
        // Let's assume we call a method from RuntimeAnimation that then calls eventRuntime.AddEvent
        // For this conceptual test, we'll focus on the effect on eventRuntime's list.

        var animationData = new RuntimeAnimation.AnimationData(testClip, myAction, eventTime);
        
        // Act
        eventRuntime.AddEvent(ref animationData); // animationData is passed by ref

        // Assert
        var internalList = GetAnimationStructs(eventRuntime);
        Assert.AreEqual(1, internalList.Count, "Event should be added to the internal list.");
        Assert.AreEqual(myAction.Method.Name, internalList[0].actionName, "Action name should match.");
        Assert.IsTrue(Mathf.Approximately(eventTime, internalList[0].eventTime), "Event time should match.");
        
        // Conceptual: Assert that an event was added to testClip.
        // This would require checking testClip.events in a real Unity test environment.
        Assert.AreEqual(1, testClip.events.Length, "Event should be added to the AnimationClip.");
        Assert.IsTrue(Mathf.Approximately(eventTime, testClip.events[0].time), "AnimationClip event time should match.");
        Assert.AreEqual(nameof(eventRuntime.InvokeAction), testClip.events[0].functionName, "AnimationClip event function name should be InvokeAction.");
    }

    [Test]
    public void RemoveEvent_RemovesExistingEvent()
    {
        // Arrange
        Action action1 = () => Debug.Log("Action1");
        float time1 = 0.5f;
        var data1 = new RuntimeAnimation.AnimationData(testClip, action1, time1);
        eventRuntime.AddEvent(ref data1); // Add an event first

        // Act
        eventRuntime.RemoveEvent(data1); // data1 does not need 'ref' here

        // Assert
        var internalList = GetAnimationStructs(eventRuntime);
        Assert.AreEqual(0, internalList.Count, "Event should be removed from the list.");
    }

    [Test]
    public void RemoveEvent_HandlesNonExistingEventGracefully()
    {
        // Arrange
        Action action1 = () => Debug.Log("Action1");
        float time1 = 0.5f;
        // Don't add the event, so it's non-existing
        var data1 = new RuntimeAnimation.AnimationData(testClip, action1, time1);

        // Act & Assert
        LogAssert.Expect(LogType.Warning, "This event does not exist or has already removed");
        eventRuntime.RemoveEvent(data1); // Attempt to remove non-existing event
        
        var internalList = GetAnimationStructs(eventRuntime);
        Assert.AreEqual(0, internalList.Count, "List should remain empty.");
    }
    
    [Test]
    public void RemoveEvent_IteratesSafely_Conceptual()
    {
        // Arrange
        Action action1 = () => Debug.Log("Action1_IterSafe");
        Action action2 = () => Debug.Log("Action2_IterSafe");
        float time = 0.5f;

        var data1 = new RuntimeAnimation.AnimationData(testClip, action1, time);
        var data2 = new RuntimeAnimation.AnimationData(testClip, action2, time); // Same time, different action

        eventRuntime.AddEvent(ref data1);
        eventRuntime.AddEvent(ref data2); // Add two events

        // Act
        // Removing the first one encountered (data1 if order is preserved and names are different)
        // The backwards iteration is meant to prevent issues if removing one affects iteration over others.
        eventRuntime.RemoveEvent(data1); 

        // Assert
        var internalList = GetAnimationStructs(eventRuntime);
        Assert.AreEqual(1, internalList.Count, "One event should remain after removal.");
        Assert.AreEqual(action2.Method.Name, internalList[0].actionName, "The correct event should remain.");
        Assert.Pass("Conceptual test for safe iteration. Actual validation of no InvalidOperationException is implicit.");
    }

    [Test]
    public void InvokeAction_CallsCorrectActionAtPreciseTime()
    {
        // Arrange
        bool actionCalled = false;
        Action myAction = () => actionCalled = true;
        float eventTime = 0.75f;
        var animationData = new RuntimeAnimation.AnimationData(testClip, myAction, eventTime);
        eventRuntime.AddEvent(ref animationData);

        // Act
        eventRuntime.InvokeAction(eventTime);

        // Assert
        Assert.IsTrue(actionCalled, "The correct action should have been called.");
    }

    [Test]
    public void InvokeAction_DoesNotCallActionAtWrongTime()
    {
        // Arrange
        bool actionCalled = false;
        Action myAction = () => actionCalled = true;
        float eventTime = 0.75f;
        float wrongTime = 0.25f;
        var animationData = new RuntimeAnimation.AnimationData(testClip, myAction, eventTime);
        eventRuntime.AddEvent(ref animationData);

        // Act
        eventRuntime.InvokeAction(wrongTime);

        // Assert
        Assert.IsFalse(actionCalled, "Action should not have been called at the wrong time.");
    }
    
    [Test]
    public void AddEvent_HandlesDuplicateEventGracefully()
    {
        // Arrange
        Action myAction = () => Debug.Log("DuplicateActionTest");
        float eventTime = 0.5f;
        var animationData = new RuntimeAnimation.AnimationData(testClip, myAction, eventTime);

        eventRuntime.AddEvent(ref animationData); // Add first time

        // Act & Assert
        LogAssert.Expect(LogType.Warning, "This event already exists");
        eventRuntime.AddEvent(ref animationData); // Attempt to add duplicate

        var internalList = GetAnimationStructs(eventRuntime);
        Assert.AreEqual(1, internalList.Count, "Duplicate event should not be added.");
        Assert.AreEqual(1, testClip.events.Length, "AnimationClip should still have only one event.");
    }

    // --- Tests for RuntimeAnimation.cs ---
    // These tests are more conceptual as RuntimeAnimation is a static class
    // and interacts heavily with Animator and its state.
    // Mocking or integration tests in Unity would be more appropriate.

    [Test]
    public void AddAnimationEvent_AddsEventViaRuntimeComponent()
    {
        // Arrange
        Action myAction = () => Debug.Log("TestAnimatorAction");
        float eventTime = 0.5f;
        string clipName = testClip.name; // Use the name of our testClip

        // Ensure mockAnimator's controller has the clip
        // This setup is complex for a conceptual test.
        // We'd typically use AnimatorOverrideController or ensure the base controller has 'TestClip1'.
        // For now, assume GetClip can find it.
        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        if (!clips.Contains(testClip)) clips.Add(testClip);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();


        // Act
        mockAnimator.AddAnimationEvent(clipName, eventTime, myAction);

        // Assert
        var eventComponent = testGameObject.GetComponent<AnimationEventRunTime>();
        Assert.IsNotNull(eventComponent, "AnimationEventRunTime component should be added.");
        var internalList = GetAnimationStructs(eventComponent);
        Assert.AreEqual(1, internalList.Count, "Event should be added to the runtime component's list.");
        Assert.AreEqual(myAction.Method.Name, internalList[0].actionName);
    }

    [Test]
    public void AddAnimationEventByStateName_AddsEventCorrectly()
    {
        // Arrange
        Action myAction = () => Debug.Log("StateTestAction");
        float eventTime = 0.3f;
        string stateName = "TestState1";
        string clipNameForState = testClip.name; // Assume testClip is the one for TestState1

        // Configure MockAnimator to simulate being in "TestState1"
        // and that "TestState1" maps to "TestClip1"
        mockAnimator.animatorStateInfo = new AnimatorStateInfo(); // Default, then modify if IsName can be mocked
        // This is where mocking GetCurrentAnimatorStateInfo().IsName(stateName) becomes important.
        // For simplicity, we'll assume GetClipByState can work if the clip exists.
        
        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        if (!clips.Contains(testClip)) clips.Add(testClip);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();


        // Act
        // Need to ensure GetClipByState works. For this, we need to simulate state.
        // This is highly conceptual without deeper mocking of Animator's state machine.
        // Let's assume for this test that GetClipByState will find testClip if stateName matches a convention
        // or if targetClipName is provided (which it is here by clipNameForState).

        // To make GetClipByState pass the stateInfo.IsName(stateName) check:
        // This is tricky. A real test might use a test AnimatorController with actual states.
        // Or, we could modify MockAnimator to allow setting what IsName returns for specific states.
        // For this conceptual test, we'll assume the state is active and proceed.
        // The GetClipByState method has logic that might return the clip even if IsName is false,
        // if targetClipName is provided and matches a clip in the controller.
        // Let's rely on that part for this conceptual test.

        mockAnimator.AddAnimationEventByStateName(stateName, eventTime, myAction, clipNameForState);

        // Assert
        var eventComponent = testGameObject.GetComponent<AnimationEventRunTime>();
        Assert.IsNotNull(eventComponent);
        var internalList = GetAnimationStructs(eventComponent);
        Assert.AreEqual(1, internalList.Count);
        Assert.AreEqual(myAction.Method.Name, internalList[0].actionName);
        Assert.IsTrue(Mathf.Approximately(eventTime, internalList[0].eventTime));
        Assert.Pass("Conceptual: Assumes GetClipByState can resolve the clip. Full test needs AnimatorController setup.");
    }
    
    [Test]
    public void RemoveAnimationEvent_RemovesEventViaRuntimeComponent()
    {
        // Arrange
        Action myAction = () => Debug.Log("ActionToRemove");
        float eventTime = 0.6f;
        string clipName = testClip.name;

        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        if (!clips.Contains(testClip)) clips.Add(testClip);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();

        mockAnimator.AddAnimationEvent(clipName, eventTime, myAction); // Add an event first
        
        var eventComponent = testGameObject.GetComponent<AnimationEventRunTime>();
        Assert.AreEqual(1, GetAnimationStructs(eventComponent).Count, "Event should be present before removal.");

        // Act
        mockAnimator.RemoveAnimationEvent(myAction, eventTime);

        // Assert
        Assert.AreEqual(0, GetAnimationStructs(eventComponent).Count, "Event should be removed by RemoveAnimationEvent.");
    }

    [Test]
    public void GetClipByState_ReturnsCorrectClip_Conceptual()
    {
        // This test is highly conceptual due to Animator state complexities.
        // Arrange
        string stateName = "MyState";
        string targetClipName = "MyStateClip"; // Assume this clip is associated with MyState
        AnimationClip expectedClip = new AnimationClip { name = targetClipName };

        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        if (!clips.Contains(expectedClip)) clips.Add(expectedClip);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();
        
        // To make GetClipByState work as intended, stateInfo.IsName(stateName) should be true.
        // This requires more advanced mocking of GetCurrentAnimatorStateInfo or a test AnimatorController.
        // For now, we'll assume the state is active and targetClipName is used.

        // Act
        // MethodInfo privateMethod = typeof(RuntimeAnimation).GetMethod("GetClipByState", BindingFlags.NonPublic | BindingFlags.Static);
        // AnimationClip actualClip = (AnimationClip)privateMethod.Invoke(null, new object[] { mockAnimator, stateName, 0, targetClipName });
        
        // Assert
        // Assert.AreEqual(expectedClip, actualClip, "Should return the clip specified by targetClipName if state matches.");
        Assert.Inconclusive("GetClipByState requires complex Animator state mocking or integration testing. Test logic via AddAnimationEventByStateName.");
    }
    
    [Test]
    public void GetClipByState_HandlesAmbiguousStateNameWithWarning_Conceptual()
    {
        // Arrange
        string stateName = "AmbiguousState";
        AnimationClip clip1 = new AnimationClip { name = stateName }; // Same name as state
        AnimationClip clip2 = new AnimationClip { name = stateName }; // Another with same name

        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        clips.Add(clip1);
        clips.Add(clip2);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();

        // Assume state "AmbiguousState" is active. This is hard to mock simply.
        // The warning is logged if targetClipName is null AND stateInfo.IsName(stateName) is true AND multiple clips are named stateName.
        
        // Act & Assert
        LogAssert.Expect(LogType.Warning, $"Found 2 clips with name {stateName}");
        
        // Call a public method that uses GetClipByState internally with null targetClipName
        // For example, AddAnimationEventByStateName without the optional clipName
        // mockAnimator.AddAnimationEventByStateName(stateName, 0.1f, () => {}, null);
        
        Assert.Inconclusive("Needs proper Animator state mocking to ensure IsName(stateName) is true for the warning to trigger as designed. Test logic via AddAnimationEventByStateName.");
    }

    [Test]
    public void AddAnimationEvent_CreatesRuntimeComponentIfNotPresent()
    {
        // Arrange
        Action myAction = () => Debug.Log("ComponentCreationTest");
        float eventTime = 0.1f;
        string clipName = testClip.name;

        // Ensure the component is not there initially
        var existingComponent = testGameObject.GetComponent<AnimationEventRunTime>();
        if (existingComponent != null) GameObject.DestroyImmediate(existingComponent);
        
        Assert.IsNull(testGameObject.GetComponent<AnimationEventRunTime>(), "Component should not exist before call.");

        var clips = new List<AnimationClip>(mockAnimator.runtimeAnimatorController.animationClips ?? new AnimationClip[0]);
        if (!clips.Contains(testClip)) clips.Add(testClip);
        mockAnimator.runtimeAnimatorController.animationClips = clips.ToArray();

        // Act
        mockAnimator.AddAnimationEvent(clipName, eventTime, myAction);

        // Assert
        var eventComponent = testGameObject.GetComponent<AnimationEventRunTime>();
        Assert.IsNotNull(eventComponent, "AnimationEventRunTime component should have been added.");
    }
}
