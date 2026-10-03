using System.Text;

using Elements.Assets;
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

namespace AnimatorTrackReference
{
	public sealed class AnimatorTrackReference : ResoniteMod
	{
		internal const string NameConstant = nameof(AnimatorTrackReference);

		internal const string VersionConstant = "1.0.0";

		public override string Name => NameConstant;

		public override string Author => "Dominion";

		public override string Version => VersionConstant;

		public override string Link => $"https://github.com/Cyberboss/{NameConstant}";

		[AutoRegisterConfigKey]
		private static readonly ModConfigurationKey<bool> Enabled = new ModConfigurationKey<bool>("Enabled", "Mod Enabled", () => true);

		public override void OnEngineInit()
		{
			GetConfiguration()!.Save(true);

			Debug("Running harmony patches");
			Harmony harmony = new($"net.dextraspace.{NameConstant}");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Animator), nameof(Animator.BuildInspectorUI))]
		class Animator_BuildInspectorUI_Patch
		{
			public static void Postfix(Animator __instance, UIBuilder ui)
			{
				if (!Enabled.Value)
				{
					Debug("Mod disabled, not patching Animator inspector UI");
					return;
				}

				Msg($"Injecting track reference button on Animator: {__instance.ReferenceID}");

				ui.Style.MinHeight = 24f;
				ui.Text($"{NameConstant} (Mod)").Color.Value = RadiantUI_Constants.Hero.CYAN;
				ui.Style.MinHeight = 2f;
				ui.Image(RadiantUI_Constants.Hero.CYAN);
				ui.Style.MinHeight = 24f;

				string buttonText = "Show Track Reference";

				var button = ui.Button(buttonText);
				button.LocalPressed += (btn, _) =>
				{
					var textDisplay = __instance.LocalUserSpace.AddSlot("Animator Track Reference");
					Msg($"Executing reference button. Creating text display: {textDisplay.ReferenceID}");

					textDisplay.PositionInFrontOfUser(float3.Backward);

					var clip = __instance.Clip.IsAssetAvailable
						? __instance.Clip.Asset.Data
						: null;

					UniversalImporter.SpawnText(
						textDisplay,
						"Animator Track Reference",
						clip != null
							? BuildClipReference(clip)
							: "Animation asset was not available! Try again?");
				};
			}
		}

		private static string BuildClipReference(AnimX clip)
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append("Animation Name: ");
			stringBuilder.AppendLine(clip.Name);

			stringBuilder.AppendFormat("Duration: {0:F2}", clip.GlobalDuration);
			stringBuilder.AppendLine();

			stringBuilder.AppendFormat("Tracks: {0}", clip.TrackCount);
			stringBuilder.AppendLine();

			for (var i = 0; i < clip.TrackCount; ++i)
			{
				var track = clip[i];
				stringBuilder.AppendFormat("- [{0}]:", i);
				stringBuilder.AppendLine();
				stringBuilder.Append("\tNode: ");
				stringBuilder.AppendLine(track.Node);
				stringBuilder.Append("\tValue Type: ");
				stringBuilder.Append(track.FrameType.Name);
				stringBuilder.AppendLine();
			}

			return stringBuilder.ToString();
		}
	}
};
