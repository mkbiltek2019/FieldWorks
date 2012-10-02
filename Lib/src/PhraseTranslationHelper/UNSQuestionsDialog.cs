// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2011, SIL International. All Rights Reserved.
// <copyright from='2011' to='2011' company='SIL International'>
//		Copyright (c) 2011, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: UNSQuestionsDialog.cs
// Responsibility: Tom Bogle
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SIL.Utils;
using SILUBS.SharedScrControls;
using SILUBS.SharedScrUtils;

namespace SILUBS.PhraseTranslationHelper
{
	#region UNSQuestionsDialog class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// UNSQuestionsDialog.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class UNSQuestionsDialog : Form
	{
		#region Member Data
		private readonly string m_projectName;
		private readonly IEnumerable<IKeyTerm> m_keyTerms;
		private readonly Font m_vernFont;
		private readonly string m_vernIcuLocale;
		private readonly Action<bool> m_selectKeyboard;
		private readonly Action m_helpDelegate;
		private PhraseTranslationHelper m_helper;
		private readonly string m_translationsFile;
		private readonly string m_phraseSubstitutionsFile;
		private static readonly string s_unsDataFolder;
		private readonly string m_defaultLcfFolder;
		private readonly string m_appName;
		private ScrReference m_startRef;
		private ScrReference m_endRef;
		private IDictionary<string, string> m_sectionHeadText;
		private int[] m_availableBookIds;
		private readonly string m_questionsFilename;
		private DateTime m_lastSaveTime;
		private readonly List<Substitution> m_phraseSubstitutions;
		private bool m_fIgnoreNextRecvdSantaFeSyncMessage;
		private bool m_fProcessingSyncMessage;
		private ScrReference m_queuedReference;
		private int m_lastRowEntered = -1;
		private TranslatablePhrase m_currentPhrase = null;
		private int m_iCurrentColumn = -1;
		private int m_normalRowHeight = -1;
		#endregion

		#region Delegates
		public Func<IEnumerable<int>> GetAvailableBooks { private get; set; }
		#endregion

		#region Properties
		private DataGridViewTextBoxEditingControl TextControl { get; set;}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ComprehensionCheckingSettings Settings
		{
			get { return new ComprehensionCheckingSettings(this); }
		}

		internal PhraseTranslationHelper.KeyTermFilterType CheckedKeyTermFilterType
		{
			get
			{
				return (PhraseTranslationHelper.KeyTermFilterType)mnuKtFilter.DropDownItems.Cast<ToolStripMenuItem>().First(menu => menu.Checked).Tag;
			}
			private set
			{
				mnuKtFilter.DropDownItems.Cast<ToolStripMenuItem>().Where(
					menu => (PhraseTranslationHelper.KeyTermFilterType)menu.Tag == value).First().Checked = true;
				ApplyFilter();
			}
		}

		protected bool SaveNeeded
		{
			get { return btnSave.Enabled; }
			set
			{
				if (mnuAutoSave.Checked && DateTime.Now > m_lastSaveTime.AddSeconds(10))
					Save();
				else
					saveToolStripMenuItem.Enabled = btnSave.Enabled = value;
			}
		}

		protected IEnumerable<int> AvailableBookIds
		{
			get
			{
				if (GetAvailableBooks != null)
				{
					foreach (int i in GetAvailableBooks())
						yield return i;
				}
				else
				{
					for (int i = 1; i <= BCVRef.LastBook; i++)
						yield return i;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether the textual question filter requires whole-
		/// word matches.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool MatchWholeWords
		{
			get { return mnuMatchWholeWords.Checked; }
			set { mnuMatchWholeWords.Checked = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether toolbar is displayed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool ShowToolbar
		{
			get { return mnuViewToolbar.Checked; }
			set { mnuViewToolbar.Checked = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether to send Scripture references as Santa-Fe
		/// messages.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool SendScrRefs
		{
			get { return btnSendScrReferences.Checked; }
			set { btnSendScrReferences.Checked = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether to receive Santa-Fe Scripture reference
		/// focus messages.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool ReceiveScrRefs
		{
			get { return btnReceiveScrReferences.Checked; }
			set { btnReceiveScrReferences.Checked = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether to show a pane with the answers and comments
		/// on the questions.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool ShowAnswersAndComments
		{
			get { return mnuViewAnswers.Checked; }
			set { mnuViewAnswers.Checked = value; }
		}

		internal GenerateTemplateSettings GenTemplateSettings { get; private set; }

		internal BCVRef GetScrRefOfRow(int iRow)
		{
			string sRef = dataGridUns.Rows[iRow].Cells[m_colReference.Index].Value as string;
			if (string.IsNullOrEmpty(sRef))
				return null;
			int ichDash = sRef.IndexOf('-');
			if (ichDash > 0)
				sRef = sRef.Substring(0, ichDash);
			return new BCVRef(sRef);
		}
		#endregion

		#region class AnswersAndComments
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the questions from the file
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal class AnswersAndComments
		{
			private List<string> m_answers;
			private List<string> m_comments;

			internal void AddAnswer(string answer)
			{
				if (string.IsNullOrEmpty(answer))
					return;
				if (m_answers == null)
					m_answers = new List<string>(1);
				m_answers.Add(answer);
			}

			internal void AddComment(string comment)
			{
				if (string.IsNullOrEmpty(comment))
					return;
				if (m_comments == null)
					m_comments = new List<string>(1);
				m_comments.Add(comment);
			}

			internal bool HasAnswer
			{
				get { return m_answers != null;}
			}

			internal bool HasComment
			{
				get { return m_comments != null; }
			}

			internal IEnumerable<string> Answers
			{
				get
				{
					if (m_answers != null)
						return m_answers;
					return new List<string>();
				}
			}

			internal IEnumerable<string> Comments
			{
				get
				{
					if (m_comments != null)
						return m_comments;
					return new List<string>();
				}
			}
		}
		#endregion

		#region class QuestionProvider
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the questions from the file
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private class QuestionProvider : IEnumerable<TranslatablePhrase>
		{
			private readonly string m_sFilename;
			private readonly Dictionary<string, string> m_sectionHeadText = new Dictionary<string, string>();
			private bool m_finishedParsingFile = false;
			private static readonly string s_kSectionHead = @"\rf";
			private static readonly string s_kRefMarker = @"\tqref";
			private static readonly string s_kQuestionMarker = @"\bttq";
			private static readonly string s_kAnswerMarker = @"\tqe";
			private static readonly string s_kCommentMarker = @"\an";
			private static readonly List<string> s_categories = new List<string>();
			private HashSet<int> m_canonicalBookNumbers;

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Initializes the <see cref="QuestionProvider"/> class.
			/// </summary>
			/// --------------------------------------------------------------------------------
			static QuestionProvider()
			{
				s_categories.Add(@"\oh");
				s_categories.Add(@"\dh");
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="QuestionProvider"/> class.
			/// </summary>
			/// <param name="filename">The filename.</param>
			/// --------------------------------------------------------------------------------
			internal QuestionProvider(string filename)
			{
				m_sFilename = filename;
			}

			internal static void ParseRefRange(string sReference, out BCVRef startRef, out BCVRef endRef)
			{
				startRef = new BCVRef();
				endRef = new BCVRef();
				BCVRef.ParseRefRange(sReference, ref startRef, ref endRef);
			}

			/// ------------------------------------------------------------------------------------
			/// <summary>
			/// Parses the given reference (that could be a verse bridge) and returns a BBBCCCVVV
			/// integer representing the start and end references.
			/// </summary>
			/// ------------------------------------------------------------------------------------
			private void Parse(string sReference, out int startRef, out int endRef)
			{
				BCVRef bcvStartRef, bcvEndRef;
				ParseRefRange(sReference, out bcvStartRef, out bcvEndRef);
				startRef = bcvStartRef;
				endRef = bcvEndRef;
				if (bcvStartRef.Valid)
					m_canonicalBookNumbers.Add(bcvStartRef.Book);
			}

			#region Internal Properties
			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Gets a dictionary that correlates (textual) Scripture references to
			/// corresponding section head text (note that these are not the section heads in
			/// the vernacular Scripture but rather from the master question file).
			/// </summary>
			/// --------------------------------------------------------------------------------
			internal IDictionary<string, string> SectionHeads
			{
				get
				{
					if (!m_finishedParsingFile)
						throw new InvalidOperationException("Cannot access SectionHeads until all TranslatablePhrases have been retrieved");
					return m_sectionHeadText;
				}
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Gets an array of canonical book ids for which questions exist.
			/// </summary>
			/// --------------------------------------------------------------------------------
			internal int[] AvailableBookIds
			{
				get
				{
					if (!m_finishedParsingFile)
						throw new InvalidOperationException("Cannot access AvailableBookIds until all TranslatablePhrases have been retrieved");
					return m_canonicalBookNumbers.ToArray();
				}
			}
			#endregion

			#region IEnumerable<string> Members
			public IEnumerator<TranslatablePhrase> GetEnumerator()
			{
				m_finishedParsingFile = false;
				m_canonicalBookNumbers = new HashSet<int>();

				// Initialize the ID textbox.
				TextReader reader = null;
				int currCat = -1;
				string currRef = null;
				string currQuestion = null;
				AnswersAndComments currAnswersAndComments = new AnswersAndComments();
				int startRef = 0, endRef = 0, seq = 0;
				List<int> categoriesAdded = new List<int>(s_categories.Count);
				int kSectHeadMarkerLen = s_kSectionHead.Length;
				int kRefMarkerLen = s_kRefMarker.Length;
				int kQMarkerLen = s_kQuestionMarker.Length;
				int kAMarkerLen = s_kAnswerMarker.Length;
				int kCommentMarkerLen = s_kCommentMarker.Length;
				string sectionHeadText = null;
				try
				{
					reader = new StreamReader(m_sFilename, Encoding.UTF8);

					string sLine;
					while ((sLine = reader.ReadLine()) != null)
					{
						if (sLine.StartsWith(s_kQuestionMarker))
						{
							if (currQuestion != null)
							{
								yield return new TranslatablePhrase(currQuestion, currCat, currRef, startRef, endRef, seq++, currAnswersAndComments);
								currAnswersAndComments = new AnswersAndComments();
							}
							currQuestion = sLine.Substring(kQMarkerLen).Trim();
						}
						else if (sLine.StartsWith(s_kAnswerMarker))
						{
							currAnswersAndComments.AddAnswer(sLine.Substring(kAMarkerLen).Trim());
						}
						else if (sLine.StartsWith(s_kCommentMarker))
						{
							currAnswersAndComments.AddComment(sLine.Substring(kCommentMarkerLen).Trim());
						}
						else
						{
							if (currQuestion != null)
							{
								yield return new TranslatablePhrase(currQuestion, currCat, currRef, startRef, endRef, seq++, currAnswersAndComments);
								currQuestion = null;
								currAnswersAndComments = new AnswersAndComments();
							}

							if (sLine.StartsWith(s_kRefMarker))
							{
								currRef = sLine.Substring(kRefMarkerLen).Trim();
								if (sectionHeadText != null)
								{
									m_sectionHeadText[currRef] = sectionHeadText;
									sectionHeadText = null;
								}
								Parse(currRef, out startRef, out endRef);
								seq = 0;
							}
							else if (sLine.StartsWith(s_kSectionHead))
							{
								sectionHeadText = sLine.Substring(kSectHeadMarkerLen).Trim();
							}
							else
							{
								for (int i = 0; i < s_categories.Count; i++)
								{
									string category = s_categories[i];
									if (sLine.StartsWith(category))
									{
										if (i == 0)
											startRef = endRef = 0;
										seq = 0;
										currCat = i;
										if (!categoriesAdded.Contains(i))
										{
											yield return new TranslatablePhrase(sLine.Substring(category.Length).Trim(),
												-1, string.Empty,
												ScrReference.StartOfBible(ScrVers.English).BBCCCVVV,
												ScrReference.EndOfBible(ScrVers.English).BBCCCVVV, i);
											categoriesAdded.Add(i);
										}
										break;
									}
								}
							}
						}
					}
					if (currQuestion != null)
						yield return new TranslatablePhrase(currQuestion, currCat, currRef, startRef, endRef, seq, currAnswersAndComments);
				}
				finally
				{
					if (reader != null)
						reader.Close();
					m_finishedParsingFile = true;
				}
			}
			#endregion

			#region IEnumerable Members
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion
		}
		#endregion

		#region Constructors
		static UNSQuestionsDialog()
		{
			s_unsDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"UNS Questions");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="UNSQuestionsDialog"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public UNSQuestionsDialog(string projectName, IEnumerable<IKeyTerm> keyTerms,
			Font vernFont, string VernIcuLocale, bool fVernIsRtoL, string sDefaultLcfFolder,
			ComprehensionCheckingSettings settings, string appName, ScrReference startRef,
			ScrReference endRef, Action<bool> selectKeyboard, Action helpDelegate)
		{
			if (startRef != ScrReference.Empty && endRef != ScrReference.Empty && startRef > endRef)
				throw new ArgumentException("startRef must be before endRef");
			m_projectName = projectName;
			m_keyTerms = keyTerms;
			m_vernFont = vernFont;
			m_vernIcuLocale = VernIcuLocale;
			m_selectKeyboard = selectKeyboard;
			m_helpDelegate = helpDelegate;
			m_defaultLcfFolder = sDefaultLcfFolder;
			m_appName = appName;
			m_startRef = startRef;
			m_endRef = endRef;

			InitializeComponent();

			m_biblicalTermsPane.Controls.Clear();

			Text = String.Format(Text, projectName);
			HelpButton = (m_helpDelegate != null);

			mnuShowAllPhrases.Tag = PhraseTranslationHelper.KeyTermFilterType.All;
			mnuShowPhrasesWithKtRenderings.Tag = PhraseTranslationHelper.KeyTermFilterType.WithRenderings;
			mnuShowPhrasesWithMissingKtRenderings.Tag = PhraseTranslationHelper.KeyTermFilterType.WithoutRenderings;
			m_lblAnswerLabel.Tag = m_lblAnswerLabel.Text.Trim();
			m_lblCommentLabel.Tag = m_lblCommentLabel.Text.Trim();
			lblFilterIndicator.Tag = lblFilterIndicator.Text;
			lblRemainingWork.Tag = lblRemainingWork.Text;

			Location = settings.Location;
			WindowState = settings.DefaultWindowState;
			if (MinimumSize.Height <= settings.DialogSize.Height &&
				MinimumSize.Width <= settings.DialogSize.Width)
			{
				Size = settings.DialogSize;
			}
			MatchWholeWords = !settings.MatchPartialWords;
			ShowToolbar = settings.ShowToolbar;
			GenTemplateSettings = settings.GenTemplateSettings;
			ReceiveScrRefs = settings.ReceiveScrRefs;
			ShowAnswersAndComments = settings.ShowAnswersAndComments;

			DataGridViewCellStyle translationCellStyle = new DataGridViewCellStyle();
			translationCellStyle.Font = vernFont;
			m_colTranslation.DefaultCellStyle = translationCellStyle;
			if (fVernIsRtoL)
				m_colTranslation.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			dataGridUns.RowTemplate.MinimumHeight = dataGridUns.RowTemplate.Height = m_normalRowHeight =
				(int)Math.Ceiling(vernFont.Height * CreateGraphics().DpiY / 72) + 2;
			Margin = new Padding(Margin.Left, toolStrip1.Height, Margin.Right, Margin.Bottom);

			m_questionsFilename = settings.QuestionsFile;
			if (!File.Exists(m_questionsFilename))
			{
				MessageBox.Show(Properties.Resources.kstidFileNotFound + settings.QuestionsFile, Text);
				return;
			}

			m_translationsFile = Path.Combine(s_unsDataFolder, string.Format("Translations of Checking Questions - {0}.xml", projectName));
			m_phraseSubstitutionsFile = Path.Combine(s_unsDataFolder, string.Format("Phrase substitutions - {0}.xml", projectName));
			m_phraseSubstitutions = XmlSerializationHelper.LoadOrCreateList<Substitution>(m_phraseSubstitutionsFile, true);
			KeyTermMatch.RenderingInfoFile = Path.Combine(s_unsDataFolder, string.Format("Key term rendering info - {0}.xml", projectName));

			LoadTranslations();

			// Now apply settings that have filtering or other side-effects
			CheckedKeyTermFilterType = settings.KeyTermFilterType;
			SendScrRefs = settings.SendScrRefs;
		}
		#endregion

		#region Events
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (SaveNeeded)
			{
				switch (MessageBox.Show(this, "You have made changes. Do you wish to save before closing?",
					"Save changes?", MessageBoxButtons.YesNoCancel))
				{
					case DialogResult.Yes:
						Save();
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}

			base.OnClosing(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Processes Windows messages.
		/// </summary>
		/// <param name="msg">The Windows Message to process.</param>
		/// ------------------------------------------------------------------------------------
		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == SantaFeFocusMessageHandler.FocusMsg)
			{
				// Always assume the English versification scheme for passing references.
				var scrRef = new ScrReference(
					SantaFeFocusMessageHandler.ReceiveFocusMessage(msg), ScrVers.English);

				if (!btnReceiveScrReferences.Checked || m_fIgnoreNextRecvdSantaFeSyncMessage ||
					m_fProcessingSyncMessage)
				{
					if (m_fProcessingSyncMessage)
						m_queuedReference = scrRef;

					m_fIgnoreNextRecvdSantaFeSyncMessage = false;
					return;
				}

				ProcessReceivedMessage(scrRef);
			}

			base.WndProc(ref msg);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Refreshes the data grid when the translations change.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_helper_TranslationsChanged()
		{
			dataGridUns.Refresh();
		}

		private void dataGridUns_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 2 && m_selectKeyboard != null)
				m_selectKeyboard(true);
		}

		private void dataGridUns_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 2 && m_selectKeyboard != null)
				m_selectKeyboard(false);
		}

		private void mnuViewDebugInfo_CheckedChanged(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (!item.Checked)
				dataGridUns.Columns.Remove(m_colDebugInfo);
			else
				dataGridUns.Columns.Add(m_colDebugInfo);
		}

		private void mnuViewAnswersColumn_CheckedChanged(object sender, EventArgs e)
		{
			m_pnlAnswersAndComments.Visible = ShowAnswersAndComments;
		}

		private void dataGridUns_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			switch (e.ColumnIndex)
			{
				case 0: e.Value = m_helper[e.RowIndex].Reference; break;
				case 1: e.Value = m_helper[e.RowIndex].OriginalPhrase; break;
				case 2: e.Value = m_helper[e.RowIndex].Translation; break;
				case 3: e.Value = m_helper[e.RowIndex].HasUserTranslation; break;
				case 4: e.Value = m_helper[e.RowIndex].Parts; break;
			}
		}

		private void dataGridUns_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			switch (e.ColumnIndex)
			{
				case 2: m_helper[e.RowIndex].Translation = (string)e.Value; SaveNeeded = true;  break;
				case 3:
					m_helper[e.RowIndex].HasUserTranslation = (bool)e.Value; SaveNeeded = true;
					dataGridUns.InvalidateRow(e.RowIndex);
					break;
			}
			UpdateCountsAndFilterStatus();
		}

		private void dataGridUns_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 3)
			{
				// Force commital of the click's change.
				dataGridUns.CommitEdit(DataGridViewDataErrorContexts.Commit);
				dataGridUns.EndEdit();
			}
		}

		private void dataGridUns_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			int iClickedCol = e.ColumnIndex;
			// We want to sort it ascending unless it already was ascending.
			bool sortAscending = (dataGridUns.Columns[iClickedCol].HeaderCell.SortGlyphDirection != SortOrder.Ascending);
			if (!sortAscending)
			{
				dataGridUns.Columns[iClickedCol].HeaderCell.SortGlyphDirection = SortOrder.Descending;
			}
			else
			{
				for (int i = 0; i < dataGridUns.Columns.Count; i++)
				{
					dataGridUns.Columns[i].HeaderCell.SortGlyphDirection = (i == iClickedCol) ?
						SortOrder.Ascending : SortOrder.None;
				}
			}
			SortByColumn(iClickedCol, sortAscending);
			dataGridUns.Refresh();
		}

		private void SortByColumn(int iClickedCol, bool sortAscending)
		{
			switch (iClickedCol)
			{
				case 0: m_helper.Sort(PhraseTranslationHelper.SortBy.Reference, sortAscending); break;
				case 1: m_helper.Sort(PhraseTranslationHelper.SortBy.OriginalPhrase, sortAscending); break;
				case 2: m_helper.Sort(PhraseTranslationHelper.SortBy.Translation, sortAscending); break;
				case 3: m_helper.Sort(PhraseTranslationHelper.SortBy.Status, sortAscending); break;
				case 4: m_helper.Sort(PhraseTranslationHelper.SortBy.Default, sortAscending); break;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Enter event of the txtFilterByPart control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void txtFilterByPart_Enter(object sender, EventArgs e)
		{
			//if (m_selectKeyboard != null)
			//    m_selectKeyboard(true);
			RememberCurrentSelection();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Remembers the current selection so the same phrase and column can be re-selected
		/// after filtering.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void RememberCurrentSelection()
		{
			if (dataGridUns.CurrentRow != null && dataGridUns.CurrentCell != null)
			{
				m_currentPhrase = m_helper[dataGridUns.CurrentRow.Index];
				m_iCurrentColumn = dataGridUns.CurrentCell.ColumnIndex;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Filtering is done, so
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void txtFilterByPart_Leave(object sender, EventArgs e)
		{
			m_currentPhrase = null;
			m_iCurrentColumn = -1;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CheckChanged event of the mnuMatchWholeWords control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuMatchWholeWords_CheckChanged(object sender, EventArgs e)
		{
			if (txtFilterByPart.Text.Trim().Length > 0)
				ApplyFilter();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Applies the filter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ApplyFilter(object sender, EventArgs e)
		{
			ApplyFilter();
		}
		private void ApplyFilter()
		{
			bool clearCurrentSelection = false;
			if (m_currentPhrase == null)
			{
				RememberCurrentSelection();
				clearCurrentSelection = true;
			}
			Func<int, int, string, bool> refFilter = (m_startRef == ScrReference.Empty) ? null :
				new Func<int, int, string, bool>((start, end, sref) => m_endRef >= start && m_startRef <= end);
			dataGridUns.RowCount = 0;
			m_biblicalTermsPane.Hide();

			m_helper.Filter(txtFilterByPart.Text, MatchWholeWords, CheckedKeyTermFilterType, refFilter);
			dataGridUns.RowCount = m_helper.Phrases.Count();

			if (m_currentPhrase != null)
			{
				for (int i = 0; i < dataGridUns.Rows.Count; i++)
				{
					if (m_helper[i] == m_currentPhrase)
					{
						dataGridUns.CurrentCell = dataGridUns.Rows[i].Cells[m_iCurrentColumn];
						break;
					}
				}
				if (clearCurrentSelection)
				{
					m_currentPhrase = null;
					m_iCurrentColumn = -1;
				}
			}
			UpdateCountsAndFilterStatus();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CellDoubleClick event of the dataGridUns control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 4)
			{
				StringBuilder sbldr = new StringBuilder("Key Terms:\n");
				foreach (KeyTermMatch keyTermMatch in m_helper[e.RowIndex].GetParts().OfType<KeyTermMatch>())
				{
					foreach (string sEnglishTerm in keyTermMatch.AllTerms.Select(term => term.Term))
					{
						sbldr.Append(sEnglishTerm);
						sbldr.Append(Environment.NewLine);
					}
				}
				MessageBox.Show(sbldr.ToString(), "More Key Term Debug Info");
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Called when one of the Key Term filtering sub-menus is clicked.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void OnKeyTermsFilterChange(object sender, EventArgs e)
		{
			if (sender == mnuShowAllPhrases && mnuShowAllPhrases.Checked)
				return;

			if (!((ToolStripMenuItem)sender).Checked)
				mnuShowAllPhrases.Checked = true;
			ApplyFilter();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Called when one of the Key Term filtering sub-menus is checked.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void OnKeyTermsFilterChecked(object sender, EventArgs e)
		{
			ToolStripMenuItem clickedMenu = (ToolStripMenuItem)sender;
			if (clickedMenu.Checked)
			{
				foreach (ToolStripMenuItem menu in mnuKtFilter.DropDownItems)
				{
					if (menu != clickedMenu)
						menu.Checked = false;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the HelpButtonClicked event of the UNSQuestionsDialog control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void UNSQuestionsDialog_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
		{
			m_helpDelegate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the UNS Translation data.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void Save(object sender, EventArgs e)
		{
			Save();
		}

		private void Save()
		{
			EnsureDataFolderExists();
			XmlSerializationHelper.SerializeToFile(m_translationsFile,
				(from translatablePhrase in m_helper.UnfilteredPhrases
				 where translatablePhrase.HasUserTranslation
				 select new XmlTranslation(translatablePhrase)).ToList());
			m_lastSaveTime = DateTime.Now;
			SaveNeeded = false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the closeToolStripMenuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the mnuGenerateTemplate control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuGenerateTemplate_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(GenTemplateSettings.Folder))
				GenTemplateSettings.Folder = m_defaultLcfFolder;
			using (GenerateTemplateDlg dlg = new GenerateTemplateDlg(m_projectName,
				GenTemplateSettings, AvailableBookIds, m_sectionHeadText.AsEnumerable()))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					GenTemplateSettings = dlg.Settings;

					Func<int, int, bool> InRange;
					if (dlg.m_rdoWholeBook.Checked)
					{
						int bookNum = BCVRef.BookToNumber((string)dlg.m_cboBooks.SelectedItem);
						InRange = (bcvStart, bcvEnd) =>
						{
							return BCVRef.GetBookFromBcv(bcvStart) == bookNum;
						};
					}
					else
					{
						BCVRef startRef = dlg.VerseRangeStartRef;
						BCVRef endRef = dlg.VerseRangeEndRef;
						InRange = (bcvStart, bcvEnd) =>
						{
							return bcvStart >= startRef && bcvEnd <= endRef;
						};
					}

					List<TranslatablePhrase> allPhrasesInRange = m_helper.UnfilteredPhrases.Where(tp => tp.Category > -1 && InRange(tp.StartRef, tp.EndRef)).ToList();
					if (dlg.m_rdoDisplayWarning.Checked)
					{
						int untranslatedQuestions = allPhrasesInRange.Count(p => !p.HasUserTranslation);
						if (untranslatedQuestions > 0 &&
							MessageBox.Show(string.Format(Properties.Resources.kstidUntranslatedQuestionsWarning, untranslatedQuestions),
							m_appName, MessageBoxButtons.YesNo) == DialogResult.No)
						{
							return;
						}
					}
					using (StreamWriter sw = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
					{
						sw.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
						sw.WriteLine("<html>");
						sw.WriteLine("<head>");
						sw.WriteLine("<meta content=\"text/html; charset=UTF-8\" http-equiv=\"content-type\"/>");
						sw.WriteLine("<title>" + dlg.m_txtTitle.Text.Normalize(NormalizationForm.FormC) + "</title>");
						if (!dlg.m_rdoEmbedStyleInfo.Checked)
						{
							sw.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href= \"" + dlg.CssFile + "\"/>");
							if (dlg.WriteCssFile)
							{
								if (dlg.m_chkOverwriteCss.Checked)
								{
									using (StreamWriter css = new StreamWriter(dlg.FullCssPath))
									{
										WriteCssStyleInfo(css, dlg.m_lblQuestionGroupHeadingsColor.ForeColor,
											dlg.m_lblEnglishQuestionColor.ForeColor, dlg.m_lblEnglishAnswerTextColor.ForeColor,
											dlg.m_lblCommentTextColor.ForeColor, (int)dlg.m_numBlankLines.Value,
											dlg.m_chkNumberQuestions.Checked);
									}
								}
							}
						}

						sw.WriteLine("<style type=\"text/css\">");
						// This CSS directive always gets written directly to the template file because it's
						// important to get right and it's unlikely that someone will want to do a global override.
						sw.WriteLine(":lang(" + m_vernIcuLocale + ") {font-family:serif," +
							m_colTranslation.DefaultCellStyle.Font.FontFamily.Name + ",Arial Unicode MS;}");
						if (dlg.m_rdoEmbedStyleInfo.Checked)
						{
							WriteCssStyleInfo(sw, dlg.m_lblQuestionGroupHeadingsColor.ForeColor,
								dlg.m_lblEnglishQuestionColor.ForeColor, dlg.m_lblEnglishAnswerTextColor.ForeColor,
								dlg.m_lblCommentTextColor.ForeColor, (int)dlg.m_numBlankLines.Value,
								dlg.m_chkNumberQuestions.Checked);
						}
						sw.WriteLine("</style>");
						sw.WriteLine("</head>");
						sw.WriteLine("<body lang=\"" + m_vernIcuLocale + "\">");
						sw.WriteLine("<h1 lang=\"en\">" + dlg.m_txtTitle.Text.Normalize(NormalizationForm.FormC) + "</h1>");
						int prevCategory = -1;
						string prevSectionRef = null;
						string prevQuestionRef = null;
						string pendingSectionHead = null;

						foreach (TranslatablePhrase phrase in allPhrasesInRange)
						{
							if (phrase.Category == 0 && prevSectionRef != phrase.Reference)
							{
								if (!m_sectionHeadText.TryGetValue(phrase.Reference, out pendingSectionHead))
									pendingSectionHead = phrase.Reference;
								prevCategory = -1;
							}
							prevSectionRef = phrase.Reference;

							if (!phrase.HasUserTranslation && !dlg.m_rdoUseOriginal.Checked)
								continue; // skip this question

							if (pendingSectionHead != null)
							{
								sw.WriteLine("<h2 lang=\"en\">" + pendingSectionHead.Normalize(NormalizationForm.FormC) + "</h2>");
								pendingSectionHead = null;
							}

							if (phrase.Category != prevCategory)
							{
								sw.WriteLine("<h3>" + phrase.CategoryName.Normalize(NormalizationForm.FormC) + "</h3>");
								prevCategory = phrase.Category;
							}

							if (prevQuestionRef != phrase.Reference)
							{
								if (phrase.Category > 0 || dlg.m_chkPassageBeforeOverview.Checked)
								{
									sw.WriteLine("<p class=\"scripture\">");
									sw.WriteLine(@"\ref " + BCVRef.MakeReferenceString(phrase.StartRef, phrase.EndRef, ".", "-"));
									sw.WriteLine("</p>");
								}
								prevQuestionRef = phrase.Reference;
							}

							sw.WriteLine("<p class=\"question\">" +
								(phrase.HasUserTranslation ? phrase.Translation : phrase.OriginalPhrase).Normalize(NormalizationForm.FormC) + "</p>");

							sw.WriteLine("<div class=\"extras\" lang=\"en\">");
							if (dlg.m_chkEnglishQuestions.Checked && phrase.HasUserTranslation)
								sw.WriteLine("<p class=\"questionbt\">" + phrase.OriginalPhrase.Normalize(NormalizationForm.FormC) + "</p>");
							AnswersAndComments answersAndComments = (AnswersAndComments)phrase.AdditionalInfo[0];
							if (dlg.m_chkEnglishAnswers.Checked && answersAndComments.HasAnswer)
							{
								foreach (string answer in answersAndComments.Answers)
									sw.WriteLine("<p class=\"answer\">" + answer.Normalize(NormalizationForm.FormC) + "</p>");
							}
							if (dlg.m_chkIncludeComments.Checked && answersAndComments.HasComment)
							{
								foreach (string comment in answersAndComments.Comments)
									sw.WriteLine("<p class=\"comment\">" + comment.Normalize(NormalizationForm.FormC) + "</p>");
							}
							sw.WriteLine("</div>");
						}

						sw.WriteLine("</body>");
					}
					MessageBox.Show(Properties.Resources.kstidTemplateGenerationComplete);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Writes the CSS style info.
		/// </summary>
		/// <param name="sw">The sw.</param>
		/// <param name="questionGroupHeadingsClr">The question group headings CLR.</param>
		/// <param name="englishQuestionClr">The english question CLR.</param>
		/// <param name="englishAnswerClr">The english answer CLR.</param>
		/// <param name="commentClr">The comment CLR.</param>
		/// <param name="cBlankLines">The c blank lines.</param>
		/// <param name="fNumberQuestions">if set to <c>true</c> [f number questions].</param>
		/// ------------------------------------------------------------------------------------
		private void WriteCssStyleInfo(StreamWriter sw, Color questionGroupHeadingsClr,
			Color englishQuestionClr, Color englishAnswerClr, Color commentClr, int cBlankLines, bool fNumberQuestions)
		{
			if (fNumberQuestions)
			{
				sw.WriteLine("body {font-size:100%; counter-reset:qnum;}");
				sw.WriteLine(".question {counter-increment:qnum;}");
				sw.WriteLine("p.question:before {content:counter(qnum) \". \";}");
			}
			else
				sw.WriteLine("body {font-size:100%;}");
			sw.WriteLine("h1 {font-size:2.0em;");
			sw.WriteLine("  text-align:center}");
			sw.WriteLine("h2 {font-size:1.7em;");
			sw.WriteLine("  color:white;");
			sw.WriteLine("  background-color:black;}");
			sw.WriteLine("h3 {font-size:1.3em;");
			sw.WriteLine("  color:blue;}");
			sw.WriteLine("p {font-size:1.0em;}");
			sw.WriteLine("h1:lang(en) {font-family:sans-serif;}");
			sw.WriteLine("h2:lang(en) {font-family:serif;}");
			sw.WriteLine("p:lang(en) {font-family:serif;");
			sw.WriteLine("font-size:0.85em;}");
			sw.WriteLine("h3 {color:" + questionGroupHeadingsClr.Name + ";}");
			sw.WriteLine(".questionbt {color:" + englishQuestionClr.Name + ";}");
			sw.WriteLine(".answer {color:" + englishAnswerClr.Name + ";}");
			sw.WriteLine(".comment {color:" + commentClr.Name + ";}");
			sw.WriteLine(".extras {margin-bottom:" + cBlankLines + "em;}");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CheckedChanged event of the mnuViewToolbar control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuViewToolbar_CheckedChanged(object sender, EventArgs e)
		{
			toolStrip1.Visible = mnuViewToolbar.Checked;
			if (toolStrip1.Visible)
				m_mainMenu.SendToBack(); // this makes the toolbar appear below the menu
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CheckedChanged event of the mnuAutoSave control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuAutoSave_CheckedChanged(object sender, EventArgs e)
		{
			if (mnuAutoSave.Checked && SaveNeeded)
				Save();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the phraseSubstitutionsToolStripMenuItem control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void phraseSubstitutionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (PhraseSubstitutionsDlg dlg = new PhraseSubstitutionsDlg(m_phraseSubstitutions,
				m_helper.Phrases.Select(p => p.OriginalPhrase), dataGridUns.CurrentRow.Index))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					m_phraseSubstitutions.Clear();
					m_phraseSubstitutions.AddRange(dlg.Substitutions);
					// Save items to file
					EnsureDataFolderExists();
					XmlSerializationHelper.SerializeToFile(m_phraseSubstitutionsFile, m_phraseSubstitutions);

					Reload();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the reloadToolStripMenuItem control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Reload();
		}

		private void Reload()
		{
			using (new WaitCursor(this))
			{
				Save();
				m_helper.TranslationsChanged -= m_helper_TranslationsChanged;
				dataGridUns.RowCount = 0;
				LoadTranslations();
				ApplyFilter();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CheckedChanged event of the mnuViewBiblicalTermsPane control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuViewBiblicalTermsPane_CheckedChanged(object sender, EventArgs e)
		{
			if (mnuViewBiblicalTermsPane.Checked && dataGridUns.CurrentRow != null)
				LoadKeyTermsPane(dataGridUns.CurrentRow.Index);
			else
			{
				m_biblicalTermsPane.Hide();
				ClearBiblicalTermsPane();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the RowEnter event of the dataGridUns control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			m_lastRowEntered = e.RowIndex;
			if (mnuViewBiblicalTermsPane.Checked)
				LoadKeyTermsPane(e.RowIndex);
			if (m_pnlAnswersAndComments.Visible)
				LoadAnswerAndComment(e.RowIndex);
			if (btnSendScrReferences.Checked)
				SendScrReference(e.RowIndex);

			m_normalRowHeight = dataGridUns.Rows[e.RowIndex].Height;
			dataGridUns.AutoResizeRow(e.RowIndex);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the RowLeave event of the dataGridUns control. Resets the row height to the
		/// standard height.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_RowLeave(object sender, DataGridViewCellEventArgs e)
		{
			dataGridUns.Rows[e.RowIndex].Height = m_normalRowHeight;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Resize event of the dataGridUns control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_Resize(object sender, EventArgs e)
		{
			if (m_lastRowEntered < 0 || m_lastRowEntered >= dataGridUns.RowCount)
				return;
			int heightOfDisplayedPortionOfRow = dataGridUns.GetRowDisplayRectangle(m_lastRowEntered, true).Height;
			if (heightOfDisplayedPortionOfRow != dataGridUns.Rows[m_lastRowEntered].Height)
			{
				// Changing panel sizes have now hidden the current row. Need to scroll it into view.
				int iNewFirstRow = dataGridUns.FirstDisplayedScrollingRowIndex + 1; // bump it up at least 1 whole row
				if (heightOfDisplayedPortionOfRow == 0)
					iNewFirstRow++; // Completely hidden, so bump it up at least one more row.
				int iRow = m_lastRowEntered;
				while (iRow > 0 && !dataGridUns.Rows[--iRow].Displayed && iNewFirstRow < dataGridUns.RowCount)
					iNewFirstRow++;
				if (iNewFirstRow < dataGridUns.RowCount)
					dataGridUns.FirstDisplayedScrollingRowIndex = iNewFirstRow;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the mnuReferenceRange control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		private void mnuReferenceRange_Click(object sender, EventArgs e)
		{
			using (ScrReferenceFilterDlg dlg = new ScrReferenceFilterDlg(
				new ScrReference(m_startRef), new ScrReference(m_endRef), m_availableBookIds))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_startRef = dlg.FromRef;
					m_endRef = dlg.ToRef;
					ApplyFilter();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the SelectedIndexChanged event for one of the biblical terms list boxes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void KeyTermRenderingSelected(TermRenderingCtrl sender)
		{
			if (dataGridUns.CurrentRow == null)
				return;
			if (sender.SelectedRendering == null)
				return;
			int rowIndex = dataGridUns.CurrentRow.Index;
			m_helper[rowIndex].ReplaceKeyTermRendering(FindTermRenderingInUse(sender, rowIndex),
				sender.SelectedRendering);
			dataGridUns.InvalidateRow(rowIndex);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Need to invalidate any columns that might be showing key term renderings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void KeyTermBestRenderingsChanged()
		{
			dataGridUns.InvalidateColumn(m_colTranslation.Index);
			if (dataGridUns.ColumnCount == (m_colDebugInfo.Index + 1))
				dataGridUns.InvalidateColumn(m_colDebugInfo.Index);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CheckStateChanged event of the btnSendScrReferences control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnSendScrReferences_CheckStateChanged(object sender, EventArgs e)
		{
			if (btnSendScrReferences.Checked && dataGridUns.CurrentRow != null)
				SendScrReference(dataGridUns.CurrentRow.Index);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the VisibleChanged event of the m_pnlAnswersAndComments control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_pnlAnswersAndComments_VisibleChanged(object sender, EventArgs e)
		{
			if (m_pnlAnswersAndComments.Visible && dataGridUns.CurrentRow != null)
				LoadAnswerAndComment(dataGridUns.CurrentRow.Index);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the biblicalTermsRenderingSelectionRulesToolStripMenuItem control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void biblicalTermsRenderingSelectionRulesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (RenderingSelectionRulesDlg dlg = new RenderingSelectionRulesDlg(
				m_helper.TermRenderingSelectionRules, m_selectKeyboard))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					m_helper.TermRenderingSelectionRules = new List<RenderingSelectionRule>(dlg.Rules);
					KeyTermBestRenderingsChanged();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the EditingControlShowing event of the m_dataGridView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DataGridViewEditingControlShowingEventArgs"/> instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			TextControl = e.Control as DataGridViewTextBoxEditingControl;
			if (TextControl == null)
				return;
			TextControl.KeyDown += txtControl_KeyDown;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the CellEndEdit event of the dataGridUns control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void dataGridUns_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (TextControl != null)
				TextControl.KeyDown -= txtControl_KeyDown;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the KeyDown event of the dataGridUns control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void txtControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (dataGridUns.IsCurrentCellInEditMode &&
				dataGridUns.CurrentCell.ColumnIndex == m_colTranslation.Index &&
				(e.Modifiers & Keys.Alt) > 0 && e.Shift &&
				(e.KeyCode == Keys.Right || e.KeyCode == Keys.Left))
			{
				DataGridViewTextBoxEditingControl txt = (DataGridViewTextBoxEditingControl)dataGridUns.EditingControl;
				e.SuppressKeyPress = txt.MoveSelectedWord(e.KeyCode == Keys.Right);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the Click event of the aboutTransceleratorToolStripMenuItem control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void mnuHelpAbout_Click(object sender, EventArgs e)
		{
			// Get copyright information from assembly info. By doing this we don't have
			// to update the splash screen each year.
			string copyRight;
			var assembly = Assembly.GetExecutingAssembly();
			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if (attributes.Length > 0)
				copyRight = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			else
			{
				// if we can't find it in the assembly info, use generic one (which might be out of date)
				copyRight = "Copyright (c) 2011, SIL International";
			}
			copyRight = string.Format(Properties.Resources.kstidCopyrightFmt, copyRight.Replace("(C)", "�"));
			MessageBox.Show("Transcelerator is in alpha. Currently under development by Tom Bogle." +
				Environment.NewLine + copyRight + Environment.NewLine + Environment.NewLine +
				"Distributable under the terms of either the Common Public License or the GNU Lesser General Public License" + Environment.NewLine +
				Environment.NewLine + "Some icons were downloaded from http://www.iconfinder.com and are covered by their respective licenses:" +
				Environment.NewLine + "The Add Rule icon was developed by Yusuke Kamiyamane and is covered by this Creative Commons License: http://creativecommons.org/licenses/by/3.0/" +
				Environment.NewLine + "The Copy Rule icon was developed by Momenticons and is covered by this Creative Commons Licence: http://creativecommons.org/licenses/by/3.0/" +
				Environment.NewLine + "The Delete Rule icon was developed by Rodolphe and is covered by the GNU General Public License: http://www.gnu.org/copyleft/gpl.html",
				"Transcelerator");
		}
		#endregion

		#region Private helper methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Ensures the data folder exists.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static void EnsureDataFolderExists()
		{
			if (!Directory.Exists(s_unsDataFolder))
				Directory.CreateDirectory(s_unsDataFolder);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the translations.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void LoadTranslations()
		{
			Exception e;
			KeyTermRules rules = XmlSerializationHelper.DeserializeFromFile<KeyTermRules>(Path.Combine(s_unsDataFolder, "keyTermRules.xml"), out e);
			if (e != null)
				MessageBox.Show(e.ToString(), Text);

			QuestionProvider qp = new QuestionProvider(m_questionsFilename);
			m_helper = new PhraseTranslationHelper(qp, m_keyTerms, rules, m_phraseSubstitutions);
			m_helper.KeyTermRenderingRulesFile = Path.Combine(s_unsDataFolder, string.Format("Term rendering selection rules - {0}.xml", m_projectName));
			m_sectionHeadText = qp.SectionHeads;
			m_availableBookIds = qp.AvailableBookIds;
			if (File.Exists(m_translationsFile))
			{
				List<XmlTranslation> translations = XmlSerializationHelper.DeserializeFromFile<List<XmlTranslation>>(m_translationsFile, out e);
				if (e != null)
				{
					MessageBox.Show(e.ToString());
				}
				else
				{
					foreach (XmlTranslation unsTranslation in translations)
					{
						TranslatablePhrase phrase = m_helper.GetPhrase(unsTranslation.Reference, unsTranslation.OriginalPhrase);
						if (phrase != null) // unlikely, but an happen if master list is modified
							phrase.Translation = unsTranslation.Translation;
					}
				}
			}
			m_helper.ProcessAllTranslations();
			m_helper.TranslationsChanged += m_helper_TranslationsChanged;

			dataGridUns.RowCount = m_helper.Phrases.Count();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the key terms pane.
		/// </summary>
		/// <param name="rowIndex">Index of the row to load for.</param>
		/// ------------------------------------------------------------------------------------
		private void LoadKeyTermsPane(int rowIndex)
		{
			m_biblicalTermsPane.SuspendLayout();
			ClearBiblicalTermsPane();
			int col = 0;
			Dictionary<KeyTermMatch, int> previousKeyTermEndOfRenderingOffsets = new Dictionary<KeyTermMatch, int>();
			foreach (KeyTermMatch keyTerm in m_helper[rowIndex].GetParts().OfType<KeyTermMatch>())
			{
				IEnumerable<string> renderings = keyTerm.Renderings;
				if (renderings.Any())
				{
					int ichEndRenderingOfPreviousOccurrenceOfThisSameKeyTerm;
					previousKeyTermEndOfRenderingOffsets.TryGetValue(keyTerm, out ichEndRenderingOfPreviousOccurrenceOfThisSameKeyTerm);
					TermRenderingCtrl ktRenderCtrl = new TermRenderingCtrl(keyTerm,
						ichEndRenderingOfPreviousOccurrenceOfThisSameKeyTerm, m_selectKeyboard);
					ktRenderCtrl.VernacularFont = m_vernFont;

					SubstringDescriptor sd = FindTermRenderingInUse(ktRenderCtrl, rowIndex);
					if (sd == null)
					{
						// Didn't find any renderings for this term in the translation, so don't select anything
						previousKeyTermEndOfRenderingOffsets[keyTerm] = m_helper[rowIndex].Translation.Length;
					}
					else
					{
						previousKeyTermEndOfRenderingOffsets[keyTerm] = sd.EndOffset;
						ktRenderCtrl.SelectedRendering = m_helper[rowIndex].Translation.Substring(sd.Offset, sd.Length);
					}
					ktRenderCtrl.Dock = DockStyle.Fill;
					m_biblicalTermsPane.Controls.Add(ktRenderCtrl, col, 0);
					ktRenderCtrl.SelectedRenderingChanged += KeyTermRenderingSelected;
					ktRenderCtrl.BestRenderingsChanged += KeyTermBestRenderingsChanged;
					col++;
				}
			}
			m_biblicalTermsPane.ColumnCount = col;
			ResizeKeyTermPaneColumns();
			m_biblicalTermsPane.Visible = m_biblicalTermsPane.ColumnCount > 0;
			m_biblicalTermsPane.ResumeLayout();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Resizes the key term pane columns.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ResizeKeyTermPaneColumns()
		{
			int columnsToFit = m_biblicalTermsPane.ColumnCount;
			if (columnsToFit == 0)
				return;
			int colWidth = m_biblicalTermsPane.ClientSize.Width / columnsToFit;
			m_biblicalTermsPane.ColumnStyles.Clear();
			for (int iCol = 0; iCol < columnsToFit; iCol++)
			{
				m_biblicalTermsPane.ColumnStyles.Add(new ColumnStyle(
					SizeType.Percent, colWidth));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the counts and filter status.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void UpdateCountsAndFilterStatus()
		{
			if (m_helper.FilteredPhraseCount == m_helper.UnfilteredPhraseCount)
			{
				lblFilterIndicator.Text = (string)lblFilterIndicator.Tag;
				lblFilterIndicator.Image = null;
			}
			else
			{
				lblFilterIndicator.Text = Properties.Resources.kstidFilteredStatus;
				lblFilterIndicator.Image = Properties.Resources.Filtered;
			}
			lblRemainingWork.Text = string.Format((string)lblRemainingWork.Tag,
				m_helper.Phrases.Count(p => !p.HasUserTranslation), m_helper.FilteredPhraseCount);
			lblRemainingWork.Visible = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Clears the biblical terms pane.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ClearBiblicalTermsPane()
		{
			foreach (TermRenderingCtrl ctrl in m_biblicalTermsPane.Controls.OfType<TermRenderingCtrl>())
				ctrl.SelectedRenderingChanged -= KeyTermRenderingSelected;
			m_biblicalTermsPane.Controls.Clear();
			m_biblicalTermsPane.ColumnCount = 0;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Finds the term rendering (from the known ones in the renderingInfo) in use in
		/// the current translation.
		/// </summary>
		/// <param name="renderingInfo">The information about a single occurrence of a key
		/// biblical term and its rendering in a string in the target language.</param>
		/// <param name="rowIndex">Index of the row.</param>
		/// <returns>An object that indicates where in the translation string the match was
		/// found (offset and length)</returns>
		/// ------------------------------------------------------------------------------------
		private SubstringDescriptor FindTermRenderingInUse(ITermRenderingInfo renderingInfo, int rowIndex)
		{
			return m_helper[rowIndex].FindTermRenderingInUse(renderingInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sends the start reference for the given row as a Santa-Fe "focus" message.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void SendScrReference(int iRow)
		{
			m_fIgnoreNextRecvdSantaFeSyncMessage = true;
			BCVRef currRef = GetScrRefOfRow(iRow);
			if (currRef != null && currRef.Valid)
				SantaFeFocusMessageHandler.SendFocusMessage(currRef.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Processes the received sync message.
		/// </summary>
		/// <param name="reference">The reference in English versification scheme.</param>
		/// ------------------------------------------------------------------------------------
		private void ProcessReceivedMessage(ScrReference reference)
		{
			// While we process the given reference we might get additional synch events, the
			// most recent of which we store in m_queuedReference. If we're done
			// and we have a new reference in m_queuedReference we process that one, etc.
			for (; reference != null; reference = m_queuedReference)
			{
				m_queuedReference = null;
				m_fProcessingSyncMessage = true;

				try
				{
					if (reference.Valid && (dataGridUns.CurrentRow == null ||
						!QuestionCoversRef(dataGridUns.CurrentRow.Index, reference)))
					{
						GoToReference(reference);
					}
				}
				finally
				{
					m_fProcessingSyncMessage = false;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether the question in the given row covers the given Scripture
		/// reference.
		/// </summary>
		/// <param name="iRow">The index of the row</param>
		/// <param name="reference">The reference.</param>
		/// ------------------------------------------------------------------------------------
		internal bool QuestionCoversRef(int iRow, ScrReference reference)
		{
			string sRef = dataGridUns.Rows[iRow].Cells[m_colReference.Index].Value as string;
			BCVRef bcvStartRef, bcvEndRef;
			QuestionProvider.ParseRefRange(sRef, out bcvStartRef, out bcvEndRef);
			return reference >= bcvStartRef && reference <= bcvEndRef;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes to the first row in the data grid corresponding to the given reference.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void GoToReference(ScrReference reference)
		{
			for (int iRow = 0; iRow < dataGridUns.Rows.Count; iRow++)
			{
				if (QuestionCoversRef(iRow, reference))
				{
					dataGridUns.CurrentCell = dataGridUns.Rows[iRow].Cells[dataGridUns.CurrentCell.ColumnIndex];
					return;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the answer and comment labels for the given row.
		/// </summary>
		/// <param name="rowIndex">Index of the row.</param>
		/// ------------------------------------------------------------------------------------
		private void LoadAnswerAndComment(int rowIndex)
		{
			object[] addlInfo = m_helper[rowIndex].AdditionalInfo;
			if (addlInfo == null || addlInfo.Length == 0)
			{
				m_lblAnswerLabel.Visible = m_lblAnswers.Visible = false;
				m_lblCommentLabel.Visible = m_lblComments.Visible = false;
				return;
			}
			AnswersAndComments answersAndComments = (AnswersAndComments)addlInfo[0];
			PopulateAnswerOrCommentLabel(answersAndComments.Answers, m_lblAnswerLabel,
				m_lblAnswers, Properties.Resources.kstidAnswersLabel);
			PopulateAnswerOrCommentLabel(answersAndComments.Comments, m_lblCommentLabel,
				m_lblComments, Properties.Resources.kstidCommentsLabel);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Populates the answer or comment label.
		/// </summary>
		/// <param name="details">The list of answers or comments.</param>
		/// <param name="label">The label that has the "Answer:" or "Comment:" label.</param>
		/// <param name="contents">The label that is to be populated with the actual answer(s)
		/// or comment(s).</param>
		/// <param name="sLabelMultiple">The text to use for <see cref="label"/> if there are
		/// multiple answers/comments.</param>
		/// ------------------------------------------------------------------------------------
		private static void PopulateAnswerOrCommentLabel(IEnumerable<string> details,
			Label label, Label contents, string sLabelMultiple)
		{
			label.Visible = contents.Visible = details.Any();
			if (label.Visible)
			{
				label.Show();
				label.Text = (details.Count() == 1) ? (string)label.Tag : sLabelMultiple;
				contents.Text = details.ToString("\r\n\t");
			}
		}
		#endregion
	}
	#endregion

	#region class SubstringDescriptor
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Simple class to allow methods to pass an offset and a length in order to descibe a
	/// substring.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class SubstringDescriptor
	{
		public int Offset { get; set; }
		public int Length { get; set; }

		public int EndOffset
		{
			get { return Offset + Length; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="SubstringDescriptor"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public SubstringDescriptor(int offset, int length)
		{
			Offset = offset;
			Length = length;
		}
	}
	#endregion
}