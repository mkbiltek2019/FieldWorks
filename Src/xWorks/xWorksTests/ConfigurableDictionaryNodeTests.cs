﻿// Copyright (c) 2014 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NUnit.Framework;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO.FDOTests;

namespace SIL.FieldWorks.XWorks
{
	public class ConfigurableDictionaryNodeTests
	{
		[Test]
		public void ChildlessCanDeepClone()
		{
			var parent = new ConfigurableDictionaryNode();
			var child = new ConfigurableDictionaryNode() { After = "after", IsEnabled = true, Parent = parent };
			// SUT
			var clone = child.DeepCloneUnderSameParent();
			VerifyDuplication(clone, child);
			Assert.That(clone.Label, Is.EqualTo(child.Label));
		}

		[Test]
		public void CanDeepClone()
		{
			var parent = new ConfigurableDictionaryNode();
			var child = new ConfigurableDictionaryNode() { After = "after", IsEnabled = true, Parent = parent };
			var grandchild = new ConfigurableDictionaryNode() { Before = "childBefore", Parent = child };
			child.Children = new List<ConfigurableDictionaryNode>() { grandchild };
			// SUT
			var clone = child.DeepCloneUnderSameParent();
			VerifyDuplication(clone, child);
			Assert.That(clone.Label, Is.EqualTo(child.Label));
		}

		private static void VerifyDuplication(ConfigurableDictionaryNode clone, ConfigurableDictionaryNode node)
		{
			Assert.That(clone.Parent, Is.EqualTo(node.Parent));
			Assert.That(clone.Parent, Is.SameAs(node.Parent));
			VerifyDuplicationInner(clone, node);
		}

		private static void VerifyDuplicationInner(ConfigurableDictionaryNode clone, ConfigurableDictionaryNode node)
		{
			Assert.That(clone.FieldDescription, Is.EqualTo(node.FieldDescription));
			Assert.That(clone.Style, Is.EqualTo(node.Style));
			Assert.That(clone.Before, Is.EqualTo(node.Before));
			Assert.That(clone.After, Is.EqualTo(node.After));
			Assert.That(clone.Between, Is.EqualTo(node.Between));
			Assert.That(clone.DictionaryNodeOptions, Is.EqualTo(node.DictionaryNodeOptions));
			Assert.That(clone.IsEnabled, Is.EqualTo(node.IsEnabled));

			if (node.Children != null)
			{
				Assert.That(clone.Children.Count, Is.EqualTo(node.Children.Count));
				for (int childIndex = 0; childIndex < node.Children.Count; childIndex++)
				{
					Assert.That(clone.Children[childIndex].Label, Is.EqualTo(node.Children[childIndex].Label));
					VerifyDuplicationInner(clone.Children[childIndex], node.Children[childIndex]);
					Assert.That(clone.Children[childIndex], Is.Not.SameAs(node.Children[childIndex]), "Didn't deep-clone");
					Assert.That(clone.Children[childIndex].Parent, Is.SameAs(clone), "cloned children were not re-parented within deep-cloned object");
					Assert.That(clone.Children[childIndex].Parent, Is.Not.SameAs(node.Children[childIndex].Parent), "Cloned children should be pointing to different parent nodes than the original");
				}
			}
		}

		[Test]
		public void DuplicateIsPutAmongSiblings()
		{
			var parent = new ConfigurableDictionaryNode();
			var childA = new ConfigurableDictionaryNode() { After = "after", IsEnabled = true, Parent = parent };
			var grandchildA = new ConfigurableDictionaryNode() { Before = "childBefore", Parent = childA };
			childA.Children = new List<ConfigurableDictionaryNode>() { grandchildA };
			var childB = new ConfigurableDictionaryNode() { After = "nodeBAfter", Parent = parent };
			parent.Children = new List<ConfigurableDictionaryNode>() { childA, childB };

			// SUT
			var duplicate = childA.DuplicateAmongSiblings();
			VerifyDuplication(duplicate, childA);
			Assert.That(parent.Children.Count, Is.EqualTo(3), "should have increased");
			Assert.That(parent.Children.Contains(duplicate), Is.True, "duplicate should be listed among siblings, added to the parent's list of children");
		}

		[Test]
		public void DuplicatesAreMarkedAsSuch()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>() };
			var node = new ConfigurableDictionaryNode() { Parent = parent };
			parent.Children.Add(node);
			Assert.That(node.IsDuplicate, Is.False);

