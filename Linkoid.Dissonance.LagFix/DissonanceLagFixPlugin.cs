using BepInEx;
using HarmonyLib;
using Dissonance;
using DissonanceLogLevel = Dissonance.LogLevel;

namespace Linkoid.Dissonance.LagFix
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	public sealed class DissonanceLagFixPlugin : BaseUnityPlugin
	{
		private void Awake()
		{
			Logs.SetLogLevel(LogCategory.Recording, DissonanceLogLevel.Error);

			Harmony harmony = new(PluginInfo.GUID);
			harmony.PatchAll();
			//harmony.PatchAll(typeof(BasicMicrophoneCapturePatches));
		}
	}
}
