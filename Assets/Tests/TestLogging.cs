using System;
using System.Reflection;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Assets.Tests.Utils;
using System.Threading;

public class TestLogging {
	private Type loggingTabType;
	private Type loggerType;

	[SetUp]
	public void Setup() {
		SceneUtils.Load();

		var domain = TypeUtils.GetAssemblyCSharp();
		
		loggingTabType = domain.GetType("HardCoded.VRigUnity.GUITabLogger");
		loggerType = domain.GetType("HardCoded.VRigUnity.Logger");
	}

	[TearDown]
	public void Teardown() {
	}
	
	[UnityTest, Order(1)]
	public IEnumerator TestLoggerWindow() {
		yield return null;

		object loggingTab = UnityEngine.Object.FindObjectOfType(loggingTabType, true);
		
		PropertyInfo totalLogs = loggingTabType.GetProperty("TotalLogs");

		int previousCount = (int) totalLogs.GetValue(loggingTab);
		loggerType.GetMethod("Log", new[] { typeof(string) }).Invoke(null, new[] { "Test case logging" });
		int newCount = (int) totalLogs.GetValue(loggingTab);

		Assert.AreNotEqual(previousCount, newCount, "Logging did not add a new message");

		yield return null;
	}

	[UnityTest, Order(2)]
	public IEnumerator TestLoggerWindowThreaded() {
		yield return null;

		object loggingTab = UnityEngine.Object.FindObjectOfType(loggingTabType, true);
		
		PropertyInfo totalLogs = loggingTabType.GetProperty("TotalLogs");

		int previousCount = (int) totalLogs.GetValue(loggingTab);

		Thread thread = new(() => {
			loggerType.GetMethod("Log", new[] { typeof(string) }).Invoke(null, new[] { "Test case logging in a thread" });
		});

		// Start the thread and wait for execution
		thread.Start();
		thread.Join();

		int newCount = (int) totalLogs.GetValue(loggingTab);
		Assert.AreEqual(previousCount, newCount, "Threaded messages are added next cycle");

		yield return null;
		
		newCount = (int) totalLogs.GetValue(loggingTab);
		Assert.AreNotEqual(previousCount, newCount, "Logging did not add a new message from a thread");

		yield return null;
	}
}
