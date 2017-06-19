// Copyright (c) 2003-2013 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)
//
// File: RecordView.cs
// Responsibility: WordWorks
// Last reviewed:
//
// <remarks>
// </remarks>
// ------------------- -------------------------------------------------------------------------
using System;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.FDO;
using SIL.Xml;

namespace SIL.FieldWorks.XWorks
{
	/// <summary>
	/// RecordView is an abstract class for data views that show one object from a list.
	/// A RecordClerk class does most of the work of managing the list and current object.
	///	list management and navigation is entirely handled by the
	/// RecordClerk.
	///
	/// RecordClerk has no knowledge of how to display an individual object. A concrete subclass must handle
	/// this task.
	///
	/// Concrete subclasses must:
	///		1. Implement IxCoreColleague.Init, which should call InitBase, do any other initialization,
	///			and then set m_fullyInitialized.
	///		2. Implement the pane that shows the current object. Typically, set its Dock property to
	///			DockStyle.Fill and add it to this.Controls. This is typically done in an override
	///			of SetupDataContext.
	///		3. Implement ShowRecord to update the view of the object to a display of Clerk.CurrentObject.
	///	Subclasses may:
	///		- Override ReadParameters to extract info from the configuration node. (This is the
	///		representation of the XML <parameters></parameters> node from the <control></control>
	///		node used to invoke the window.)
	///		- Override GetMessageAdditionalTargets to provide message handlers in addition to the
	///		record clerk and this.
	/// </summary>
	public abstract class RecordView : XWorksViewBase
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Consruction and disposal
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="RecordView"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public RecordView()
		{
			Init();
		}

		public RecordView(XElement configurationParametersElement, RecordClerk recordClerk)
			: base(configurationParametersElement, recordClerk)
		{
			Init();
		}

		/// <summary>
		/// Initialize a FLEx component with the basic interfaces.
		/// </summary>
		/// <param name="flexComponentParameters">Parameter object that contains the required three interfaces.</param>
		public override void InitializeFlexComponent(FlexComponentParameters flexComponentParameters)
		{
			base.InitializeFlexComponent(flexComponentParameters);

			Subscriber.Subscribe("RecordNavigation", RecordNavigation_Message_Handler);
		}

	private void Init()
		{
			//it is up to the subclass to change this when it is finished Initializing.
			m_fullyInitialized = false;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//MakePaneBar();

			AccNameDefault = "RecordView"; // default accessibility name
		}

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged
		/// resources; <c>false</c> to release only unmanaged resources.
		/// </param>
		/// -----------------------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if( disposing )
			{
				Subscriber.Unsubscribe("RecordNavigation", RecordNavigation_Message_Handler);
				components?.Dispose();
			}

