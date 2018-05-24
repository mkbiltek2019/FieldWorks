// Copyright (c) 2015-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using LanguageExplorer.Controls.PaneBar;
using LanguageExplorer.Controls.XMLViews;
using SIL.Code;
using SIL.FieldWorks.Common.FwUtils;
using SIL.LCModel.Core.Text;
using SIL.LCModel.Core.KernelInterfaces;
using SIL.LCModel.Infrastructure;
using SIL.LCModel;

namespace LanguageExplorer.Areas.Lexicon.Tools.Edit
{
#if RANDYTODO
// TODO: Used by this:
/*
			<guicontrol id="findExampleSentences">
				<dynamicloaderinfo assemblyPath="LanguageExplorer.dll" class="LanguageExplorer.Areas.Lexicon.Tools.Edit.FindExampleSentenceDlg"/>
				<parameters id="senseConcordanceControls">
					<control id="ConcOccurrenceList">
						<dynamicloaderinfo assemblyPath="LanguageExplorer.dll" class="LanguageExplorer.Areas.Lexicon.Tools.Edit.ConcOccurrenceBrowseView"/>
						<parameters id="ConcOccurrenceList" selectColumn="true" defaultChecked="false" forceReloadListOnInitOrChangeRoot="true" editable="false" clerk="OccurrencesOfSense" filterBar="true" ShowOwnerShortname="true">
<!-- START include Lexicon Area (a10status="Fork has this in resources now"): "./Words/reusableBrowseControlConfiguration.xml" query="reusableControls/control[@id='concordanceColumns']/columns" -->
<!-- Look for it in TextAndWordsResources.ConcordanceColumns -->
<!-- END include Lexicon Area (a10status="Fork has this in resources now"): "./Words/reusableBrowseControlConfiguration.xml" query="reusableControls/control[@id='concordanceColumns']/columns" -->
						</parameters>
					</control>
					<control id="SegmentPreviewControl">
						<dynamicloaderinfo assemblyPath="LanguageExplorer.dll" class="LanguageExplorer.Areas.Lexicon.Tools.Edit.RecordDocXmlView" />
						<parameters id="SegmentPreviewControl" clerk="OccurrencesOfSense" treeBarAvailability="NotMyBusiness" layout="publicationNew" editable="false"/>
					</control>
				</parameters>
			</guicontrol>
*/
#endif
	/// <summary />
	internal partial class FindExampleSentenceDlg : Form, IFwGuiControl
	{
		LcmCache m_cache;
		XmlNode m_configurationNode;
		ILexExampleSentence m_les;
		ILexSense m_owningSense;
		ConcOccurrenceBrowseView m_rbv;
		XmlView m_previewPane;
		string m_helpTopic = "khtpFindExampleSentence";
		IRecordList m_recordList;
		private StatusBar _statusBar;

		/// <summary />
		public FindExampleSentenceDlg()
		{
			InitializeComponent();
		}

