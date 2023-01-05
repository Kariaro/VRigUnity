using System;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using UnityEngine.TestTools;
using Assets.Tests.Utils;
using System.Threading;

public class TestLogging {
	private Type loggingWindowType;
	private Type loggerType;

	[SetUp]
	public void Setup() {
		SceneUtils.Load();

		var domain = AppDomain.CurrentDomain.GetAssemblies()
			.Where(item => item.GetName().Name == "Assembly-CSharp")
			.First();
		
		loggingWindowType = domain.GetType("HardCoded.VRigUnity.GUILoggerWindow");
		loggerType = domain.GetType("HardCoded.VRigUnity.Logger");
	}

	[TearDown]
	public void Teardown() {
	}
	
	[UnityTest, Order(1)]
	public IEnumerator TestLoggerWindow() {
		yield return null;

		object loggingWindow = UnityEngine.Object.FindObjectOfType(loggingWindowType, true);
		
		PropertyInfo totalLogs = loggingWindowType.GetProperty("TotalLogs");

		int previousCount = (int) totalLogs.GetValue(loggingWindow);
		loggerType.GetMethod("Log", new[] { typeof(string) }).Invoke(null, new[] { "Test case logging" });
		int newCount = (int) totalLogs.GetValue(loggingWindow);

		Assert.AreNotEqual(previousCount, newCount, "Logging did not add a new message");

		yield return null;
	}

	[UnityTest, Order(2)]
	public IEnumerator TestLoggerWindowThreaded() {
		yield return null;

		object loggingWindow = UnityEngine.Object.FindObjectOfType(loggingWindowType, true);
		
		PropertyInfo totalLogs = loggingWindowType.GetProperty("TotalLogs");

		int previousCount = (int) totalLogs.GetValue(loggingWindow);

		Thread thread = new(() => {
			loggerType.GetMethod("Log", new[] { typeof(string) }).Invoke(null, new[] { "Test case logging in a thread" });
		});

		// Start the thread and wait for execution
		thread.Start();
		thread.Join();

		int newCount = (int) totalLogs.GetValue(loggingWindow);
		Assert.AreEqual(previousCount, newCount, "Threaded messages are added next cycle");

		yield return null;
		
		newCount = (int) totalLogs.GetValue(loggingWindow);
		Assert.AreNotEqual(previousCount, newCount, "Logging did not add a new message from a thread");

		yield return null;
	}
}
