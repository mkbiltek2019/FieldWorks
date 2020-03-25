// Copyright (c) 2006-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using SIL.LCModel.Core.WritingSystems;
using SIL.WritingSystems;

namespace SIL.FieldWorks.FwCoreDlgs.Controls
{
	/// <summary />
	public class DefaultFontsControl : UserControl
	{

		#region Member variables
		private FwOverrideComboBox m_defaultFontComboBox;
		private Label m_defaultFontLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private FontFeaturesButton m_defaultFontFeaturesButton;
		private HelpProvider m_helpProvider;

		private CoreWritingSystemDefinition m_ws;
		private CheckBox m_enableGraphiteCheckBox;
		private GroupBox m_graphiteGroupBox;
		#endregion

		#region Constructor/destructor

		/// <summary />
		public DefaultFontsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Fill in the font selection combo boxes.

			using (var installedFontCollection = new InstalledFontCollection())
			{
				// Mono doesn't sort the font names currently 20100322. Workaround for FWNX-273: Fonts not in alphabetical order
				foreach (var family in installedFontCollection.Families.OrderBy(family => family.Name))
				{
					// The .NET framework is unforgiving of using a font that doesn't support the
					// "regular"  style.  So we won't allow the user to even see them...
					if (family.IsStyleAvailable(FontStyle.Regular))
					{
						var familyName = family.Name;
						m_defaultFontComboBox.Items.Add(familyName);
						family.Dispose();
					}
				}
			}
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			if (IsDisposed)
			{
				// No need to run it more than once.
				return;
			}

			if (disposing)
			{
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Properties
		/// <summary>
		/// The language definition object that the controls will modify and
		/// from which they will be initialized.
		/// </summary>
		public CoreWritingSystemDefinition WritingSystem
		{
			get => m_ws;
			set
			{
				m_ws = value;
				SetSelectedFonts();
			}
		}

		/// <summary>
		/// Access the Default Normal Font property.
		/// </summary>
		public string DefaultNormalFont
		{
			get => m_defaultFontComboBox.Text;
			set => m_defaultFontComboBox.Text = value;
		}

		internal ComboBox DefaultFontComboBox => m_defaultFontComboBox;
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultFontsControl));
			this.m_defaultFontLabel = new System.Windows.Forms.Label();
			this.m_helpProvider = new System.Windows.Forms.HelpProvider();
			this.m_defaultFontComboBox = new FwOverrideComboBox();
			this.m_defaultFontFeaturesButton = new SIL.FieldWorks.FwCoreDlgs.Controls.FontFeaturesButton();
			this.m_enableGraphiteCheckBox = new System.Windows.Forms.CheckBox();
			this.m_graphiteGroupBox = new System.Windows.Forms.GroupBox();
			this.m_graphiteGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// m_defaultFontLabel
			//
			resources.ApplyResources(this.m_defaultFontLabel, "m_defaultFontLabel");
			this.m_defaultFontLabel.BackColor = System.Drawing.Color.Transparent;
			this.m_defaultFontLabel.Name = "m_defaultFontLabel";
			this.m_helpProvider.SetShowHelp(this.m_defaultFontLabel, ((bool)(resources.GetObject("m_defaultFontLabel.ShowHelp"))));
			//
			// m_defaultFontComboBox
			//
			this.m_defaultFontComboBox.AllowSpaceInEditBox = false;
			this.m_defaultFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_helpProvider.SetHelpString(this.m_defaultFontComboBox, resources.GetString("m_defaultFontComboBox.HelpString"));
			resources.ApplyResources(this.m_defaultFontComboBox, "m_defaultFontComboBox");
			this.m_defaultFontComboBox.Name = "m_defaultFontComboBox";
			this.m_helpProvider.SetShowHelp(this.m_defaultFontComboBox, ((bool)(resources.GetObject("m_defaultFontComboBox.ShowHelp"))));
			this.m_defaultFontComboBox.SelectedIndexChanged += new System.EventHandler(this.m_defaultFontComboBox_SelectedIndexChanged);
			//
			// m_defaultFontFeaturesButton
			//
			resources.ApplyResources(this.m_defaultFontFeaturesButton, "m_defaultFontFeaturesButton");
			this.m_defaultFontFeaturesButton.FontFeatures = null;
			this.m_defaultFontFeaturesButton.FontName = null;
			this.m_helpProvider.SetHelpString(this.m_defaultFontFeaturesButton, resources.GetString("m_defaultFontFeaturesButton.HelpString"));
			this.m_defaultFontFeaturesButton.Name = "m_defaultFontFeaturesButton";
			this.m_helpProvider.SetShowHelp(this.m_defaultFontFeaturesButton, ((bool)(resources.GetObject("m_defaultFontFeaturesButton.ShowHelp"))));
			this.m_defaultFontFeaturesButton.WritingSystemFactory = null;
			this.m_defaultFontFeaturesButton.FontFeatureSelected += new System.EventHandler(this.m_defaultFontFeaturesButton_FontFeatureSelected);
			//
			// m_enableGraphiteCheckBox
			//
			resources.ApplyResources(this.m_enableGraphiteCheckBox, "m_enableGraphiteCheckBox");
			this.m_enableGraphiteCheckBox.Name = "m_enableGraphiteCheckBox";
			this.m_enableGraphiteCheckBox.UseVisualStyleBackColor = true;
			this.m_enableGraphiteCheckBox.Click += new System.EventHandler(this.m_enableGraphiteCheckBox_Click);
			//
			// m_graphiteGroupBox
			//
			this.m_graphiteGroupBox.Controls.Add(this.m_enableGraphiteCheckBox);
			this.m_graphiteGroupBox.Controls.Add(this.m_defaultFontFeaturesButton);
			resources.ApplyResources(this.m_graphiteGroupBox, "m_graphiteGroupBox");
			this.m_graphiteGroupBox.Name = "m_graphiteGroupBox";
			this.m_graphiteGroupBox.TabStop = false;
			//
			// DefaultFontsControl
			//
			this.Controls.Add(this.m_graphiteGroupBox);
			this.Controls.Add(this.m_defaultFontComboBox);
			this.Controls.Add(this.m_defaultFontLabel);
			this.Name = "DefaultFontsControl";
			this.m_helpProvider.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
			resources.ApplyResources(this, "$this");
			this.m_graphiteGroupBox.ResumeLayout(false);
			this.m_graphiteGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region Helper methods

