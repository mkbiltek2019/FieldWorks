// Copyright (c) 2002-2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Windows.Forms;
using SIL.CoreImpl;

namespace LanguageExplorer.Controls
{
	/// <summary />
	/// <remarks>
	/// Used by: FindExampleSentenceDlg
	/// </remarks>
	internal class BasicPaneBarContainer : UserControl
	{
		#region Data Members

		/// <summary />
		protected IPaneBar m_paneBar;

		#endregion Data Members

		/// <summary>
		/// Placement in the IPropertyTableProvider interface lets FwApp call IPropertyTable.DoStuff.
		/// </summary>
		public IPropertyTable PropertyTable { get; set; }

		/// <summary>
		/// Init for basic PaneBar.
		/// </summary>
		/// <param name="propertyTable"></param>
		/// <param name="mainControl"></param>
		/// <param name="paneBar"></param>
		public void Init(IPropertyTable propertyTable, Control mainControl, IPaneBar paneBar)
		{
			if (PropertyTable != null && PropertyTable != propertyTable)
				throw new ArgumentException("Mis-matched property tables being set for this object.");

			PropertyTable = propertyTable;
			PaneBar = paneBar;
			Controls.Add(PaneBar as Control);

			mainControl.Dock = DockStyle.Fill;
			Controls.Add(mainControl);
			mainControl.BringToFront();
		}

		/// <summary />
		public IPaneBar PaneBar
		{
			get { return m_paneBar; }
			private set
			{
				if (m_paneBar != null)
				{
					throw new InvalidOperationException(@"Pane bar container already has a pane bar.");
				}
				m_paneBar = value;
				var pbAsControl = m_paneBar as Control;
				if (pbAsControl != null && pbAsControl.AccessibleName == null)
				{
					pbAsControl.AccessibleName = "XCore.PaneBar";
				}
			}
		}
	}
}