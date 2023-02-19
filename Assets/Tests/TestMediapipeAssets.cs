using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Tests.Utils;

public class TestMediapipeAssets {
	private Type holisticGraphType;
	private Type emptySourceType;
	private Type waitForResultType;

	[SetUp]
	public void Setup() {
		SceneUtils.Load();
		
		var domain = TypeUtils.GetAssemblyCSharp();
			
		holisticGraphType = domain.GetType("HardCoded.VRigUnity.HolisticGraph");
		emptySourceType = domain.GetType("HardCoded.VRigUnity.EmptySource");
		waitForResultType = domain.GetType("Mediapipe.Unity.WaitForResult");
	}

	[TearDown]
	public void Teardown() {
	}
	
	[UnityTest, Order(1)]
	public IEnumerator TestGraphLoading() {
		yield return null;

		object holisticTrackingGraph = UnityEngine.Object.FindObjectOfType(holisticGraphType);

		// Try to initialize the graph
		IEnumerator waitForResult = holisticGraphType.GetMethod("WaitForInitAsync").Invoke(holisticTrackingGraph, new object[] {}) as IEnumerator;
		yield return waitForResult;

		// Validate that it does not give errors
		bool isError = (bool) waitForResultType.GetProperty("isError").GetValue(waitForResult);
		Exception error = waitForResultType.GetProperty("error").GetValue(waitForResult) as Exception;

		Assert.IsFalse(isError, "WaitForInitAsync should not contain errors");
		if (isError) {
			Debug.LogException(error);
		}

		// Try start the graph
		GameObject testObject = new("TestObject");
		object emptySource = testObject.AddComponent(emptySourceType);
		holisticGraphType.GetMethod("StartRun").Invoke(holisticTrackingGraph, new object[] { emptySource });
		
		// Wait a single frame
		yield return null;

		object calculatorGraph = holisticGraphType
			.GetProperty("CalculatorGraph", BindingFlags.NonPublic | BindingFlags.Instance)
			.GetValue(holisticTrackingGraph);
		
		bool hasCalculatorGraphError = (bool) calculatorGraph.GetType().GetMethod("HasError").Invoke(calculatorGraph, new object[] {});
		if (hasCalculatorGraphError) {
			Debug.LogWarning(calculatorGraph.GetType().GetMethod("WaitUntilDone").Invoke(calculatorGraph, new object[] {}));
		}

		Assert.IsFalse(hasCalculatorGraphError, "CalculatorGraph should not have any errors");
		
		yield return null;
		
		holisticGraphType.GetMethod("Stop").Invoke(holisticTrackingGraph, new object[] {});

		yield return null;
	}

	[UnityTest, Order(2)]
	public IEnumerator TestGraphLoadingRepeat() {
		yield return TestGraphLoading();
	}
}
