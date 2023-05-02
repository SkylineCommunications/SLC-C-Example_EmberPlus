namespace QAction_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using EmberLib.Glow;
	using Skyline.DataMiner.Scripting;

	/// <summary>
	///     The only class used for the local object model.
	///     An Element instance can represent either a node, a parameter or
	///     the root of the local object tree.
	/// </summary>
	internal class Element : IGlowVisitor<object, bool>
	{
		private readonly List<Element> children = new List<Element>();

		/// <summary>
		///     Initializes a new instance of the <see cref="Element" /> class.
		/// </summary>
		/// <param name="parent">The parent element. Null if the new instance is the root element.</param>
		/// <param name="number">The node/parameter number of the new instance.</param>
		/// <param name="identifier">The node/parameter identifier of the new instance.</param>
		/// <param name="type">The type of the new instance.</param>
		/// <param name="protocol"></param>
		private Element(Element parent, int number, string identifier, ElementType type, SLProtocolExt protocol)
		{
			Protocol = protocol;
			Parent = parent;
			Number = number;
			Update(identifier, type);
		}

		/// <summary>
		///     Gets the collection of child Element objects.
		/// </summary>
		public IEnumerable<Element> Children => children;

		/// <summary>
		///     Gets the node/parameter identifier.
		/// </summary>
		public string Identifier { get; private set; }

		/// <summary>
		///     Gets the node/parameter number.
		/// </summary>
		public int Number { get; }

		/// <summary>
		///     Gets the parameter type of the parameter this element represents.
		///     Only valid if Type is ElementType.Parameter.
		/// </summary>
		public int ParameterType { get; private set; }

		/// <summary>
		///     Gets the parent element. Null if this instance is the root element.
		/// </summary>
		public Element Parent { get; }

		public SLProtocolExt Protocol { get; }

		/// <summary>
		///     Gets the type of the element.
		/// </summary>
		public ElementType Type { get; private set; }

		public object Value { get; set; }

		public static Element CreateElement(SLProtocolExt protocol, Element parent, int number, string identifier, ElementType type)
		{
			return new Element(parent, number, identifier, type, protocol);
		}

		public void AddEmberElementToTable(SLProtocolExt protocol)
		{
			var emberNodeRows = new List<QActionTableRow>();
			var emberNodeTableRow = new EmbernodestableQActionRow
			{
				Embernodesidentifier_101 = Identifier,
				Embernodesnumber_102 = Number,
				Embernodesparent_103 = Parent.Identifier,
			};

			emberNodeRows.Add(emberNodeTableRow);

			var emberParameterRows = new List<QActionTableRow>();

			foreach (var child in children)
			{
				if (child.Type == ElementType.Node)
				{
					var emberNodeRow = new EmbernodestableQActionRow
					{
						Embernodesidentifier_101 = child.Identifier,
						Embernodesnumber_102 = child.Number,
						Embernodesparent_103 = Identifier,
					};

					emberNodeRows.Add(emberNodeRow);

					continue;
				}

				var emberParameterTableRow = new EmberparameterstableQActionRow
				{
					Emberparametersidentifier_111 = child.Identifier,
					Emberparametersnumber_112 = child.Number,
					Emberparametersparent_113 = child.Parent.Identifier,
					Emberparameterstype_114 = child.ParameterType,
					Emberparametersvalue_115 = child.Value,
				};

				emberParameterRows.Add(emberParameterTableRow);
			}

			protocol.emberparameterstable.FillArrayNoDelete(emberParameterRows);
			protocol.embernodestable.FillArrayNoDelete(emberNodeRows);
		}

		/// <summary>
		///     Creates an ember/glow tree which can be used to issue
		///     the "dir" command to the remote host.
		/// </summary>
		/// <returns>
		///     The GlowContainer object that is the root of
		///     a glow tree that mirrors the element tree up this element,
		///     having the GlowCommand object as the single leaf.
		/// </returns>
		public GlowContainer GetDirectory()
		{
			var glow = new GlowCommand(GlowCommandType.GetDirectory);

			// this builds a QualifiedNode or QualifiedParameter,
			// containing the GetDirectory Command
			// return BuildQualified(this, glow);

			// this builds a complete glow tree using the Node and
			// Parameter types (more verbose).
			return BuildGlowTree(this, glow);
		}

		/// <summary>
		///     Writes out information about an element to a string.
		///     Used for listing the current cursor's children.
		/// </summary>
		/// <returns>A string containing information about the passed element.</returns>
		public string PrintElement()
		{
			var sb = new StringBuilder();

			foreach (var child in children)
			{
				sb.AppendLine(
					$"\t\tChild: Type: {child.Type}, Parent: {child.Parent.Identifier}, Number: {child.Number:000}, Identifier: {child.Identifier}, ParameterType: {child.ParameterType}, Value: {child.Value}");
			}

			return $"\nType: {Type}, Number: {Number:000}, Identifier: {Identifier}, ParameterType: {ParameterType}" +
				   "\n\tChildren:" +
				   $"\n{sb}";
		}

		/// <summary>
		///     Writes out information about an element to a string.
		///     Used for listing the current cursor's children.
		/// </summary>
		/// <returns>A string containing information about the passed element.</returns>
		public string PrintNode()
		{
			return $"Type: {Type}, Number: {Number:000}, Identifier: {Identifier}, ParameterType: {ParameterType}";
		}

		public string PrintParameter()
		{
			return $"Type: {Type}, Number: {Number:000}, Identifier: {Identifier}, ParameterType: {ParameterType}, Value: {Value}";
		}

		///// <summary>
		/////     Creates an ember/glow tree which can be used to issue
		/////     a parameter value change to the remote host.
		///// </summary>
		///// <returns>
		/////     The GlowContainer object that is the root of
		/////     a glow tree that mirrors the element tree up this element,
		/////     having a GlowParameter object as the single leaf.
		///// </returns>
		//public GlowContainer SetParameterValue(GlowValue value)
		//{
		//	if (Type != ElementType.Parameter)
		//	{
		//		throw new InvalidOperationException();
		//	}

		//	var glow = new GlowParameter(Number)
		//	{
		//		Value = value,
		//	};

		//	// this builds a QualifiedNode or QualifiedParameter
		//	// return BuildQualified(Parent, glow);

		//	// this builds a complete glow tree using the Node and
		//	// Parameter types (more verbose).
		//	//return BuildGlowTree(Parent, glow);
		//}

		private static GlowRootElementCollection BuildGlowTree(Element local, GlowElement glow)
		{
			while (local.Type != ElementType.Root)
			{
				GlowElement glowParent;

				if (local.Type == ElementType.Parameter)
				{
					var param = new GlowParameter(local.Number)
					{
						Children = new GlowElementCollection(GlowTags.Parameter.Children),
					};

					param.Children.Insert(glow);
					glowParent = param;
				}
				else
				{
					var node = new GlowNode(local.Number)
					{
						Children = new GlowElementCollection(GlowTags.Node.Children),
					};

					node.Children.Insert(glow);
					glowParent = node;
				}

				glow = glowParent;
				local = local.Parent;
			}

			var root = GlowRootElementCollection.CreateRoot();
			root.Insert(glow);

			return root;
		}

		private static GlowRootElementCollection BuildQualified(Element local, GlowElement glow)
		{
			var qualified = null as GlowElement;

			switch (local.Type)
			{
				case ElementType.Root:
					qualified = glow;

					break;
				case ElementType.Parameter:
					int[] path = local.BuildPath();
					var qparam = new GlowQualifiedParameter(path)
					{
						Children = new GlowElementCollection(GlowTags.QualifiedParameter.Children),
					};

					qparam.Children.Insert(glow);
					qualified = qparam;

					break;
				case ElementType.Node:
					path = local.BuildPath();
					var qnode = new GlowQualifiedNode(path)
					{
						Children = new GlowElementCollection(GlowTags.QualifiedNode.Children),
					};

					qnode.Children.Insert(glow);
					qualified = qnode;

					break;
			}

			if (qualified == null)
			{
				return null;
			}

			var root = GlowRootElementCollection.CreateRoot();
			root.Insert(qualified);

			return root;
		}

		private static void GetValue(GlowParameterBase glow, GlowValue value, Element local)
		{
			switch (value.Type)
			{
				case GlowParameterType.Boolean:
					local.Value = Convert.ToInt32(value.Boolean);

					break;

				case GlowParameterType.Integer:
					local.Value = glow.Factor != null && glow.Factor != 0 ? (double)value.Integer / glow.Factor : value.Integer;

					break;

				case GlowParameterType.Real:
					local.Value = value.Real;

					break;

				case GlowParameterType.String:
					local.Value = value.String.Trim();

					break;

				case GlowParameterType.Enum:
					local.Value = value.Integer;

					break;

				default:
					local.Value = null;

					break;
			}
		}

		private int[] BuildPath()
		{
			var path = new LinkedList<int>();
			var elem = this;

			while (elem != null && elem.Type != ElementType.Root)
			{
				path.AddFirst(elem.Number);
				elem = elem.Parent;
			}

			return path.ToArray();
		}

		/// <summary>
		///     Gets the descendant element with the specified path.
		///     This call only makes sense on the root element.
		/// </summary>
		/// <param name="path">Path to the element to get.</param>
		/// <param name="parent">Receives the parent element of the found element.</param>
		/// <returns>The found element or null if not found.</returns>
		private Element GetElementAt(int[] path, out Element parent)
		{
			var current = this;
			parent = null;

			for (var index = 0; index < path.Length; index++)
			{
				var child = current.Children.FirstOrDefault(elem => elem.Number == path[index]);

				if (child == null)
				{
					parent = index == path.Length - 1
						? current
						: null;

					return null;
				}

				parent = current;
				current = child;
			}

			return current;
		}

		private void Update(string identifier, ElementType type)
		{
			if (identifier != null)
			{
				Identifier = identifier;
			}

			Type = type;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowCommand glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowElementCollection glow, object state)
		{
			var hasCompleteNodeOrParameter = false;

			foreach (var element in glow.Elements)
			{
				hasCompleteNodeOrParameter |= element.Accept(this, state);
			}

			return hasCompleteNodeOrParameter;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowRootElementCollection glow, object state)
		{
			var hasCompleteNodeOrParameter = false;

			foreach (var element in glow.Elements)
			{
				hasCompleteNodeOrParameter |= element.Accept(this, state);
			}

			return hasCompleteNodeOrParameter;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowNode glow, object state)
		{
			var local = children.FirstOrDefault(elem => elem.Number == glow.Number);

			bool isComplete = glow.Identifier != null;

			if (local == null)
			{
				local = new Element(this, glow.Number, glow.Identifier, ElementType.Node, Protocol);

				children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Node);
			}

			var glowChildren = glow.Children;

			if (glowChildren != null)
			{
				isComplete |= glowChildren.Accept(local, null);
			}

			return isComplete;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedNode glow, object state)
		{
			var local = GetElementAt(glow.Path, out var parent);
			bool isComplete = glow.Identifier != null;

			if (parent == null)
			{
				return isComplete;
			}

			if (local == null)
			{
				local = new Element(parent, glow.Path.Last(), glow.Identifier, ElementType.Node, Protocol);

				parent.children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Node);
			}

			var glowChildren = glow.Children;

			if (glowChildren != null)
			{
				isComplete |= glowChildren.Accept(local, null);
			}

			return isComplete;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowParameter glow, object state)
		{
			var local = children.FirstOrDefault(elem => elem.Number == glow.Number);

			if (local == null)
			{
				local = new Element(this, glow.Number, glow.Identifier, ElementType.Parameter, Protocol);

				children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Parameter);
			}

			var value = glow.Value;

			if (value != null)
			{
				local.ParameterType = value.Type;

				GetValue(glow, value, local);
			}

			return true;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedParameter glow, object state)
		{
			var local = GetElementAt(glow.Path, out var parent);

			if (parent == null)
			{
				return true;
			}

			if (local == null)
			{
				local = new Element(parent, glow.Path.Last(), glow.Identifier, ElementType.Parameter, Protocol);

				parent.children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Parameter);
			}

			var value = glow.Value;

			if (value != null)
			{
				local.ParameterType = value.Type;

				GetValue(glow, value, local);
			}

			return true;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowStreamCollection glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowSubContainer glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowMatrix glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedMatrix glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowFunction glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedFunction glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowInvocationResult glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowTemplate glow, object state)
		{
			return false;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedTemplate glow, object state)
		{
			return false;
		}
	}
}