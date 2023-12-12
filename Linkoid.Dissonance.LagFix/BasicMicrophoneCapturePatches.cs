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
	[HarmonyPatch(typeof(BasicMicrophoneCapture))]
	internal class BasicMicrophoneCapturePatches
	{
		[HarmonyPatch(nameof(BasicMicrophoneCapture.DrainMicSamples))]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> DrainMicSamples_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Log fakeLog = Logs.Create(0, "");
			MethodInfo meth_Warn_uint_uint_float = ((Action<string, uint, uint, float>)fakeLog.Warn<uint, uint, float>).Method;
			MethodInfo smeth_WarnWithCooldown = AccessTools.DeclaredMethod(typeof(BasicMicrophoneCapturePatches), nameof(WarnWithCooldown));

			CodeMatcher matcher = new CodeMatcher(instructions);

			/*  - REPLACE -
			 * BasicMicrophoneCapture.Log.Warn<uint, uint, float>(format, count, maxCount, p2);
			 *  - WITH -
			 * WarnWithCooldown(BasicMicrophoneCapture.Log, format, count, maxCount, p2);
			 */
			CodeMatch[] warnInvoke =
			{
				// ?.Warn<uint, uint, float>()
				new(OpCodes.Callvirt, meth_Warn_uint_uint_float),
			};
			matcher.MatchForward(false, warnInvoke);
			matcher.Set(OpCodes.Call, smeth_WarnWithCooldown);

			return matcher.InstructionEnumeration();
		}

		private static float lastWarn = float.MinValue;
		private static void WarnWithCooldown(Log log, string format, uint count, uint maxCount, float p2)
		{
			float time = Time.unscaledTime;
			if (time - lastWarn > 1)
			{
				lastWarn = time;
				log.Warn(format, count, maxCount, p2);
			}
		}
	}
}
