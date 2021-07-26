using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace Photon.Bolt
{
	[Preserve]
	public static class BoltDynamicData
	{
		private static Dictionary<string, Assembly> _assemblies;

		private static List<STuple<BoltGlobalBehaviourAttribute, Type>> _globalBehaviours;

		private static Dictionary<string, Assembly> GetAssemblies
		{
			get
			{
				if (_assemblies == null)
				{
					_assemblies = new Dictionary<string, Assembly>();

					foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
					{
						var name = asm.GetName().Name;
						if (_assemblies.ContainsKey(name) == false)
						{
							_assemblies.Add(name, asm);
						}
					}
				}

				return _assemblies;
			}
		}

		public static void Setup()
		{
			BoltNetworkInternal.DebugDrawer = new UnityDebugDrawer();

#if UNITY_PRO_LICENSE
			BoltNetworkInternal.UsingUnityPro = true;
#else
			BoltNetworkInternal.UsingUnityPro = false;
#endif

			BoltNetworkInternal.GetActiveSceneIndex = GetActiveSceneIndex;
			BoltNetworkInternal.GetSceneName = GetSceneName;
			BoltNetworkInternal.GetSceneIndex = GetSceneIndex;
			BoltNetworkInternal.GetGlobalBehaviourTypes = GetGlobalBehaviourTypes;
			BoltNetworkInternal.EnvironmentSetup = BoltNetworkInternal_User.EnvironmentSetup;
			BoltNetworkInternal.EnvironmentReset = BoltNetworkInternal_User.EnvironmentReset;

			// Setup Unity Config

#if ENABLE_IL2CPP
			UnitySettings.IsBuildIL2CPP = true;
#elif ENABLE_MONO
			UnitySettings.IsBuildMono = true;
#elif ENABLE_DOTNET
			UnitySettings.IsBuildDotNet = true;
#endif

			UnitySettings.CurrentPlatform = Application.platform;
		}

		private static int GetActiveSceneIndex()
		{
			return GetSceneIndex(SceneManager.GetActiveScene().name);
		}

		private static int GetSceneIndex(string name)
		{
			try
			{
				return BoltScenes_Internal.GetSceneIndex(name);
			}
			catch
			{
				return -1;
			}
		}

		private static string GetSceneName(int index)
		{
			try
			{
				return BoltScenes_Internal.GetSceneName(index);
			}
			catch
			{
				return null;
			}
		}

		private static List<STuple<BoltGlobalBehaviourAttribute, Type>> GetGlobalBehaviourTypes()
		{
			if (_globalBehaviours == null)
			{
				_globalBehaviours = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();
			}
			else
			{
				_globalBehaviours.Clear();
			}

			try
			{
				var asmIter = BoltAssemblies.AllAssemblies;
				while (asmIter.MoveNext())
				{
					if (GetAssemblies.TryGetValue(asmIter.Current, out var asm))
					{
						foreach (Type type in asm.GetTypes())
						{
							if (typeof(MonoBehaviour).IsAssignableFrom(type))
							{
								var attrs = (BoltGlobalBehaviourAttribute[])type.GetCustomAttributes(typeof(BoltGlobalBehaviourAttribute), false);

								if (attrs.Length == 1)
								{
									_globalBehaviours.Add(new STuple<BoltGlobalBehaviourAttribute, Type>(attrs[0], type));
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				BoltLog.Exception(e);
			}

			return _globalBehaviours;
		}
	}
}
