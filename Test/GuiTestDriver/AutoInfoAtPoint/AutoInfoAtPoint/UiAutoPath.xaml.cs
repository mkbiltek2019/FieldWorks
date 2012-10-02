﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Collections;

namespace AutoInfoAtPoint
{
	/// <summary>
	/// Interaction logic for UiAutoPath.xaml
	/// </summary>
	public partial class UiAutoPath : Window
	{
		private System.Windows.Point releasePoint;

		public UiAutoPath()
		{
			InitializeComponent();
			string fileloc = @"C:\fwr\Test\GuiTestDriver\FwUiAutomationProviders\FwUiAutomationProviders\bin\Debug\FwUiAutomationProviders.dll";
			Assembly a = null;
			try { a = Assembly.LoadFile(fileloc); }
			catch (FileNotFoundException e1) { label1.Content = e1.Message; }
			if (a != null)
			{
				try { ClientSettings.RegisterClientSideProviderAssembly(a.GetName()); }
				catch (ProxyAssemblyNotLoadedException e) {label1.Content = e.Message;}
			}
		}

		/// <summary>
		/// Generated by right-click mouse press.
		/// </summary>
		/// <param name="sender">System.Windows.Controls.Button</param>
		/// <param name="e">MouseDown</param>
		private void button1_MouseDown(object sender, MouseButtonEventArgs e)
		{
			label1.Content = sender.GetType().ToString() + ": " + e.RoutedEvent.Name;
			button1.CaptureMouse();
		}

		/// <summary>
		/// Generated by right-click mouse release.
		/// </summary>
		/// <param name="sender">System.Windows.Controls.Button</param>
		/// <param name="e">MouseUp</param>
		private void button1_MouseUp(object sender, MouseButtonEventArgs e)
		{	// this window is inside the window frame
			releasePoint = e.GetPosition(this); // relative to this window
			releasePoint.Offset(appWin.Left + 4, appWin.Top + 30);
			label1.Content = sender.GetType().ToString() + ": " + e.RoutedEvent.Name
				+ " (" + releasePoint.X + "," + releasePoint.Y + ")";
			button1.ReleaseMouseCapture();

			// Get the UI Element under the releasePoint and
			// navigate up the tree to the main window.
			AutomationElement pathTarget = AutomationElement.FromPoint(releasePoint);
			if (pathTarget == null) label1.Content = "No AutomationElement found :-(";
			else
			{ // traverse to the root and record the path.
				//AutomationElement desktop = AutomationElement.RootElement; // the desktop
				//AutomationElement ae = pathTarget.CachedParent;
				//DependencyObject depo = VisualTreeHelper.GetParent(v);
				//LocalizedControlType is a short control desc. like button, spinner, group, title bar, etc..
				// label1.Content = pathTarget.Current.ControlType.LocalizedControlType; // getBestPattern(pathTarget);
				string pathImage = pathToDesktop(pathTarget);
				ArrayList wrappedImage = wrapImage(pathImage);
				foreach (string line in wrappedImage) label1.Content += "\nl" + line;

				AutomationPattern[] aps = pathTarget.GetSupportedPatterns();
				foreach (AutomationPattern ap in aps)
				{
					label1.Content += "\nl" + ap.Id + ap.ProgrammaticName;
				}
				AutomationProperty[] apr = pathTarget.GetSupportedProperties();
				foreach (AutomationProperty ap in apr)
				{
					label1.Content += "\nl" + ap.Id + ap.ProgrammaticName;
					//object oValue = pathTarget.GetCurrentPropertyValue(ap);

					// get the name of the prop and switch to its getter
					// or cast object to the right type and add to the string

					// What are the possible property values sorted by type?
					string str = pathTarget.Current.AcceleratorKey;
					str = pathTarget.Current.AccessKey;
					str = pathTarget.Current.AutomationId;
					Rect rect = pathTarget.Current.BoundingRectangle;
					str = pathTarget.Current.ClassName;
					ControlType ct = pathTarget.Current.ControlType;
					str = pathTarget.Current.FrameworkId;
					bool boo = pathTarget.Current.HasKeyboardFocus;
					str = pathTarget.Current.HelpText;
					boo = pathTarget.Current.IsContentElement;
					boo = pathTarget.Current.IsControlElement;
					boo = pathTarget.Current.IsEnabled;
					boo = pathTarget.Current.IsKeyboardFocusable;
					boo = pathTarget.Current.IsOffscreen;
					boo = pathTarget.Current.IsPassword;
					boo = pathTarget.Current.IsRequiredForForm;
					str = pathTarget.Current.ItemStatus;
					str = pathTarget.Current.ItemType;
					AutomationElement ae = pathTarget.Current.LabeledBy;
					str = pathTarget.Current.LocalizedControlType;
					str = pathTarget.Current.Name;
					int hwnd = pathTarget.Current.NativeWindowHandle;
					OrientationType ot = pathTarget.Current.Orientation;
					int procId = pathTarget.Current.ProcessId;
				}
				// don't need to clear the clipboard
				System.Windows.Clipboard.SetText(pathImage);
			}
		}

