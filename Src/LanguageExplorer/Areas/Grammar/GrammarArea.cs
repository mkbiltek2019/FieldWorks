﻿// Copyright (c) 2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SIL.CoreImpl;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Application;
using SIL.FieldWorks.Filters;
using SIL.FieldWorks.XWorks;

namespace LanguageExplorer.Areas.Grammar
{
	/// <summary>
	/// IArea implementation for the grammar area.
	/// </summary>
	internal sealed class GrammarArea : IArea
	{
		private readonly IToolRepository m_toolRepository;

		/// <summary>
		/// Contructor used by Reflection to feed the tool repository to the area.
		/// </summary>
		/// <param name="toolRepository"></param>
		internal GrammarArea(IToolRepository toolRepository)
		{
			m_toolRepository = toolRepository;
		}

		internal static RecordClerk CreateBrowseClerkForGrammarArea(IPropertyTable propertyTable, bool includeTreeBarHandler)
		{
			var cache = propertyTable.GetValue<FdoCache>("cache");
			var recordList = new PossibilityRecordList(cache.ServiceLocator.GetInstance<ISilDataAccessManaged>(), cache.LanguageProject.PartsOfSpeechOA);
			return includeTreeBarHandler
				? new RecordClerk("categories", recordList, new PropertyRecordSorter("ShortName"), "Default", null, false, false, new PossibilityTreeBarHandler(propertyTable, true, true, false, "best analorvern"))
				: new RecordClerk("categories", recordList, new PropertyRecordSorter("ShortName"), "Default", null, false, false);
		}

		#region Implementation of IPropertyTableProvider

		/// <summary>
		/// Placement in the IPropertyTableProvider interface lets FwApp call IPropertyTable.DoStuff.
		/// </summary>
		public IPropertyTable PropertyTable { get; private set; }

		#endregion

		#region Implementation of IPublisherProvider

		/// <summary>
		/// Get the IPublisher.
		/// </summary>
		public IPublisher Publisher { get; private set; }

		#endregion

		#region Implementation of ISubscriberProvider

		/// <summary>
		/// Get the ISubscriber.
		/// </summary>
		public ISubscriber Subscriber { get; private set; }

		#endregion

		#region Implementation of IFlexComponent

		/// <summary>
		/// Initialize a FLEx component with the basic interfaces.
		/// </summary>
		/// <param name="flexComponentParameterObject">Parameter object that contains the required three interfaces.</param>
		public void InitializeFlexComponent(FlexComponentParameterObject flexComponentParameterObject)
		{
			FlexComponentCheckingService.CheckInitializationValues(flexComponentParameterObject, new FlexComponentParameterObject(PropertyTable, Publisher, Subscriber));

			PropertyTable = flexComponentParameterObject.PropertyTable;
			Publisher = flexComponentParameterObject.Publisher;
			Subscriber = flexComponentParameterObject.Subscriber;
		}

		#endregion

		#region Implementation of IMajorFlexComponent

		/// <summary>
		/// Deactivate the component.
		/// </summary>
		/// <remarks>
		/// This is called on the outgoing component, when the user switches to a component.
		/// </remarks>
		public void Deactivate(ICollapsingSplitContainer mainCollapsingSplitContainer,
			MenuStrip menuStrip, ToolStripContainer toolStripContainer, StatusBar statusbar)
		{
		}

		/// <summary>
		/// Activate the component.
		/// </summary>
		/// <remarks>
		/// This is called on the component that is becoming active.
		/// </remarks>
		public void Activate(ICollapsingSplitContainer mainCollapsingSplitContainer,
			MenuStrip menuStrip,
			ToolStripContainer toolStripContainer, StatusBar statusbar)
		{
		}

		/// <summary>
		/// Do whatever might be needed to get ready for a refresh.
		/// </summary>
		public void PrepareToRefresh()
		{
		}

		/// <summary>
		/// Finish the refresh.
		/// </summary>
		public void FinishRefresh()
		{
		}

		/// <summary>
		/// The properties are about to be saved, so make sure they are all current.
		/// Add new ones, as needed.
		/// </summary>
		public void EnsurePropertiesAreCurrent()
		{
			PropertyTable.SetProperty("InitialArea", MachineName, SettingsGroup.LocalSettings, true, false);

			var myCurrentTool = m_toolRepository.GetPersistedOrDefaultToolForArea(this);
			myCurrentTool.EnsurePropertiesAreCurrent();
		}

		#endregion

		#region Implementation of IMajorFlexUiComponent

		/// <summary>
		/// Get the internal name of the component.
		/// </summary>
		/// <remarks>NB: This is the machine friendly name, not the user friendly name.</remarks>
		public string MachineName
		{
			get { return "grammar"; }
		}

		/// <summary>
		/// User-visible localizable component name.
		/// </summary>
		public string UiName
		{
			get { return "Grammar"; }
		}

		#endregion

		#region Implementation of IArea

		/// <summary>
		/// Get the most recently persisted tool, or the default tool if
		/// the persisted one is no longer available.
		/// </summary>
		/// <returns>The last persisted tool or the default tool for the area.</returns>
		public ITool GetPersistedOrDefaultToolForArea()
		{
			return m_toolRepository.GetPersistedOrDefaultToolForArea(this);
		}

		/// <summary>
		/// Get the machine name of the area's default tool.
		/// </summary>
		public string DefaultToolMachineName
		{
			get { return "posEdit"; }
		}

		/// <summary>
		/// Get all installed tools for the area.
		/// </summary>
		public IList<ITool> AllToolsInOrder
		{
			get
			{
				var myToolsInOrder = new List<string>
				{
					"posEdit",
					"categoryBrowse",
					"compoundRuleAdvancedEdit",
					"phonemeEdit",
					"phonologicalFeaturesAdvancedEdit",
					"bulkEditPhonemes",
					"naturalClassEdit",
					"EnvironmentEdit",
					"PhonologicalRuleEdit",
					"AdhocCoprohibitionRuleEdit",
					"featuresAdvancedEdit",
					"ProdRestrictEdit",
					"grammarSketch",
					"lexiconProblems"
				};
				return m_toolRepository.AllToolsForAreaInOrder(myToolsInOrder, MachineName);
			}
		}

		/// <summary>
		/// Get the image for the area.
		/// </summary>
		public Image Icon
		{
			get { return LanguageExplorerResources.Grammar.ToBitmap(); }
		}

		#endregion
	}
}
