using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Dissonance.Audio.Capture;
using Dissonance;
using HarmonyLib;
using UnityEngine;

namespace Linkoid.Dissonance.LagFix
{
	[HarmonyPatch(typeof(BasePreprocessingPipeline))]
	internal class BasePreprocessingPipelinePatches
	{
		[HarmonyPatch("Dissonance.Audio.Capture.IMicrophoneSubscriber.ReceiveMicrophoneData")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> ReceiveMicrophoneData_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Log fakeLog = Logs.Create(0, "");
			MethodInfo meth_Warn_int = ((Action<string, int>)fakeLog.Warn<int>).Method;
			MethodInfo smeth_WarnWithCooldown = AccessTools.DeclaredMethod(typeof(BasePreprocessingPipelinePatches), nameof(WarnWithCooldown));

			CodeMatcher matcher = new CodeMatcher(instructions);

			/*  - REPLACE -
			 * BasePreprocessingPipeline.Log.Warn<int>(format, p0);
			 *  - WITH -
			 * ReceiveMicrophoneData_WarnWithCooldown(BasicMicrophoneCapture.Log, format, p0);
			 */
			CodeMatch[] warnInvoke =
			{
				// ?.Warn<int>()
				new(OpCodes.Callvirt, meth_Warn_int),
			};
			matcher.MatchForward(false, warnInvoke);
			matcher.Set(OpCodes.Call, smeth_WarnWithCooldown);

			return matcher.InstructionEnumeration();
		}

		private static float lastWarn = float.MinValue;
		private static void WarnWithCooldown(Log log, string format, int p0)
		{
			float time = Time.unscaledTime;
			if (time - lastWarn > 1)
			{
				lastWarn = time;
				log.Warn(format, p0);
			}
		}
	}
}