		private ArrayList wrapImage(string pathImage)
		{
			ArrayList wrapped = new ArrayList();
			if (pathImage == null || pathImage.Length == 0) return null;
			const int k_chars = 100;
			int size = pathImage.Length;
			for (int l = 0; l < 1 + size / k_chars; l++)
			{
				int start = l * k_chars;
				int charsLeft = size - start;
				if (charsLeft >= k_chars) charsLeft = k_chars;
				string line = pathImage.Substring(start, charsLeft);
				wrapped.Add(line);
			}
			return wrapped;
		}

		/// <summary>
		/// Bores to the desktop creating a path image.
		/// The control name ex: pane:/window:C:\WINDOWS/Dialog:Folder Options/tab:/tab item:View/tree:Advanced settings:/tree item:Files and Folders/tree item:Do not cache thumbnails/
		/// Note, nameless controls have no name! Dialog windows are Dialogs and children of their apps!
		/// </summary>
		/// <param name="rootElement">The root of the search on this iteration.</param>
		/// <param name="treeNode">The node in the TreeView for this iteration.</param>
		/// <returns></returns>
		private string pathToDesktop(AutomationElement rootElement)
		{
			string pathImage = "";
			if (rootElement != null)
			{
				AutomationElement elementNode = TreeWalker.ControlViewWalker.GetParent(rootElement);
				if (elementNode != null)
				{
					pathImage = pathToDesktop(elementNode);
				}
				pathImage += rootElement.Current.LocalizedControlType + ":"
					+ rootElement.Current.Name + "/";
			}
			return pathImage;
		}

		/*private readonly int[,] bestPattern = new int[18, 2]
		{ { 10015, 1 }, { 10003, 2 }, { 10014, 3 }, { 10002, 4 }, { 10005, 5 },
		{ 10010, 6 }, { 10001, 7 }, { 10013, 8 }, { 10012, 9 }, { 10017, 10 },
		{ 10004, 11 }, { 10000, 12 }, { 10007, 13 }, { 10006, 14 }, { 10008, 15 },
		{ 10016, 16 }, { 10011, 17 }, { 10009, 18 } }; */
		private readonly int[] bestPattern = new int[18]
		{ 12, 7, 4, 2, 11, 5, 14, 13, 15, 18, 6, 17, 9, 8, 3, 1, 16, 10 };

		/// <summary>
		/// Gets the most useful pattern from the element to use in the test path.
		/// </summary>
		/// <param name="rootElement"></param>
		/// <returns></returns>
		private string getBestPattern(AutomationElement rootElement)
		{
			string pathImage = "";
			AutomationPattern[] aps = rootElement.GetSupportedPatterns();
			if (aps.Length == 0) pathImage += "none";
			else if (aps.Length == 1) pathImage += aps[0].ProgrammaticName;
			else
			{
				string best = "";
				int bestWeight = 9999;
				foreach (AutomationPattern ap in aps)
				{
					int ind = ap.Id - 10000;
					int w = bestPattern[ind];
					if (w < bestWeight)
					{
						bestWeight = w;
						best = ap.ProgrammaticName;
					}
				}
				pathImage = best;
			}
			return pathImage.Replace("Pattern", null).Replace("Identifiers.", null) + ":";
		}

		/// <summary>
		/// A left-click while holding the right-click down.
		/// Pass the click to the control - it's captured by this control.
		/// </summary>
		/// <param name="sender">System.Windows.Controls.Button</param>
		/// <param name="e">Click</param>
		/*private void button1_Click(object sender, RoutedEventArgs e)
		{
			// can't seem to find a way to get the location of the left click
			// always comes up with the UIAutoPath button coords instaed of the left-click while right-clicking
			Object over = e.OriginalSource;
			RoutedEvent re = e.RoutedEvent;
			string name = re.Name;
			Type t = re.OwnerType;
			Type ht = re.HandlerType;
			RoutingStrategy rs = re.RoutingStrategy;
			Visual control = over as Visual;
			Point youClickedHere = control.PointToScreen(new Point(0, 0));
			//GeneralTransform transform = control.TransformToAncestor(this); // desktop?
			//Point currentPoint = transform.Transform(new Point(0, 0));
			//Vector vector = VisualTreeHelper.GetOffset(control); // offset from parent only
			//Point currentPoint = new Point(vector.X, vector.Y);
			AutomationElement pathTarget = AutomationElement.FromPoint(youClickedHere);
			//thing = pathTarget == pathTarget2;
		}*/
	}

