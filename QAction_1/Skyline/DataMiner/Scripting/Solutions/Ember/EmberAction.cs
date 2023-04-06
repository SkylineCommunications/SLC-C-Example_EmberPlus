﻿namespace QAction_1.Skyline.DataMiner.Scripting.Solutions.Ember
{
	using System;
	using EmberLib.Glow;
	using EmberLib.Glow.Framing;
	using global::Skyline.DataMiner.Scripting;
	using QAction_1.Skyline.Ember.Protocol;

	public abstract class EmberAction : GlowWalker
	{
		protected readonly EmberData EmberData;

		protected readonly SLProtocol protocol;

		protected EmberAction(SLProtocol protocol, Configuration configuration, EmberData emberData)
		{
			Configurations = configuration;
			this.protocol = protocol;
			EmberData = emberData;
		}

		public bool Done { get; internal set; }

		protected Configuration Configurations { get; }

		public abstract void Execute();

		public abstract int[] ProcessReceivedGlow(EmberData emberData, GlowContainer glowContainer, int[] validateLastRequestPath);

		protected override void OnCommand(GlowCommand glow, int[] path)
		{
		}

		protected override void OnFunction(GlowFunctionBase glow, int[] path)
		{
		}

		protected override void OnInvocationResult(GlowInvocationResult glow)
		{
		}

		protected override void OnMatrix(GlowMatrixBase glow, int[] path)
		{
		}

		protected override void OnStreamEntry(GlowStreamEntry glow)
		{
		}

		protected override void OnTemplate(GlowTemplateBase glow, int[] path)
		{
		}

		internal object ConvertGlowValue(GlowParameterBase glow)
		{
			object converted = null;

			if (glow?.Value == null)
			{
				return null;
			}

			switch (glow.Type)
			{
				case GlowParameterType.Boolean:
					converted = Convert.ToInt32(glow.Value.Boolean);

					break;

				case GlowParameterType.Integer:
					converted = glow.Factor != null && glow.Factor != 0 ? (double)glow.Value.Integer / glow.Factor : glow.Value.Integer;

					break;

				case GlowParameterType.Real:
					converted = glow.Value.Real;

					break;

				case GlowParameterType.String:
					converted = glow.Value.String.Trim();

					break;

				case GlowParameterType.Enum:
					converted = glow.Value.Integer;

					break;
			}

			return converted;
		}

		internal void GlowPackageReady(byte[] framedPackage)
		{
			protocol.SetParameterBinary(Configurations.S101Pids.S101RequestDataPid, framedPackage);
			protocol.CheckTrigger(Configurations.SendEmberRequestTrigger);
		}

		internal void SendGetDirectoryRequest(int[][] path, bool nested = false)
		{
			var root = GlowRootElementCollection.CreateRoot();
			var command = new GlowCommand(GlowCommandType.GetDirectory);

			if (path == null || (path.Length == 1 && path[0] == null))
			{
				root.Insert(command);
				SendGlow(root);

				return;
			}

			foreach (int[] subPath in path)
			{
				if (!nested)
				{
					var qnode1 = new GlowQualifiedNode(subPath)
					{
						Children = new GlowElementCollection(GlowTags.QualifiedNode.Children),
					};

					qnode1.Children.Insert(command);
					root.Insert(qnode1);
				}
				else
				{
					for (var i = 0; i < subPath.Length; i++)
					{
						var nestedSubPath = new int[i + 1];
						Array.Copy(subPath, nestedSubPath, i + 1);

						var qnode1 = new GlowQualifiedNode(nestedSubPath)
						{
							Children = new GlowElementCollection(GlowTags.QualifiedNode.Children),
						};

						qnode1.Children.Insert(command);
						root.Insert(qnode1);
					}
				}
			}

			SendGlow(root);
		}

		internal void SendGlow(GlowContainer glow)
		{
			var glowOutPut = new GlowOutput(true, 1024, 0x00, (_, e) => GlowPackageReady(e.FramedPackage));

			using (glowOutPut)
			{
				glow.Encode(glowOutPut);
				glowOutPut.Finish();
			}
		}
	}
}