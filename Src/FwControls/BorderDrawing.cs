// Copyright (c) 2002-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace SIL.FieldWorks.Common.Controls
{
	/// <summary />
	[ToolboxBitmap(typeof(FwButton), "resources.BorderDrawing.bmp")]
	internal sealed class BorderDrawing : Component
	{
		/// <summary />
		private Rectangle m_rect;
		/// <summary />
		private BorderTypes m_brdrType = BorderTypes.Single;
		/// <summary />
		private Pen m_penLightestEdge = new Pen(SystemColors.ControlLightLight, 1);
		/// <summary />
		private Pen m_penLightEdge = new Pen(SystemColors.ControlLight, 1);
		/// <summary />
		private Pen m_penDarkestEdge = new Pen(SystemColors.ControlDarkDark, 1);
		/// <summary />
		private Pen m_penDarkEdge = new Pen(SystemColors.ControlDark, 1);

		#region Constructors

		/// <summary />
		internal BorderDrawing()
		{
			Graphics = null;
		}
		#endregion

		/// <summary />
		private bool IsDisposed { get; set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType() + ". ******");
			if (IsDisposed)
			{
				// No need to run it more than once.
				return;
			}

			base.Dispose(disposing);

			Graphics = null;
			m_penLightestEdge = null;
			m_penLightEdge = null;
			m_penDarkestEdge = null;
			m_penDarkEdge = null;

			IsDisposed = true;
		}

		#region Properties

		/// <summary>
		/// Gets or sets the graphics object.
		/// </summary>
		[Browsable(false)]
		internal Graphics Graphics { get; set; }

		/// <summary>
		/// Gets or sets the color used to draw border style Single.
		/// </summary>
		[Description("Determines the color used to draw border style Single.")]
		internal Color SingleBorderColor { get; set; } = SystemColors.ControlDark;

		/// <summary>
		///  Allow setting individual edge colors for raised and sunken borders.
		/// </summary>
		internal Color BorderLightestColor
		{
			get => m_penLightestEdge.Color;
			set => m_penLightestEdge.Color = value;
		}

		/// <summary>
		///  Allow setting individual edge colors for raised and sunken borders.
		/// </summary>
		internal Color BorderLightColor
		{
			get => m_penLightEdge.Color;
			set => m_penLightEdge.Color = value;
		}

		/// <summary>
		///  Allow setting individual edge colors for raised and sunken borders.
		/// </summary>
		internal Color BorderDarkColor
		{
			get => m_penDarkEdge.Color;
			set => m_penDarkEdge.Color = value;
		}

		/// <summary>
		///  Allow setting individual edge colors for raised and sunken borders.
		/// </summary>
		internal Color BorderDarkestColor
		{
			get => m_penDarkestEdge.Color;
			set => m_penDarkestEdge.Color = value;
		}
		#endregion

		#region ShouldSerialize methods

		/// <summary>
		/// Returns true if value is different from Default value
		/// </summary>
		private bool ShouldSerializeBorderDarkColor()
		{
			return BorderDarkColor != SystemColors.ControlDark;
		}

		/// <summary>
		/// Returns true if value is different from Default value
		/// </summary>
		private bool ShouldSerializeBorderDarkestColor()
		{
			return BorderDarkestColor != SystemColors.ControlDarkDark;
		}

		/// <summary>
		/// Returns true if value is different from Default value
		/// </summary>
		private bool ShouldSerializeBorderLightestColor()
		{
			return BorderLightestColor != SystemColors.ControlLightLight;
		}

		/// <summary>
		/// Returns true if value is different from Default value
		/// </summary>
		private bool ShouldSerializeBorderLightColor()
		{
			return BorderLightColor != SystemColors.ControlLight;
		}

		/// <summary>
		/// Returns true if value is different from Default value
		/// </summary>
		private bool ShouldSerializeSingleBorderColor()
		{
			return SingleBorderColor != SystemColors.ControlDark;
		}
		#endregion

		/// <summary />
		internal void Draw(Graphics g, Rectangle rect, BorderTypes brdrType)
		{
			Graphics = g;
			m_rect = rect;
			m_brdrType = brdrType;
			Draw();
		}

		/// <summary>
		/// Draws this instance.
		/// </summary>
		private void Draw()
		{
			if (Graphics == null)
			{
				throw (new ArgumentNullException());
			}

			m_rect.Width--;
			m_rect.Height--;

			// Single border
			if (m_brdrType == BorderTypes.Single)
			{
				using (var penDarkEdge = new Pen(SingleBorderColor, 1))
				{
					Graphics.DrawRectangle(penDarkEdge, m_rect);
					return;
				}
			}

			// Single border, raised or sunken
			if (m_brdrType == BorderTypes.SingleRaised || m_brdrType == BorderTypes.SingleSunken)
			{
				// First, draw the dark border all around the rectangle.
				Graphics.DrawRectangle(m_penDarkEdge, m_rect);
				// Then draw the two light edges where they should appear.
				if (m_brdrType == BorderTypes.SingleRaised)
				{
					// Note: left and top border line are one pixel shorter!
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left, m_rect.Top, m_rect.Left, m_rect.Bottom - 1);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left, m_rect.Top, m_rect.Right - 1, m_rect.Top);
				}
				else
				{
					Graphics.DrawLine(m_penLightestEdge, m_rect.Right, m_rect.Top, m_rect.Right, m_rect.Bottom);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left, m_rect.Bottom, m_rect.Right, m_rect.Bottom);
				}
			}

			// Double border raised or sunken
			else
			{
				// Draw the dark and darkest border all around the rectangle. One inside the other.
				Graphics.DrawRectangle(m_penDarkestEdge, m_rect);
				Graphics.DrawRectangle(m_penDarkEdge, m_rect.X + 1, m_rect.Y + 1, m_rect.Width - 2, m_rect.Height - 2);

				if (m_brdrType == BorderTypes.DoubleRaised)
				{
					// Note: left and top border line are one pixel shorter!
					Graphics.DrawLine(m_penLightEdge, m_rect.Left, m_rect.Top, m_rect.Left, m_rect.Bottom - 1);
					Graphics.DrawLine(m_penLightEdge, m_rect.Left, m_rect.Top, m_rect.Right - 1, m_rect.Top);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left + 1, m_rect.Top + 1, m_rect.Left + 1, m_rect.Bottom - 2);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left + 1, m_rect.Top + 1, m_rect.Right - 2, m_rect.Top + 1);
				}
				else
				{
					// DoubleSunken
					Graphics.DrawLine(m_penLightEdge, m_rect.Right, m_rect.Top, m_rect.Right, m_rect.Bottom);
					Graphics.DrawLine(m_penLightEdge, m_rect.Left, m_rect.Bottom, m_rect.Right, m_rect.Bottom);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Right - 1, m_rect.Top + 1, m_rect.Right - 1, m_rect.Bottom - 1);
					Graphics.DrawLine(m_penLightestEdge, m_rect.Left + 1, m_rect.Bottom - 1, m_rect.Right - 1, m_rect.Bottom - 1);
				}
			}
		}
	}
}