	/*
	 * Control Pattern Class      Provider Interface       Description
	 *
	 * 10000 InvokePattern  IInvokeProvider
	 * Used for controls that can be invoked, such as a button.
	 *
	 * 10001 SelectionPattern  ISelectionProvider
	 * Used for selection container controls.
	 * For example, list boxes and combo boxes.
	 *
	 * 10002 ValuePattern  IValueProvider
	 * Allows clients to get or set a value on controls that do not support
	 * a range of values.
	 * For example, a date time picker.
	 *
	 * 10003 RangeValuePattern  IRangeValueProvider
	 * Used for controls that have a range of values that can be applied
	 * to the control.
	 * For example, a spinner control containing years might have a
	 * range of 1900 to 2010, while another spinner control presenting
	 * months would have a range of 1 to 12.
	 *
	 * 10004 ScrollPattern  IScrollProvider
	 * Used for controls that can scroll.
	 * For example, a control that has scroll bars that are active when
	 * there is more information than can be displayed in the viewable area
	 * of the control.
	 *
	 * 10005 ExpandCollapsePattern  IExpandCollapseProvider
	 * Used for controls that can be expanded or collapsed.
	 * For example, menu items in an application such as the File menu.
	 *
	 * 10006 GridPattern  IGridProvider
	 * Used for controls that support grid functionality such as sizing
	 * and moving to a specified cell.
	 * For example, the large icon view in Windows Explorer or
	 * simple tables without headers in Microsoft Word.
	 *
	 * 10007 GridItemPattern  IGridItemProvider
	 * Used for controls that have cells within grids.
	 * The individual cells should support the GridItem pattern.
	 * For example, each cell in Microsoft Windows Explorer detail view.
	 *
	 * 10008 MultipleViewPattern  IMultipleViewProvider
	 * Used for controls that can switch between multiple representations
	 * of the same set of information, data, or children.
	 * For example, a list view control where data is available in thumbnail,
	 * tile, icon, list, or detail views.
	 *
	 * 10009 WindowPattern  IWindowProvider
	 * Exposes information specific to windows, a fundamental concept
	 * to the Microsoft Windows operating system.
	 * Examples of controls that are windows are top-level application
	 * windows (Microsoft Word, Microsoft Windows Explorer, and so on),
	 * multiple-document interface (MDI) child windows, and dialogs.
	 *
	 * 10010 SelectionItemPattern  ISelectionItemProvider
	 * Used for individual items in selection container controls,
	 * such as list boxes and combo boxes.
	 *
	 * 10011 DockPattern  IDockProvider
	 * Used for controls that can be docked in a docking container.
	 * For example, toolbars or tool palettes.
	 *
	 * 10012 TablePattern  ITableProvider
	 * Used for controls that have a grid as well as header information.
	 * For example, Microsoft Excel worksheets.
	 *
	 * 10013 TableItemPattern  ITableItemProvider
	 * Used for items in a table.
	 *
	 * 10014 TextPattern  ITextProvider
	 * Used for edit controls and documents that expose textual information.
	 *
	 * 10015 TogglePattern  IToggleProvider
	 * Used for controls where the state can be toggled.
	 * For example, check boxes and checkable menu items.
	 *
	 * 10016 TransformPattern  ITransformProvider
	 * Used for controls that can be resized, moved, and rotated.
	 * Typical uses for the Transform control pattern are in designers,
	 * forms, graphical editors, and drawing applications.
	 *
	 * 10017 ScrollItemPattern  IScrollItemProvider
	 * Used for controls that have individual items in a list that scrolls.
	 * For example, a list control that has individual items in the scroll list, such as a combo box control.
	 *
10000 InvokePattern
10001 SelectionPattern
10002 ValuePattern
10003 RangeValuePattern
10004 ScrollPattern
10005 ExpandCollapsePattern
10006 GridPattern
10007 GridItemPattern
10008 MultipleViewPattern
10009 WindowPattern
10010 SelectionItemPattern
10011 DockPattern
10012 TablePattern
10013 TableItemPattern
10014 TextPattern
10015 TogglePattern
10016 TransformPattern
10017 ScrollItemPattern
	 */

}
