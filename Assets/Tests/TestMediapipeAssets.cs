using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Linq;
using System.Threading;

public class TestMediapipeAssets {
	private Type holisticTrackingGraphType;
	private Type emptySourceType;
	private Type waitForResultType;

	[SetUp]
	public void Setup() {
		SceneManager.LoadScene("Workspace", LoadSceneMode.Single);

		var domain = AppDomain.CurrentDomain.GetAssemblies()
			.Where(item => item.GetName().Name == "Assembly-CSharp")
			.First();
			
		holisticTrackingGraphType = domain.GetType("HardCoded.VRigUnity.HolisticTrackingGraph");
		emptySourceType = domain.GetType("HardCoded.VRigUnity.EmptySource");
		waitForResultType = domain.GetType("Mediapipe.Unity.WaitForResult");
	}

	[TearDown]
	public void Teardown() {
		Debug.Log("After Test Graph Loading");
	}
	
	[UnityTest]
	public IEnumerator TestGraphLoading() {
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
	}
}
