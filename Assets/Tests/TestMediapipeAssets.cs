using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assets.Tests.Utils;
using System.Linq;

public class TestMediapipeAssets {
	private Type holisticTrackingGraphType;
	private Type emptySourceType;
	private Type waitForResultType;

	[SetUp]
	public void Setup() {
		SceneUtils.Load();

		var domain = AppDomain.CurrentDomain.GetAssemblies()
			.Where(item => item.GetName().Name == "Assembly-CSharp")
			.First();
			
		holisticTrackingGraphType = domain.GetType("HardCoded.VRigUnity.HolisticTrackingGraph");
		emptySourceType = domain.GetType("HardCoded.VRigUnity.EmptySource");
		waitForResultType = domain.GetType("Mediapipe.Unity.WaitForResult");
	}

	[TearDown]
	public void Teardown() {
	}
	
	[UnityTest, Order(1)]
	public IEnumerator TestGraphLoading() {
		yield return null;

		object holisticTrackingGraph = UnityEngine.Object.FindObjectOfType(holisticTrackingGraphType);

		// Try to initialize the graph
		Debug.Log("Running WaitForInitAsync");
		IEnumerator waitForResult = holisticTrackingGraphType.GetMethod("WaitForInitAsync").Invoke(holisticTrackingGraph, new object[] {}) as IEnumerator;
		yield return waitForResult;

		// Validate that it does not give errors
		bool isError = (bool) waitForResultType.GetProperty("isError").GetValue(waitForResult);
		Exception error = waitForResultType.GetProperty("error").GetValue(waitForResult) as Exception;

		Assert.IsFalse(isError, "WaitForInitAsync should not contain errors");
		if (isError) {
			Debug.LogException(error);
		}

		// Try start the graph
		Debug.Log("Running StartRun");
		GameObject testObject = new("TestObject");
		object emptySource = testObject.AddComponent(emptySourceType);
		holisticTrackingGraphType.GetMethod("StartRun").Invoke(holisticTrackingGraph, new object[] { emptySource });
		
		// Wait a single frame
		yield return null;

		object calculatorGraph = holisticTrackingGraphType
			.GetProperty("CalculatorGraph", BindingFlags.NonPublic | BindingFlags.Instance)
			.GetValue(holisticTrackingGraph);
		
		bool hasCalculatorGraphError = (bool) calculatorGraph.GetType().GetMethod("HasError").Invoke(calculatorGraph, new object[] {});
		if (hasCalculatorGraphError) {
			Debug.LogWarning(calculatorGraph.GetType().GetMethod("WaitUntilDone").Invoke(calculatorGraph, new object[] {}));
		}

		Assert.IsFalse(hasCalculatorGraphError, "CalculatorGraph should not have any errors");
		
		yield return null;
		
		Debug.Log("Stop Graph");
		holisticTrackingGraphType.GetMethod("Stop").Invoke(holisticTrackingGraph, new object[] {});

		yield return null;
	}

	[UnityTest, Order(2)]
	public IEnumerator TestGraphLoadingRepeat() {
		yield return TestGraphLoading();
	}
}
