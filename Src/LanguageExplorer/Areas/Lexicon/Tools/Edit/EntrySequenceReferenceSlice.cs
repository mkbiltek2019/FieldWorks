// Copyright (c) 2004-2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using LanguageExplorer.Controls.DetailControls;

namespace LanguageExplorer.Areas.Lexicon.Tools.Edit
{
	/// <summary>
	/// Provides a class that uses the Go dlg for selecting entries for subentries.
	/// </summary>
	internal class EntrySequenceReferenceSlice : CustomReferenceVectorSlice
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntrySequenceReferenceSlice"/> class.
		/// </summary>
		public EntrySequenceReferenceSlice()
			: base(new EntrySequenceReferenceLauncher())
		{
		}
	}
}