namespace ScottPlotCookbook.Recipes.Introduction;

public class Legend : ICategory
{
    public Chapter Chapter => Chapter.General;
    public string CategoryName => "Legends";
    public string CategoryDescription => "A legend is a key typically displayed in the corner of a plot";

    public class LegendQuickstart : RecipeBase
    {
        public override string Name => "Legend Quickstart";
        public override string Description => "Many plottables have a Label property " +
            "that can be set so they appear in the legend.";

        [Test]
        public override void Execute()
        {
            var sig1 = myPlot.Add.Signal(Generate.Sin(51));
            sig1.LegendText = "Sin";

            var sig2 = myPlot.Add.Signal(Generate.Cos(51));
            sig2.LegendText = "Cos";

            myPlot.ShowLegend();
        }
    }

    public class LegendWithRTLText : RecipeBase
    {
        public override string Name => "Right To Left (RTL) text support";
        public override string Description => "Enabling Right To Left (RTL) support " +
            "correctly renders text in RTL languages.";

        [Test]
        public override void Execute()
        {
            LabelStyle.RTLSupport = true; // enable right-to-left text support

            var sig1 = myPlot.Add.Signal(Generate.Sin(51));
            sig1.LegendText = "אמת"; // example right-to-left text

            var sig2 = myPlot.Add.Signal(Generate.Cos(51));
            sig2.LegendText = "English"; // example left-to-right text

            myPlot.ShowLegend();
        }
    }

    public class ManualLegend : RecipeBase
    {
        public override string Name => "Manual Legend Items";
        public override string Description => "Legends may be constructed manually.";

        [Test]
        public override void Execute()
        {
            myPlot.Add.Signal(Generate.Sin(51));
            myPlot.Add.Signal(Generate.Cos(51));
            myPlot.Legend.IsVisible = true;

            LegendItem item1 = new()
            {
                LineColor = Colors.Magenta,
                MarkerFillColor = Colors.Magenta,
                MarkerLineColor = Colors.Magenta,
                LineWidth = 2,
                LabelText = "Alpha"
            };

            LegendItem item2 = new()
            {
                LineColor = Colors.Green,
                MarkerFillColor = Colors.Green,
                MarkerLineColor = Colors.Green,
                LineWidth = 4,
                LabelText = "Beta"
            };

            LegendItem[] items = { item1, item2 };
            myPlot.ShowLegend(items);
        }
    }

    public class LegendStyle : RecipeBase
    {
        public override string Name => "Legend Customization";
        public override string Description => "Access the Legend object directly " +
            "for advanced customization options.";

        [Test]
        public override void Execute()
        {
            var sig1 = myPlot.Add.Signal(Generate.Sin(51));
            sig1.LegendText = "Sin";

            var sig2 = myPlot.Add.Signal(Generate.Cos(51));
            sig2.LegendText = "Cos";

            myPlot.Legend.IsVisible = true;
            myPlot.Legend.Alignment = Alignment.UpperCenter;

            myPlot.Legend.OutlineColor = Colors.Navy;
            myPlot.Legend.OutlineWidth = 5;
            myPlot.Legend.BackgroundColor = Colors.LightBlue;

            myPlot.Legend.ShadowColor = Colors.Blue.WithOpacity(.2);
            myPlot.Legend.ShadowOffset = new(10, 10);

            myPlot.Legend.FontSize = 32;
            myPlot.Legend.FontName = Fonts.Serif;
        }
    }

    public class LegendOrientation : RecipeBase
    {
        public override string Name => "Legend Orientation";
        public override string Description => "Legend items may be arranged horizontally instead of vertically";

        [Test]
        public override void Execute()
        {
            var sig1 = myPlot.Add.Signal(Generate.Sin(51, phase: .2));
            var sig2 = myPlot.Add.Signal(Generate.Sin(51, phase: .4));
            var sig3 = myPlot.Add.Signal(Generate.Sin(51, phase: .6));

            sig1.LegendText = "Signal 1";
            sig2.LegendText = "Signal 2";
            sig3.LegendText = "Signal 3";

            myPlot.ShowLegend(Alignment.UpperLeft, Orientation.Horizontal);
        }
    }