			// SUT
			var duplicate = node.DuplicateAmongSiblings();
			Assert.That(duplicate.IsDuplicate, Is.True);
			Assert.That(node.IsDuplicate, Is.False, "Original should not have been marked as a duplicate.");
		}

		[Test]
		public void DuplicatesHaveUniqueLabels()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>() };
			var nodeToDuplicateLabel = "node";
			var nodeToDuplicate = new ConfigurableDictionaryNode() { Parent = parent, Label = nodeToDuplicateLabel};
			var otherNodeA = new ConfigurableDictionaryNode() { Parent = parent, Label = "node (1)" };
			var otherNodeB = new ConfigurableDictionaryNode() { Parent = parent, Label = "node B" };
			parent.Children.Add(nodeToDuplicate);
			parent.Children.Add(otherNodeA);
			parent.Children.Add(otherNodeB);

			// SUT
			var duplicate = nodeToDuplicate.DuplicateAmongSiblings();
			Assert.That(parent.Children.FindAll(node => node.Label == nodeToDuplicate.Label).Count, Is.EqualTo(1), "Should not have any more nodes with the original label. Was the duplicate node's label not changed?");
			Assert.That(parent.Children.FindAll(node => node.Label == duplicate.Label).Count, Is.EqualTo(1), "The duplicate node was not given a unique label among its siblings.");
			Assert.That(nodeToDuplicate.Label, Is.EqualTo(nodeToDuplicateLabel), "should not have changed original node label");
		}

		[Test]
		public void CanUnlink()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var node = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = parent };
			parent.Children.Add(node);
			// SUT
			node.UnlinkFromParent();
			Assert.That(parent.Children.Count, Is.EqualTo(0), "Parent should not link to unlinked child");
			Assert.That(node.Parent, Is.Null, "Node should not still claim the original parent");
		}

		/// <summary>
		/// Can unlink a node twice in a row, or if a node is already at the root of a hierarchy.
		/// </summary>
		[Test]
		public void CanUnlinkTwice()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var node = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = parent };
			parent.Children.Add(node);
			node.UnlinkFromParent();
			Assert.That(node.Parent, Is.Null); // node is now at the root of a hierarchy
			// SUT
			Assert.DoesNotThrow(() => node.UnlinkFromParent());
		}

		[Test]
		public void CanRelabel()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var node = new ConfigurableDictionaryNode() { Parent = parent, Label = "originalLabel"};
			parent.Children.Add(node);

			var newLabel = "newLabel";
			// SUT
			node.Relabel(newLabel);
			Assert.That(node.Label, Is.EqualTo(newLabel), "Label was not updated");
		}

		[Test]
		public void ReportSuccessfulRelabel()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var node = new ConfigurableDictionaryNode() { Parent = parent, Label = "originalLabel" };
			parent.Children.Add(node);

			// SUT
			var result = node.Relabel("newLabel");
			Assert.That(result, Is.True);
		}

		/// <summary>
		/// Disallow relabeling with a label that is identical to an existing label among a node's siblings.
		/// </summary>
		[Test]
		public void RelabelWontUseAnExistingLabel()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var originalLabel = "originalLabel";
			var node = new ConfigurableDictionaryNode() { Parent = parent, Label = originalLabel };
			var otherNode = new ConfigurableDictionaryNode() { Parent = parent, Label = "otherLabel" };
			parent.Children.Add(node);
			parent.Children.Add(otherNode);

			// SUT
			var result = node.Relabel(otherNode.Label);
			Assert.That(result, Is.False, "Should have reported failure to relabel");
			Assert.That(node.Label, Is.EqualTo(originalLabel), "Should not have changed label to the same value as an existing label");
		}

		[Test]
		public void CanRelabelToSameLabel()
		{
			var parent = new ConfigurableDictionaryNode() { Children = new List<ConfigurableDictionaryNode>(), Parent = null };
			var originalLabel = "originalLabel";
			var node = new ConfigurableDictionaryNode() { Parent = parent, Label = originalLabel };
			parent.Children.Add(node);

			// SUT
			var result = node.Relabel(originalLabel);
			Assert.That(result, Is.True, "Allow relabeling to the same label");
			Assert.That(node.Label, Is.EqualTo(originalLabel), "Should not have changed label");
		}
	}
}