			base.Dispose( disposing );
		}

		#endregion // Consruction and disposal

		#region Properties

		#endregion Properties

		#region Other methods

		public virtual void RecordNavigation_Message_Handler(object newValue)
		{
			CheckDisposed();

			if(!m_fullyInitialized)
				return;

			var rni = (RecordNavigationInfo)newValue;
			var options = new RecordClerk.ListUpdateHelper.ListUpdateHelperOptions
			{
				SuppressSaveOnChangeRecord = rni.SuppressSaveOnChangeRecord
			};
			using (new RecordClerk.ListUpdateHelper(Clerk, options))
			{
				ShowRecord(rni);
			}
		}

		/// <summary>
		/// Shows the record.
		/// </summary>
		/// <param name="rni">The record navigation info.</param>
		protected virtual void ShowRecord(RecordNavigationInfo rni)
		{
			if (!rni.SkipShowRecord)
				ShowRecord();
		}

		/// <summary>
		/// Shows the record.
		/// </summary>
		protected override void ShowRecord()
		{
			base.ShowRecord();
			if (m_configurationParametersElement != null
				&& !XmlUtils.GetOptionalBooleanAttributeValue(m_configurationParametersElement, "omitFromHistory", false))
			{
				UpdateContextHistory();
			}
		}

		/// <summary>
		/// create and register a URL describing the current context, for use in going backwards and forwards
		/// </summary>
		protected virtual void UpdateContextHistory()
		{
			//are we the dominant pane? The thinking here is that if our clerk is controlling the record tree bar, then we are.
			// The second condition prevents recording the intermediate record in the history when following a link
			// causes us to change areas and then change records.
			if (Clerk.IsControllingTheRecordTreeBar
				&& string.IsNullOrEmpty(PropertyTable.GetValue<string>("SuspendLoadingRecordUntilOnJumpToRecord")))
			{
				//add our current state to the history system
				string toolName = PropertyTable.GetValue("currentContentControl", "");
				Guid guid = Guid.Empty;
				if (Clerk.CurrentObject != null)
					guid = Clerk.CurrentObject.Guid;
				Clerk.SelectedRecordChanged(true, true); // make sure we update the record count in the Status bar.
				Publisher.Publish("AddContextToHistory", new FwLinkArgs(toolName, guid));
			}
		}

		/// <summary>
		/// Note: currently called in the context of ListUpdateHelper, which suspends the clerk from reloading its list
		/// until it is disposed. So, don't do anything here (eg. Clerk.SelectedRecordChanged())
		/// that depends upon a list being loaded yet.
		/// </summary>
		protected override void SetupDataContext()
		{
			TriggerMessageBoxIfAppropriate();

			if(m_treebarAvailability!=TreebarAvailability.NotMyBusiness)
				Clerk.ActivateUI(m_treebarAvailability == TreebarAvailability.Required);//nb optional would be a bug here

			m_madeUpFieldIdentifier = Clerk.VirtualFlid;
		}

		/// <summary>
		/// Initialize this as an IxCoreColleague
		/// </summary>
		/// <remarks> subclasses must call this from their Init.
		/// This was done, rather than providing an Init() here in the normal way,
		/// to drive home the point that the subclass must set m_fullyInitialized
		/// to true when it is fully initialized.</remarks>
		protected void InitBase()
		{
			Debug.Assert(m_fullyInitialized == false, "No way we are fully initialized yet!");

			ReadParameters();

			RecordClerk clerk = ExistingClerk;
			Debug.Assert(clerk != null);
			bool fClerkAlreadySuppressed = false;
			bool fClerkWasCreated = false;
			fClerkAlreadySuppressed = clerk.ListLoadingSuppressed; // If we didn't create the clerk, someone else might have suppressed it.
			// suspend any loading of the Clerk's list items until after a
			// subclass (possibly) initializes sorters/filters
			// in SetupDataContext()
			using (var luh = new RecordClerk.ListUpdateHelper(clerk, fClerkAlreadySuppressed))
			{
				luh.ClearBrowseListUntilReload = true;
				clerk.UpdateOwningObjectIfNeeded();
				SetTreebarAvailability();
				AddPaneBar();

				//Historical comments here indicated that the Clerk should be processed by the mediator before the
				//view. This is handled by Priority now, RecordView is by default just after RecordClerk in the processing.
				SetupDataContext();
			}
			// In case it hasn't yet been loaded, load it!  See LT-10185.
			if (!Clerk.ListLoadingSuppressed && Clerk.RequestedLoadWhileSuppressed)
				Clerk.UpdateList(true, true); // sluggishness culprit for LT-12844 was in here
			Clerk.SetCurrentFromRelatedClerk(); // See if some other clerk wants to influence our current object.
		}

		private string GetClerkPersistPathname()
		{
			return GetSortFilePersistPathname(Cache, Clerk.Id);
		}

		internal static string GetSortFilePersistPathname(FdoCache cache, string clerkId)
		{
			var filename = clerkId + "_SortSeq";
			//(This extension is also known to ProjectRestoreService.RestoreFrom7_0AndNewerBackup.)
			// Also to IFwMainWnd.DiscardProperties().
			var filenameWithExt = Path.ChangeExtension(filename, "fwss");
			var tempDirectory = Path.Combine(cache.ProjectId.ProjectFolder, FdoFileHelper.ksSortSequenceTempDir);
			if (!Directory.Exists(tempDirectory))
				Directory.CreateDirectory(tempDirectory);
			return Path.Combine(tempDirectory, filenameWithExt);
		}

		protected virtual void PersistSortSequence()
		{
			if (Clerk == null || Clerk.IsDisposed)
				return; // temporary clerk, such as a concordance in find example dialog.
			// If we're being disposed because the application is crashing, we do NOT want to save the sort
			// sequence. It might contain bad objects, or represent a filtered state that is NOT going to
			// be persisted because of the crash. LT-11446.
			if (FwUtils.InCrashedState)
				return;
			var pathname = GetClerkPersistPathname();
			var watch = new Stopwatch();
			watch.Start();
			Clerk.PersistListOn(pathname);
			watch.Stop();
			Debug.WriteLine("Saving clerk " + pathname + " took " + watch.ElapsedMilliseconds + " ms.");
		}

		// Enhance JohnT: need to verify that sort sequence is current.
		private bool RestoreSortSequence()
		{
			var pathname = GetClerkPersistPathname();
			if (!File.Exists(pathname))
				return false;
			var watch = new Stopwatch();
			watch.Start();
			var result = Clerk.RestoreListFrom(pathname);
			watch.Stop();
			Debug.WriteLine("Restoring clerk " + pathname + " took " + watch.ElapsedMilliseconds + " ms.");
			return result;
		}

		private void SetTreebarAvailability()
		{
			string a = XmlUtils.GetOptionalAttributeValue(m_configurationParametersElement, "treeBarAvailability", "");

			if(a == "NotMyBusiness")
				m_treebarAvailability = TreebarAvailability.NotMyBusiness;
			else
			{
				if(a != "")
					m_treebarAvailability = (TreebarAvailability)Enum.Parse(typeof(TreebarAvailability), a, true);
				else
					m_treebarAvailability = DefaultTreeBarAvailability;

				//m_previousShowTreeBarValue= PropertyTable.GetValue<bool>("ShowRecordList", SettingsGroup.GlobalSettings);

				//				string e = XmlUtils.GetOptionalAttributeValue(m_configurationParametersElement, "treeBarAvailability", DefaultTreeBarAvailability);
				//				m_treebarAvailability = (TreebarAvailability)Enum.Parse(typeof(TreebarAvailability), e, true);

				switch (m_treebarAvailability)
				{
					default:
						break;
					case TreebarAvailability.NotAllowed:
						break;

					case TreebarAvailability.Required:
						break;

					case TreebarAvailability.Optional:
						//Just use it however the last guy left it (see: PrepareToGoAway())
						break;
				}
			}
		}

		/// <summary>
		/// if the XML configuration does not specify the availability of the treebar
		/// (e.g. treeBarAvailability="Required"), then use this.
		/// </summary>
		protected virtual TreebarAvailability DefaultTreeBarAvailability
		{
			get
			{
				return TreebarAvailability.Required;
			}
		}

		#endregion // Other methods

		#region Component Designer generated code

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			//
			// RecordView
			//
			this.Name = "RecordView";
			this.Size = new System.Drawing.Size(752, 150);

		}
		#endregion
	}
}
