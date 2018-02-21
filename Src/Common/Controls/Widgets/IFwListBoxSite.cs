// Copyright (c) 2003-2018 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System.Drawing;

namespace SIL.FieldWorks.Common.Widgets
{
	internal interface IFwListBoxSite : IHighlightInfo, IWritingSystemAndStylesheet
	{
		Color ForeColor { set; get; }
	}
}