		internal FindExampleSentenceDlg(StatusBar statusBar) : this()
		{
			Guard.AgainstNull(statusBar, nameof(statusBar));

			_statusBar = statusBar;
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
		/// <param name="flexComponentParameters">Parameter object that contains the required three interfaces.</param>
		public void InitializeFlexComponent(FlexComponentParameters flexComponentParameters)
		{
			FlexComponentCheckingService.CheckInitializationValues(flexComponentParameters, new FlexComponentParameters(PropertyTable, Publisher, Subscriber));

			PropertyTable = flexComponentParameters.PropertyTable;
			Publisher = flexComponentParameters.Publisher;
			Subscriber = flexComponentParameters.Subscriber;
		}

		#endregion

		#region IFwGuiControl Members

		/// <summary />
		public void Init(XmlNode configurationNode, ICmObject sourceObject)
		{
			m_cache = sourceObject.Cache;

			// Find the sense we want examples for, which depends on the kind of source object.
			if (sourceObject is ILexExampleSentence)
			{
				m_les = (ILexExampleSentence)sourceObject;
				if (m_les.Owner is ILexSense)
				{
					m_owningSense = (ILexSense)m_les.Owner;
				}
				else if (m_les.Owner is ILexExtendedNote)
				{
					m_owningSense = (ILexSense)m_les.Owner.Owner;
				}
			}
			else if (sourceObject is ILexSense)
			{
				m_owningSense = (ILexSense)sourceObject;
			}
			else if (sourceObject is ILexExtendedNote)
			{
				m_owningSense = (ILexSense)sourceObject.Owner;
			}
			else
			{
				throw new ArgumentException("Invalid object type for sourceObject.");
			}

			m_configurationNode = configurationNode;

			helpProvider.SetHelpNavigator(this, HelpNavigator.Topic);
			helpProvider.SetShowHelp(this, true);
			var helpToicProvider = PropertyTable.GetValue<IHelpTopicProvider>("HelpTopicProvider");
			if (helpToicProvider != null)
			{
				helpProvider.HelpNamespace = helpToicProvider.HelpFile;
				helpProvider.SetHelpKeyword(this, helpToicProvider.GetHelpString(m_helpTopic));
				btnHelp.Enabled = true;
			}

			AddConfigurableControls();
		}

		/// <summary />
		public void Launch()
		{
			ShowDialog(PropertyTable.GetValue<Form>("window"));
		}

		#endregion

		private XmlNode BrowseViewControlParameters => m_configurationNode.SelectSingleNode("control/parameters[@id='ConcOccurrenceList']");

		private void AddConfigurableControls()
		{
			// Load the controls.
			// 1. Initialize the preview pane (lower pane)
			m_previewPane = new XmlView(0, "publicationNew", false)
			{
				Cache = m_cache,
				StyleSheet = FwUtils.StyleSheetFromPropertyTable(PropertyTable)
			};

			var pbc = new BasicPaneBarContainer();
			pbc.Init(PropertyTable, m_previewPane, new PaneBar());
			pbc.Dock = DockStyle.Fill;
			pbc.PaneBar.Text = LanguageExplorerResources.ksFindExampleSentenceDlgPreviewPaneTitle;
			panel2.Controls.Add(pbc);
			if (m_previewPane.RootBox == null)
			{
				m_previewPane.MakeRoot();
			}

			// 2. load the browse view. (upper pane)
			var xnBrowseViewControlParameters = BrowseViewControlParameters;

			/*
			<clerk id="OccurrencesOfSense" shouldHandleDeletion="false">
			  <dynamicloaderinfo assemblyPath="xWorks.dll" class="SIL.FieldWorks.XWorks.ConcRecordClerk" />
			  <recordList class="LexSense" field="Occurrences">
				<decoratorClass assemblyPath="xWorks.dll" class="SIL.FieldWorks.XWorks.ConcDecorator" />
			  </recordList>
			  <filters />
			  <sortMethods />
			</clerk>
			*/
			// First create our record list, since we can't set it's OwningObject via the configuration/mediator/PropertyTable info.
			// This record list is a "TemporaryRecordList" suclass, so dispose it when the dlg goes away.
			var flexParameters = new FlexComponentParameters(PropertyTable, Publisher, Subscriber);
			var concDecorator = new ConcDecorator(m_cache.ServiceLocator);
			concDecorator.InitializeFlexComponent(flexParameters);
			m_recordList = new ConcRecordList(_statusBar, m_cache, concDecorator, m_owningSense);

			m_rbv = DynamicLoader.CreateObject(xnBrowseViewControlParameters.ParentNode.SelectSingleNode("dynamicloaderinfo")) as ConcOccurrenceBrowseView;
			m_rbv.InitializeFlexComponent(flexParameters);
			m_rbv.Init(m_previewPane, m_recordList.VirtualListPublisher);
			m_rbv.CheckBoxChanged += m_rbv_CheckBoxChanged;
			// add it to our controls.
			var pbc1 = new BasicPaneBarContainer();
			pbc1.Init(PropertyTable, m_rbv, new PaneBar());
			pbc1.BorderStyle = BorderStyle.FixedSingle;
			pbc1.Dock = DockStyle.Fill;
			pbc1.PaneBar.Text = LanguageExplorerResources.ksFindExampleSentenceDlgBrowseViewPaneTitle;
			panel1.Controls.Add(pbc1);

			CheckAddBtnEnabling();
		}

		void m_rbv_CheckBoxChanged(object sender, CheckBoxChangedEventArgs e)
		{
			CheckAddBtnEnabling();
		}

		private void CheckAddBtnEnabling()
		{
			btnAdd.Enabled = m_rbv.CheckedItems.Count > 0;
		}
		private void btnAdd_Click(object sender, EventArgs e)
		{
			// Get the checked occurrences;
			var occurrences = m_rbv.CheckedItems;
			if (occurrences == null || occurrences.Count == 0)
			{
				// do nothing.
				return;
			}
			var uniqueSegments = (occurrences.Select(fake => m_recordList.VirtualListPublisher.get_ObjectProp(fake, ConcDecorator.kflidSegment))).Distinct().ToList();
			var insertIndex = m_owningSense.ExamplesOS.Count; // by default, insert at the end.
			if (m_les != null)
			{
				// we were given a LexExampleSentence, so set our insertion index after the given one.
				insertIndex = m_owningSense.ExamplesOS.IndexOf(m_les) + 1;
			}

			UndoableUnitOfWorkHelper.Do(LanguageExplorerResources.ksUndoAddExamples, LanguageExplorerResources.ksRedoAddExamples,
				m_cache.ActionHandlerAccessor,
				() =>
					{
						var cNewExamples = 0;
						foreach (var segHvo in uniqueSegments)
						{
							var seg = m_cache.ServiceLocator.GetObject(segHvo) as ISegment;
							ILexExampleSentence newLexExample;
							if (cNewExamples == 0 && m_les != null &&
								m_les.Example.BestVernacularAlternative.Text == "***" &&
								(m_les.TranslationsOC == null || m_les.TranslationsOC.Count == 0) &&
								m_les.Reference.Length == 0)
							{
								// we were given an empty LexExampleSentence, so use this one for our first new Example.
								newLexExample = m_les;
							}
							else
							{
								// create a new example sentence.
								newLexExample = m_cache.ServiceLocator.GetInstance<ILexExampleSentenceFactory>().Create();
								m_owningSense.ExamplesOS.Insert(insertIndex + cNewExamples, newLexExample);
								cNewExamples++;
							}
							// copy the segment string into the new LexExampleSentence
							// Enhance: bold the relevant occurrence(s).
							// LT-11388 Make sure baseline text gets copied into correct ws
							var baseWs = GetBestVernWsForNewExample(seg);
							newLexExample.Example.set_String(baseWs, seg.BaselineText);
							if (seg.FreeTranslation.AvailableWritingSystemIds.Length > 0)
							{
								var trans = m_cache.ServiceLocator.GetInstance<ICmTranslationFactory>().Create(newLexExample,
									m_cache.ServiceLocator.GetInstance<ICmPossibilityRepository>().GetObject(CmPossibilityTags.kguidTranFreeTranslation));
								trans.Translation.CopyAlternatives(seg.FreeTranslation);
							}
							if (seg.LiteralTranslation.AvailableWritingSystemIds.Length > 0)
							{
								var trans = m_cache.ServiceLocator.GetInstance<ICmTranslationFactory>().Create(newLexExample,
									m_cache.ServiceLocator.GetInstance<ICmPossibilityRepository>().GetObject(CmPossibilityTags.kguidTranLiteralTranslation));
								trans.Translation.CopyAlternatives(seg.LiteralTranslation);
							}
						   // copy the reference.
							var tssRef = seg.Paragraph.Reference(seg, seg.BeginOffset);
							// convert the plain reference string into a link.
							var tsb = tssRef.GetBldr();
							var fwl = new FwLinkArgs(AreaServices.InterlinearEditMachineName, seg.Owner.Owner.Guid);
							tsb.SetStrPropValue(0, tsb.Length, (int)FwTextPropType.ktptObjData,(char)FwObjDataTypes.kodtExternalPathName + fwl.ToString());
							tsb.SetStrPropValue(0, tsb.Length, (int)FwTextPropType.ktptNamedStyle, "Hyperlink");
							newLexExample.Reference = tsb.GetString();
						}
					});
		}

		private int GetBestVernWsForNewExample(ISegment seg)
		{
			var baseWs = seg.BaselineText.get_WritingSystem(0);
			if (baseWs < 1)
			{
				return m_cache.DefaultVernWs;
			}

			var possibleWss = m_cache.ServiceLocator.WritingSystems.VernacularWritingSystems;
			var wsObj = m_cache.ServiceLocator.WritingSystemManager.Get(baseWs);
			return possibleWss.Contains(wsObj) ? baseWs : m_cache.DefaultVernWs;
		}

		private void btnHelp_Click(object sender, EventArgs e)
		{
			ShowHelp.ShowHelpTopic(PropertyTable.GetValue<IHelpTopicProvider>("HelpTopicProvider"), m_helpTopic);
		}
	}
}