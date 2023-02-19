using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assets.Tests.Utils;
using System.Reflection;
using System.IO;

public class TestLanguage {
	private Type languageLoaderType;
	private Type languageType;

	[SetUp]
	public void Setup() {
		SceneUtils.Load();

		var domain = TypeUtils.GetAssemblyCSharp();
		
		languageLoaderType = domain.GetType("HardCoded.VRigUnity.LanguageLoader");
		languageType = domain.GetType("HardCoded.VRigUnity.LanguageLoader+Language");
	}

	[TearDown]
	public void Teardown() {
	}

	private ICollection GetLanguages() {
		try {
			return languageLoaderType.GetMethod("GetLanguages", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[0]) as ICollection;
		} catch (TargetInvocationException e) {
			Assert.Fail("languages.json was not formatted properly: {0}", e.InnerException);
		}

		return null;
	}

	[UnityTest]
	public IEnumerator ValidateLanguageFormat() {
		yield return null;

		foreach (var item in GetLanguages()) {
			// Check that the debug language is not present
			Assert.IsFalse((bool) languageType.GetProperty("IsDebug").GetValue(item), "The debug language should not be included");
			string code = languageType.GetProperty("Code").GetValue(item) as string;
			string name = languageType.GetProperty("DisplayName").GetValue(item) as string;

			Assert.IsNotEmpty(code, "Language Code should never be empty");
			Assert.IsNotEmpty(name, "Language DisplayName should never be empty");
			
			// Check that the language exists and parses
			try {
				languageLoaderType.GetMethod("LoadLanguage", new Type[] { languageType }).Invoke(null, new object[] { item });
			} catch (TargetInvocationException e) {
				Assert.Fail($"'{code}/{code}.lang' was not formatted properly: {{0}}", e.InnerException);
			}
		}
	}

	[UnityTest]
	public IEnumerator ValidateFallbackInSync() {
		yield return null;

		// Check that the fallback language exists
		var language = languageLoaderType.GetMethod("FromCode", new Type[] { typeof(string) }).Invoke(null, new object[] { "en_US" });
		string path = languageLoaderType.GetMethod("GetLanguagePath", new Type[] { languageType }).Invoke(null, new object[] { language }) as string;
		Assert.IsTrue(File.Exists(path), "Fallback language 'en_US/en_US.lang' does not exist");

		// Check that the fallback language is defined in 'languages.json'
		bool found = false;
		foreach (var item in GetLanguages()) {
			string code = languageType.GetProperty("Code").GetValue(item) as string;
			if (code == "en_US") {
				found = true;
				break;
			}
		}
		Assert.IsTrue(found, "Fallback language 'en_US' was not defined in languages.json");
		
		// Check that the fallback language is in sync with the internal fallback language values
		string template = languageLoaderType.GetProperty("TemplateFallback").GetValue(null) as string;
		string externalContent = string.Join('\n', File.ReadAllLines(path)).Trim();
		string internalContent = template.Trim();
		Assert.IsTrue(externalContent == internalContent, "Fallback language 'en_US' is out of sync");
	}
}