    public class LegendWrapping : RecipeBase
    {
        public override string Name => "Legend Wrapping";
        public override string Description => "Legend items may wrap to improve display for a large number of items";

        [Test]
        public override void Execute()
        {
            for (int i = 1; i <= 10; i++)
            {
                double[] data = Generate.Sin(51, phase: .02 * i);
                var sig = myPlot.Add.Signal(data);
                sig.LegendText = $"#{i}";
            }

            myPlot.Legend.IsVisible = true;
            myPlot.Legend.Orientation = Orientation.Horizontal;
        }
    }

    public class LegendMultiple : RecipeBase
    {
        public override string Name => "Multiple Legends";
        public override string Description => "Multiple legends may be added to a plot";

        [Test]
        public override void Execute()
        {
            for (int i = 1; i <= 5; i++)
            {
                double[] data = Generate.Sin(51, phase: .02 * i);
                var sig = myPlot.Add.Signal(data);
                sig.LegendText = $"Signal #{i}";
                sig.LineWidth = 2;
            }

            // default legend
            var leg1 = myPlot.ShowLegend();
            leg1.Alignment = Alignment.LowerRight;
            leg1.Orientation = Orientation.Vertical;

            // additional legend
            var leg2 = myPlot.Add.Legend();
            leg2.Alignment = Alignment.UpperCenter;
            leg2.Orientation = Orientation.Horizontal;
        }
    }

    public class LegendOutside : RecipeBase
    {
        public override string Name => "Legend Outside the Plot";
        public override string Description => "Use the ShowLegend() overload that accepts " +
            "an Edge to display the legend outside the data area.";

        [Test]
        public override void Execute()
        {
            var sig1 = myPlot.Add.Signal(Generate.Sin());
            var sig2 = myPlot.Add.Signal(Generate.Cos());

            sig1.LegendText = "Sine";
            sig2.LegendText = "Cosine";

            myPlot.ShowLegend(Edge.Right);
        }
    }

    private static string GetFontsBasePath()
    {
        return Path.Combine(Paths.RepoFolder, @"src/ScottPlot5/ScottPlot5 Demos/ScottPlot5 WinForms Demo/Fonts");
    }

    public class LegendCustomFontAutomaticItems : RecipeBase
    {
        public override string Name => "Automatic Legend Items Custom Font";
        public override string Description => "Use custom fonts from TTF files in the legend.";

        [Test]
        public override void Execute()
        {
            Fonts.AddFontFile("Alumni Sans", Path.Combine(GetFontsBasePath(), @"AlumniSans/AlumniSans-Regular.ttf"), bold: false, italic: false);

            var sig1 = myPlot.Add.Signal(Generate.Sin(51));
            sig1.LegendText = "Sin";

            var sig2 = myPlot.Add.Signal(Generate.Cos(51));
            sig2.LegendText = "Cos";

            myPlot.Legend.FontName = "Alumni Sans";
            myPlot.Legend.FontSize = 48;
            myPlot.Legend.FontColor = Colors.Red;

            myPlot.ShowLegend();
        }
    }

    public class LegendCustomFontManualItems : RecipeBase
    {
        public override string Name => "Manual Legend Items Custom Font";
        public override string Description => "Use custom fonts from TTF files in the legend (manual legend items).";

        [Test]
        public override void Execute()
        {
            // Add a font file to use its typeface for fonts with a given name
            Fonts.AddFontFile(
                name: "Alumni Sans",
                path: Path.Combine(Paths.FontFolder, @"AlumniSans/AlumniSans-Regular.ttf"));

            var sig1 = myPlot.Add.Signal(Generate.Sin(51));
            sig1.LegendText = "Sin";

            var sig2 = myPlot.Add.Signal(Generate.Cos(51));
            sig2.LegendText = "Cos";

            myPlot.Legend.ManualItems.Add(new LegendItem()
            {
                LabelText = "Custom",
                LabelFontName = "Alumni Sans",
                LabelFontSize = 18,
                LabelFontColor = Colors.Magenta,
                LinePattern = LinePattern.Dotted,
                LineWidth = 2,
                LineColor = Colors.Magenta,
            });
        }
    }
}
