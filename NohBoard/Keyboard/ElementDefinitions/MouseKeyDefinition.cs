﻿/*
Copyright (C) 2016 by Eric Bataille <e.c.p.bataille@gmail.com>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace ThoNohT.NohBoard.Keyboard.ElementDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.Serialization;
    using ClipperLib;
    using Extra;

    /// <summary>
    /// Represents a key on a mouse.
    /// </summary>
    [DataContract(Name = "MouseKey", Namespace = "")]
    public class MouseKeyDefinition : KeyDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseKeyDefinition" /> class.
        /// </summary>
        /// <param name="id">The identifier of the key.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="keyCode">The keycode.</param>
        /// <param name="text">The text of the key.</param>
        /// <remarks>The position of the text is determined from the bounding box of the key.</remarks>
        public MouseKeyDefinition(int id, List<TPoint> boundaries, int keyCode, string text)
            : base(id, boundaries, keyCode, text)
        {
        }

        /// <summary>
        /// Renders the key in the specified surface.
        /// </summary>
        /// <param name="g">The GDI+ surface to render on.</param>
        /// <param name="pressed">A value indicating whether to render the key in its pressed state or not.</param>
        /// <param name="shift">A value indicating whether shift is pressed during the render.</param>
        /// <param name="capsLock">A value indicating whether caps lock is pressed during the render.</param>
        public void Render(Graphics g, bool pressed, bool shift, bool capsLock)
        {
            var txtSize = g.MeasureString(this.Text, new Font(FontFamily.GenericMonospace, 10));
            var txtPoint = new TPoint(
                this.TextPosition.X - (int)(txtSize.Width / 2),
                this.TextPosition.Y - (int)(txtSize.Height / 2));

            var backColor = pressed
                ? GlobalSettings.CurrentStyle.DefaultKeyStyle.BackgroundPressed
                : GlobalSettings.CurrentStyle.DefaultKeyStyle.BackgroundLoose;
            var textColor = pressed
                ? GlobalSettings.CurrentStyle.DefaultKeyStyle.TextPressed
                : GlobalSettings.CurrentStyle.DefaultKeyStyle.TextLoose;
            var outlineColor = pressed
                ? GlobalSettings.CurrentStyle.DefaultKeyStyle.OutlinePressed
                : GlobalSettings.CurrentStyle.DefaultKeyStyle.OutlineLoose;
            var drawOutline = (pressed && GlobalSettings.CurrentStyle.DefaultKeyStyle.ShowOutlinePressed) ||
                              (!pressed && GlobalSettings.CurrentStyle.DefaultKeyStyle.ShowOutlineLoose);

            // Draw the background
            g.FillPolygon(new SolidBrush(backColor), this.Boundaries.ConvertAll<Point>(x => x).ToArray());

            // Draw the text
            g.DrawString(
                this.Text,
                new Font(FontFamily.GenericMonospace, 10),
                new SolidBrush(textColor),
                (Point)txtPoint);

            // Draw the outline.
            if (drawOutline)
                g.DrawPolygon(new Pen(outlineColor, 1), this.Boundaries.ConvertAll<Point>(x => x).ToArray());
        }

        /// <summary>
        /// Checks whether this key overlaps with another specified key definition.
        /// </summary>
        /// <param name="otherKey">The other key to check for overlapping on.</param>
        /// <returns><c>True</c> if the keys overlap, <c>false</c> otherwise.</returns>
        public bool BordersWith(MouseKeyDefinition otherKey)
        {
            var clipper = new Clipper();

            clipper.AddPath(this.GetPath(), PolyType.ptSubject, true);
            clipper.AddPath(otherKey.GetPath(), PolyType.ptClip, true);

            var union = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, union);

            return union.Count == 1;
        }

        #region Transformations

        /// <summary>
        /// Translates the element, moving it the specified distance.
        /// </summary>
        /// <param name="dx">The distance along the x-axis.</param>
        /// <param name="dy">The distance along the y-axis.</param>
        /// <returns>A new <see cref="ElementDefinition"/> that is translated.</returns>
        public override ElementDefinition Translate(int dx, int dy)
        {
            return new MouseKeyDefinition(
                this.Id,
                this.Boundaries.Select(b => b.Translate(dx, dy)).ToList(),
                this.KeyCode,
                this.Text);
        }

        /// <summary>
        /// Updates the key definition to occupy a region of itself plus the specified other keys.
        /// </summary>
        /// <param name="keys">The keys to union with.</param>
        /// <returns>A new key definition with the updated region.</returns>
        public override KeyDefinition UnionWith(List<KeyDefinition> keys)
        {
            return this.UnionWith(keys.ConvertAll(x => (MouseKeyDefinition)x));
        }

        #endregion Transformations

        #region Private methods

        /// <summary>
        /// Updates the key definition to occupy a region of itself plus the specified other keys.
        /// </summary>
        /// <param name="keys">The keys to union with.</param>
        /// <returns>A new key definition with the updated region.</returns>
        private MouseKeyDefinition UnionWith(IList<MouseKeyDefinition> keys)
        {
            var newBoundaries = this.Boundaries.Select(b => new TPoint(b.X, b.Y)).ToList();

            if (keys.Any())
            {
                var cl = new Clipper();
                cl.AddPath(this.GetPath(), PolyType.ptSubject, true);
                cl.AddPaths(keys.Select(x => x.GetPath()).ToList(), PolyType.ptClip, true);
                var union = new List<List<IntPoint>>();
                cl.Execute(ClipType.ctUnion, union);

                if (union.Count > 1)
                    throw new ArgumentException("Cannot union two non-overlapping keys.");

                newBoundaries = union.Single().ConvertAll<TPoint>(x => x);
            }

            return new MouseKeyDefinition(this.Id, newBoundaries, this.KeyCode, this.Text);
        }
        
        #endregion Private methods
    }
}