		/// <summary>
		/// Select the Fonts in the Font comboboxes, and set any features into the controls.
		/// </summary>
		protected void SetSelectedFonts()
		{
			if (m_ws == null)
			{
				return;     // can't do anything useful.
			}

			// setup controls for default font
			SetFontInCombo(m_defaultFontComboBox, m_ws.DefaultFontName);
			m_defaultFontFeaturesButton.WritingSystemFactory = m_ws.WritingSystemFactory;
			m_defaultFontFeaturesButton.FontName = m_defaultFontComboBox.Text;
			m_defaultFontFeaturesButton.FontFeatures = m_ws.DefaultFontFeatures;

			var isGraphiteFont = m_defaultFontFeaturesButton.IsGraphiteFont;
			m_graphiteGroupBox.Enabled = isGraphiteFont;
			if (!isGraphiteFont)
			{
				m_ws.IsGraphiteEnabled = false;
			}
			m_enableGraphiteCheckBox.Checked = m_ws.IsGraphiteEnabled;
			if (!m_ws.IsGraphiteEnabled)
			{
				m_defaultFontFeaturesButton.Enabled = false;
			}
		}

		/// <summary>
		/// Set the font in the ComboBox.  If the selection ends up null, mark the font as
		/// missing and display it as such.  See LT-8750.
		/// </summary>
		private static void SetFontInCombo(FwOverrideComboBox fwcb, string sFont)
		{
			fwcb.SelectedItem = sFont;
			if (fwcb.SelectedItem == null)
			{
				var sMissingFmt = Strings.kstidMissingFontFmt;
				var sMissing = string.Format(sMissingFmt, sFont);
				fwcb.SelectedItem = sMissing;
				if (fwcb.SelectedItem == null)
				{
					fwcb.Items.Add(sMissing);
					fwcb.SelectedItem = sMissing;
				}
			}
		}
		#endregion

		#region Event handlers
		/// <summary>
		/// Handles the SelectedIndexChanged event of the cbDefaultNormalFont control.
		/// </summary>
		private void m_defaultFontComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_ws == null)
			{
				return;
			}
			var oldFont = m_ws.DefaultFontName;
			if (oldFont != m_defaultFontComboBox.Text)
			{
				// Finding a font missing should not result in persisting a font name change
				if (string.Format(Strings.kstidMissingFontFmt, oldFont) != m_defaultFontComboBox.Text)
				{
					if (!m_ws.Fonts.TryGet(m_defaultFontComboBox.Text, out var font))
					{
						font = new FontDefinition(m_defaultFontComboBox.Text);
					}
					m_ws.DefaultFont = font;
				}

				if (m_ws.DefaultFont != null)
				{
					m_defaultFontFeaturesButton.FontName = m_defaultFontComboBox.Text;
					m_defaultFontFeaturesButton.FontFeatures = m_ws.DefaultFont.Features;
				}

				var isGraphiteFont = m_defaultFontFeaturesButton.IsGraphiteFont;
				m_graphiteGroupBox.Enabled = isGraphiteFont;
				m_ws.IsGraphiteEnabled = false;
				m_enableGraphiteCheckBox.Checked = false;
				m_defaultFontFeaturesButton.Enabled = false;
			}
		}

		/// <summary>
		/// Handles the FontFeatureSelected event of the m_defaultFontFeatures control.
		/// </summary>
		private void m_defaultFontFeaturesButton_FontFeatureSelected(object sender, EventArgs e)
		{
			if (m_ws == null)
			{
				return;
			}
			m_ws.DefaultFont.Features = m_defaultFontFeaturesButton.FontFeatures;
		}

		private void m_enableGraphiteCheckBox_Click(object sender, EventArgs e)
		{
			if (m_ws == null)
			{
				return;
			}
			m_ws.IsGraphiteEnabled = m_enableGraphiteCheckBox.Checked;
			if (m_ws.IsGraphiteEnabled)
			{
				m_defaultFontFeaturesButton.SetupFontFeatures();
			}
			else
			{
				m_defaultFontFeaturesButton.Enabled = false;
			}
		}

		#endregion
	}
}