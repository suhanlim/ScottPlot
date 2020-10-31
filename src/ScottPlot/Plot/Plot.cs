﻿/* The Plot class is the primary public interface for ScottPlot.
 * State (plottables and configuration) is stored in the settings object 
 * which is private so it can be refactored without breaking the API.
 * 
 * Helper methods for styling and plottable creation are in partial class
 * files in the Plot folder.
 */

using ScottPlot.Drawing;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScottPlot
{
    public partial class Plot
    {
        private readonly Settings settings;

        public Plot(int width = 800, int height = 600)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("width and height must each be greater than 0");
            settings = new Settings();
            StyleTools.SetStyle(this, ScottPlot.Style.Default);
            Resize(width, height);
        }

        public override string ToString() =>
            $"ScottPlot ({settings.Width}x{settings.Height}) with {Plottables.Length:n0} plot objects";

        /// <summary>
        /// Return a new Plot with all the same Plottables (and some of the styles) of this one
        /// </summary>
        public Plot Copy()
        {
            Plot plt2 = new Plot(settings.Width, settings.Height);
            var settings2 = plt2.GetSettings(false);
            settings2.Plottables.AddRange(settings.Plottables);

            // TODO: add a Copy() method to the settings module, or perhaps Update(existingSettings).

            // copy over only the most relevant styles
            plt2.Title(settings.XAxis2.Title);
            plt2.XLabel(settings.XAxis.Title);
            plt2.YLabel(settings.YAxis.Title);

            plt2.AxisAuto();
            return plt2;
        }

        public void Resize(int? width = null, int? height = null)
        {
            if (width == null)
                width = settings.Width;
            if (height == null)
                height = settings.Height;

            if (width < 1)
                width = 1;
            if (height < 1)
                height = 1;

            settings.Resize((int)width, (int)height);
        }

        public Bitmap Render(Bitmap renderOnThis)
        {
            RenderBitmap(renderOnThis);
            return renderOnThis;
        }

        private Bitmap RenderBitmap(bool lowQuality = false) =>
             RenderBitmap(settings.Width, settings.Height);

        private Bitmap RenderBitmap(int width, int height, bool lowQuality = false) =>
            RenderBitmap(new Bitmap(width, height, PixelFormat.Format32bppPArgb));

        private bool NeedsAutoLayout = true;
        private Bitmap RenderBitmap(Bitmap bmp, bool lowQuality = false)
        {
            settings.BenchmarkMessage.Restart();
            RenderLegacyLayoutAdjustment();

            settings.Resize(bmp.Width, bmp.Height);
            settings.AutoSizeLayout();

            if (NeedsAutoLayout)
            {
                // on first layout two adjustments are requied: 
                // one for gross tick placement, and another for string measurement
                settings.Resize(bmp.Width, bmp.Height);
                settings.AutoSizeLayout();
                NeedsAutoLayout = false;
            }

            var dims = settings.Dimensions;
            RenderBeforePlottables(dims, bmp, lowQuality);
            RenderPlottables(dims, bmp, lowQuality);
            RenderAfterPlottables(dims, bmp, lowQuality);
            return bmp;
        }

        private void RenderLegacyLayoutAdjustment()
        {
            if (!settings.axes.hasBeenSet)
                settings.AxisAuto();
            else
                settings.axes.ApplyBounds();

            TightenLayout();
        }

        private void RenderBeforePlottables(PlotDimensions dims, Bitmap bmp, bool lowQuality)
        {
            settings.FigureBackground.Render(dims, bmp, lowQuality);
            settings.DataBackground.Render(dims, bmp, lowQuality);

            settings.ticks.x.Recalculate(settings);
            settings.XAxis.SetTicks(settings.ticks.x.tickPositionsMajor, settings.ticks.x.tickLabels, settings.ticks.x.tickPositionsMinor);
            settings.XAxis.Render(dims, bmp, lowQuality: false);

            settings.ticks.y.Recalculate(settings);
            settings.YAxis.SetTicks(settings.ticks.y.tickPositionsMajor, settings.ticks.y.tickLabels, settings.ticks.y.tickPositionsMinor);
            settings.YAxis.Render(dims, bmp, lowQuality: false);

            settings.XAxis2.Render(dims, bmp, lowQuality: false);
            settings.YAxis2.Render(dims, bmp, lowQuality: false);
        }

        private void RenderPlottables(PlotDimensions dims, Bitmap bmp, bool lowQuality)
        {
            var plottablesToRender = settings.Plottables.Where(x => x.IsVisible);
            foreach (var plottable in plottablesToRender)
            {
                try
                {
                    plottable.Render(dims, bmp, lowQuality);
                }
                catch (OverflowException)
                {
                    Debug.WriteLine($"OverflowException plotting: {plottable}");
                }
            }
        }

        private void RenderAfterPlottables(PlotDimensions dims, Bitmap bmp, bool lowQuality)
        {
            settings.CornerLegend.UpdateLegendItems(Plottables);
            settings.CornerLegend.Render(dims, bmp, lowQuality);

            settings.BenchmarkMessage.Stop();
            // TODO: set up validation check reporting
            //ErrorMessage.Text = "Error Message";

            settings.ZoomRectangle.Render(dims, bmp, lowQuality);
            settings.BenchmarkMessage.Render(dims, bmp, lowQuality);
            settings.ErrorMessage.Render(dims, bmp, lowQuality);
        }

        public Bitmap GetBitmap(bool renderFirst = true, bool lowQuality = false)
        {
            if (NeedsAutoLayout)
                RenderBitmap();

            return RenderBitmap();
        }

        public void SaveFig(string filePath, bool renderFirst = true)
        {
            Bitmap bmp = RenderBitmap();

            filePath = System.IO.Path.GetFullPath(filePath);
            string fileFolder = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(fileFolder))
                throw new Exception($"ERROR: folder does not exist: {fileFolder}");

            ImageFormat imageFormat;
            string extension = System.IO.Path.GetExtension(filePath).ToUpper();
            if (extension == ".JPG" || extension == ".JPEG")
                imageFormat = ImageFormat.Jpeg; // TODO: use jpgEncoder to set custom compression level
            else if (extension == ".PNG")
                imageFormat = ImageFormat.Png;
            else if (extension == ".TIF" || extension == ".TIFF")
                imageFormat = ImageFormat.Tiff;
            else if (extension == ".BMP")
                imageFormat = ImageFormat.Bmp;
            else
                throw new NotImplementedException("Extension not supported: " + extension);

            bmp.Save(filePath, imageFormat);
        }

        public void Add(IRenderable plottable)
        {
            settings.Plottables.Add(plottable);
        }

        [Obsolete("Access the 'Plot.Plottables' array instead", true)]
        public List<IRenderable> GetPlottables() => settings.Plottables;
        public IRenderable[] Plottables { get => settings.Plottables.ToArray(); }

        public List<IDraggable> GetDraggables()
        {
            List<IDraggable> draggables = new List<IDraggable>();

            foreach (var plottable in Plottables)
                if (plottable is IDraggable draggable)
                    draggables.Add(draggable);

            return draggables;
        }

        public IDraggable GetDraggableUnderMouse(double pixelX, double pixelY, int snapDistancePixels = 5)
        {
            double snapWidth = GetSettings(false).xAxisUnitsPerPixel * snapDistancePixels;
            double snapHeight = GetSettings(false).yAxisUnitsPerPixel * snapDistancePixels;

            foreach (IDraggable draggable in GetDraggables())
                if (draggable.IsUnderMouse(CoordinateFromPixelX(pixelX), CoordinateFromPixelY(pixelY), snapWidth, snapHeight))
                    if (draggable.DragEnabled)
                        return draggable;

            return null;
        }
    }
}
