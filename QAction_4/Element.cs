namespace QAction_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using EmberLib;
	using EmberLib.Glow;
	using EmberLib.Xml;

	/// <summary>
	///     The only class used for the local object model.
	///     An Element instance can represent either a node, a parameter or
	///     the root of the local object tree.
	/// </summary>
	internal class Element : IGlowVisitor<object, bool>
	{
		private readonly List<Element> children = new List<Element>();

		/// <summary>
		///     Constructs a new instance of Element.
		/// </summary>
		/// <param name="parent">The parent element. Null if the new instance is the root element.</param>
		/// <param name="number">The node/parameter number of the new instance.</param>
		/// <param name="identifier">The node/parameter identifier of the new instance.</param>
		/// <param name="type">The type of the new instance.</param>
		/// <param name="xml">The XML representation of the GlowNode/GlowParameter used to create the new instance from.</param>
		public Element(Element parent, int number, string identifier, ElementType type, string xml)
		{
			Parent = parent;
			Number = number;
			Update(identifier, type, xml);
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

		/// <summary>
		///     Gets the type of the element.
		/// </summary>
		public ElementType Type { get; private set; }

		/// <summary>
		///     Gets the XML representation of the node/parameter that has been
		///     used to create this element from.
		/// </summary>
		public string Xml { get; private set; }

		/// <summary>
		/// Creates a new instance of Element which is initialized as root.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="number"></param>
		/// <param name="identifier"></param>
		/// <param name="type"></param>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static Element CreateRoot(Element parent, int number, string identifier, ElementType type, string xml)
		{
			return new Element(parent, number, identifier, type, xml);
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
			return BuildQualified(this, glow);

			// this builds a complete glow tree using the Node and
			// Parameter types (more verbose).
			//return BuildGlowTree(this, glow);
		}

		/// <summary>
		///     Gets the descendant element with the specified path.
		///     This call only makes sense on the root element.
		/// </summary>
		/// <param name="path">Path to the element to get.</param>
		/// <param name="parent">Receives the parent element of the found element.</param>
		/// <returns>The found element or null if not found.</returns>
		public Element GetElementAt(int[] path, out Element parent)
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

		/// <summary>
		///     Writes out information about an element to a string.
		///     Used for listing the current cursor's children.
		/// </summary>
		/// <returns>A string containing information about the passed element.</returns>
		public string PrintElement()
		{
			string type;

			switch (Type)
			{
				case ElementType.Node:
					type = "Node";

					break;

				case ElementType.Parameter:
					type = "Parameter";

					break;

				default:
					type = String.Empty;

					break;
			}

			return $"{type} {Number:000} {Identifier}";
		}

		/// <summary>
		///     Creates an ember/glow tree which can be used to issue
		///     a parameter value change to the remote host.
		/// </summary>
		/// <returns>
		///     The GlowContainer object that is the root of
		///     a glow tree that mirrors the element tree up this element,
		///     having a GlowParameter object as the single leaf.
		/// </returns>
		public GlowContainer SetParameterValue(GlowValue value)
		{
			if (Type != ElementType.Parameter)
			{
				throw new InvalidOperationException();
			}

			var glow = new GlowParameter(Number)
			{
				Value = value,
			};

			// this builds a QualifiedNode or QualifiedParameter
			return BuildQualified(Parent, glow);

			// this builds a complete glow tree using the Node and
			// Parameter types (more verbose).
			//return BuildGlowTree(Parent, glow);
		}

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

			if (local.Type != ElementType.Root)
			{
				int[] path = local.BuildPath();

				if (local.Type == ElementType.Parameter)
				{
					var qparam = new GlowQualifiedParameter(path)
					{
						Children = new GlowElementCollection(GlowTags.QualifiedParameter.Children),
					};

					qparam.Children.Insert(glow);
					qualified = qparam;
				}
				else if (local.Type == ElementType.Node)
				{
					var qnode = new GlowQualifiedNode(path)
					{
						Children = new GlowElementCollection(GlowTags.QualifiedNode.Children),
					};

					qnode.Children.Insert(glow);
					qualified = qnode;
				}
			}
			else
			{
				qualified = glow;
			}

			if (qualified != null)
			{
				var root = GlowRootElementCollection.CreateRoot();
				root.Insert(qualified);

				return root;
			}

			return null;
		}

		private static string BuildXml(EmberNode node)
		{
			var settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				CloseOutput = false,
				OmitXmlDeclaration = true,
			};

			var buffer = new StringBuilder();

			using (var writer = XmlWriter.Create(buffer, settings))
			{
				XmlExport.Export(node, writer);
			}

			return buffer.ToString();
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

		private void Update(string identifier, ElementType type, string xml)
		{
			if (identifier != null)
			{
				Identifier = identifier;
			}

			Type = type;

			if (xml != null)
			{
				Xml = xml;
			}
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
			string xml = isComplete
				? BuildXml(glow)
				: null;

			if (local == null)
			{
				local = new Element(this, glow.Number, glow.Identifier, ElementType.Node, xml);

				children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Node, xml);
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

			string xml = isComplete
				? BuildXml(glow)
				: null;

			if (local == null)
			{
				local = new Element(parent, glow.Path.Last(), glow.Identifier, ElementType.Node, xml);

				parent.children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Node, xml);
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

			string xml = BuildXml(glow);

			if (local == null)
			{
				local = new Element(this, glow.Number, glow.Identifier, ElementType.Parameter, xml);

				children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Parameter, xml);
			}

			var value = glow.Value;

			if (value != null)
			{
				local.ParameterType = value.Type;
			}

			return true;
		}

		bool IGlowVisitor<object, bool>.Visit(GlowQualifiedParameter glow, object state)
		{
			var local = GetElementAt(glow.Path, out var parent);
			string xml = BuildXml(glow);

			if (parent == null)
			{
				return true;
			}

			if (local == null)
			{
				local = new Element(parent, glow.Path.Last(), glow.Identifier, ElementType.Parameter, xml);

				parent.children.Add(local);
			}
			else
			{
				local.Update(glow.Identifier, ElementType.Parameter, xml);
			}

			var value = glow.Value;

			if (value != null)
			{
				local.ParameterType = value.Type;
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