namespace Imago.Support.Drawing;

/// <summary>
/// Provides access to the colors in the official Tailwind CSS v4 color palette.
/// </summary>
public static class TailwindColors
{
    /// <summary>
    /// Parses the specified hex color string.
    /// </summary>
    /// <param name="hex">The hex color string.</param>
    /// <returns>The parsed color.</returns>
    private static Color GetColor(string hex) => new Color(hex);

    /// <summary>
    /// Gets the Tailwind color with the value of #000000.
    /// </summary>
    public static Color Black { get; } = GetColor("#000000");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffffff.
    /// </summary>
    public static Color White { get; } = GetColor("#ffffff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f8fafc.
    /// </summary>
    public static Color Slate50 { get; } = GetColor("#f8fafc");

    /// <summary>
    /// Gets the Tailwind color with the value of #f1f5f9.
    /// </summary>
    public static Color Slate100 { get; } = GetColor("#f1f5f9");

    /// <summary>
    /// Gets the Tailwind color with the value of #e2e8f0.
    /// </summary>
    public static Color Slate200 { get; } = GetColor("#e2e8f0");

    /// <summary>
    /// Gets the Tailwind color with the value of #cad5e2.
    /// </summary>
    public static Color Slate300 { get; } = GetColor("#cad5e2");

    /// <summary>
    /// Gets the Tailwind color with the value of #90a1b9.
    /// </summary>
    public static Color Slate400 { get; } = GetColor("#90a1b9");

    /// <summary>
    /// Gets the Tailwind color with the value of #62748e.
    /// </summary>
    public static Color Slate500 { get; } = GetColor("#62748e");

    /// <summary>
    /// Gets the Tailwind color with the value of #45556c.
    /// </summary>
    public static Color Slate600 { get; } = GetColor("#45556c");

    /// <summary>
    /// Gets the Tailwind color with the value of #314158.
    /// </summary>
    public static Color Slate700 { get; } = GetColor("#314158");

    /// <summary>
    /// Gets the Tailwind color with the value of #1d293d.
    /// </summary>
    public static Color Slate800 { get; } = GetColor("#1d293d");

    /// <summary>
    /// Gets the Tailwind color with the value of #0f172b.
    /// </summary>
    public static Color Slate900 { get; } = GetColor("#0f172b");

    /// <summary>
    /// Gets the Tailwind color with the value of #020618.
    /// </summary>
    public static Color Slate950 { get; } = GetColor("#020618");

    /// <summary>
    /// Gets the Tailwind color with the value of #f9fafb.
    /// </summary>
    public static Color Gray50 { get; } = GetColor("#f9fafb");

    /// <summary>
    /// Gets the Tailwind color with the value of #f3f4f6.
    /// </summary>
    public static Color Gray100 { get; } = GetColor("#f3f4f6");

    /// <summary>
    /// Gets the Tailwind color with the value of #e5e7eb.
    /// </summary>
    public static Color Gray200 { get; } = GetColor("#e5e7eb");

    /// <summary>
    /// Gets the Tailwind color with the value of #d1d5dc.
    /// </summary>
    public static Color Gray300 { get; } = GetColor("#d1d5dc");

    /// <summary>
    /// Gets the Tailwind color with the value of #99a1af.
    /// </summary>
    public static Color Gray400 { get; } = GetColor("#99a1af");

    /// <summary>
    /// Gets the Tailwind color with the value of #6a7282.
    /// </summary>
    public static Color Gray500 { get; } = GetColor("#6a7282");

    /// <summary>
    /// Gets the Tailwind color with the value of #4a5565.
    /// </summary>
    public static Color Gray600 { get; } = GetColor("#4a5565");

    /// <summary>
    /// Gets the Tailwind color with the value of #364153.
    /// </summary>
    public static Color Gray700 { get; } = GetColor("#364153");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e2939.
    /// </summary>
    public static Color Gray800 { get; } = GetColor("#1e2939");

    /// <summary>
    /// Gets the Tailwind color with the value of #101828.
    /// </summary>
    public static Color Gray900 { get; } = GetColor("#101828");

    /// <summary>
    /// Gets the Tailwind color with the value of #030712.
    /// </summary>
    public static Color Gray950 { get; } = GetColor("#030712");

    /// <summary>
    /// Gets the Tailwind color with the value of #fafafa.
    /// </summary>
    public static Color Zinc50 { get; } = GetColor("#fafafa");

    /// <summary>
    /// Gets the Tailwind color with the value of #f4f4f5.
    /// </summary>
    public static Color Zinc100 { get; } = GetColor("#f4f4f5");

    /// <summary>
    /// Gets the Tailwind color with the value of #e4e4e7.
    /// </summary>
    public static Color Zinc200 { get; } = GetColor("#e4e4e7");

    /// <summary>
    /// Gets the Tailwind color with the value of #d4d4d8.
    /// </summary>
    public static Color Zinc300 { get; } = GetColor("#d4d4d8");

    /// <summary>
    /// Gets the Tailwind color with the value of #9f9fa9.
    /// </summary>
    public static Color Zinc400 { get; } = GetColor("#9f9fa9");

    /// <summary>
    /// Gets the Tailwind color with the value of #71717b.
    /// </summary>
    public static Color Zinc500 { get; } = GetColor("#71717b");

    /// <summary>
    /// Gets the Tailwind color with the value of #52525c.
    /// </summary>
    public static Color Zinc600 { get; } = GetColor("#52525c");

    /// <summary>
    /// Gets the Tailwind color with the value of #3f3f46.
    /// </summary>
    public static Color Zinc700 { get; } = GetColor("#3f3f46");

    /// <summary>
    /// Gets the Tailwind color with the value of #27272a.
    /// </summary>
    public static Color Zinc800 { get; } = GetColor("#27272a");

    /// <summary>
    /// Gets the Tailwind color with the value of #18181b.
    /// </summary>
    public static Color Zinc900 { get; } = GetColor("#18181b");

    /// <summary>
    /// Gets the Tailwind color with the value of #09090b.
    /// </summary>
    public static Color Zinc950 { get; } = GetColor("#09090b");

    /// <summary>
    /// Gets the Tailwind color with the value of #fafafa.
    /// </summary>
    public static Color Neutral50 { get; } = GetColor("#fafafa");

    /// <summary>
    /// Gets the Tailwind color with the value of #f5f5f5.
    /// </summary>
    public static Color Neutral100 { get; } = GetColor("#f5f5f5");

    /// <summary>
    /// Gets the Tailwind color with the value of #e5e5e5.
    /// </summary>
    public static Color Neutral200 { get; } = GetColor("#e5e5e5");

    /// <summary>
    /// Gets the Tailwind color with the value of #d4d4d4.
    /// </summary>
    public static Color Neutral300 { get; } = GetColor("#d4d4d4");

    /// <summary>
    /// Gets the Tailwind color with the value of #a1a1a1.
    /// </summary>
    public static Color Neutral400 { get; } = GetColor("#a1a1a1");

    /// <summary>
    /// Gets the Tailwind color with the value of #737373.
    /// </summary>
    public static Color Neutral500 { get; } = GetColor("#737373");

    /// <summary>
    /// Gets the Tailwind color with the value of #525252.
    /// </summary>
    public static Color Neutral600 { get; } = GetColor("#525252");

    /// <summary>
    /// Gets the Tailwind color with the value of #404040.
    /// </summary>
    public static Color Neutral700 { get; } = GetColor("#404040");

    /// <summary>
    /// Gets the Tailwind color with the value of #262626.
    /// </summary>
    public static Color Neutral800 { get; } = GetColor("#262626");

    /// <summary>
    /// Gets the Tailwind color with the value of #171717.
    /// </summary>
    public static Color Neutral900 { get; } = GetColor("#171717");

    /// <summary>
    /// Gets the Tailwind color with the value of #0a0a0a.
    /// </summary>
    public static Color Neutral950 { get; } = GetColor("#0a0a0a");

    /// <summary>
    /// Gets the Tailwind color with the value of #fafaf9.
    /// </summary>
    public static Color Stone50 { get; } = GetColor("#fafaf9");

    /// <summary>
    /// Gets the Tailwind color with the value of #f5f5f4.
    /// </summary>
    public static Color Stone100 { get; } = GetColor("#f5f5f4");

    /// <summary>
    /// Gets the Tailwind color with the value of #e7e5e4.
    /// </summary>
    public static Color Stone200 { get; } = GetColor("#e7e5e4");

    /// <summary>
    /// Gets the Tailwind color with the value of #d6d3d1.
    /// </summary>
    public static Color Stone300 { get; } = GetColor("#d6d3d1");

    /// <summary>
    /// Gets the Tailwind color with the value of #a6a09b.
    /// </summary>
    public static Color Stone400 { get; } = GetColor("#a6a09b");

    /// <summary>
    /// Gets the Tailwind color with the value of #79716b.
    /// </summary>
    public static Color Stone500 { get; } = GetColor("#79716b");

    /// <summary>
    /// Gets the Tailwind color with the value of #57534d.
    /// </summary>
    public static Color Stone600 { get; } = GetColor("#57534d");

    /// <summary>
    /// Gets the Tailwind color with the value of #44403b.
    /// </summary>
    public static Color Stone700 { get; } = GetColor("#44403b");

    /// <summary>
    /// Gets the Tailwind color with the value of #292524.
    /// </summary>
    public static Color Stone800 { get; } = GetColor("#292524");

    /// <summary>
    /// Gets the Tailwind color with the value of #1c1917.
    /// </summary>
    public static Color Stone900 { get; } = GetColor("#1c1917");

    /// <summary>
    /// Gets the Tailwind color with the value of #0c0a09.
    /// </summary>
    public static Color Stone950 { get; } = GetColor("#0c0a09");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef2f2.
    /// </summary>
    public static Color Red50 { get; } = GetColor("#fef2f2");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffe2e2.
    /// </summary>
    public static Color Red100 { get; } = GetColor("#ffe2e2");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffc9c9.
    /// </summary>
    public static Color Red200 { get; } = GetColor("#ffc9c9");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffa2a2.
    /// </summary>
    public static Color Red300 { get; } = GetColor("#ffa2a2");

    /// <summary>
    /// Gets the Tailwind color with the value of #ff6467.
    /// </summary>
    public static Color Red400 { get; } = GetColor("#ff6467");

    /// <summary>
    /// Gets the Tailwind color with the value of #fb2c36.
    /// </summary>
    public static Color Red500 { get; } = GetColor("#fb2c36");

    /// <summary>
    /// Gets the Tailwind color with the value of #e7000b.
    /// </summary>
    public static Color Red600 { get; } = GetColor("#e7000b");

    /// <summary>
    /// Gets the Tailwind color with the value of #c10007.
    /// </summary>
    public static Color Red700 { get; } = GetColor("#c10007");

    /// <summary>
    /// Gets the Tailwind color with the value of #9f0712.
    /// </summary>
    public static Color Red800 { get; } = GetColor("#9f0712");

    /// <summary>
    /// Gets the Tailwind color with the value of #82181a.
    /// </summary>
    public static Color Red900 { get; } = GetColor("#82181a");

    /// <summary>
    /// Gets the Tailwind color with the value of #460809.
    /// </summary>
    public static Color Red950 { get; } = GetColor("#460809");

    /// <summary>
    /// Gets the Tailwind color with the value of #fff7ed.
    /// </summary>
    public static Color Orange50 { get; } = GetColor("#fff7ed");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffedd4.
    /// </summary>
    public static Color Orange100 { get; } = GetColor("#ffedd4");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffd6a7.
    /// </summary>
    public static Color Orange200 { get; } = GetColor("#ffd6a7");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffb86a.
    /// </summary>
    public static Color Orange300 { get; } = GetColor("#ffb86a");

    /// <summary>
    /// Gets the Tailwind color with the value of #ff8904.
    /// </summary>
    public static Color Orange400 { get; } = GetColor("#ff8904");

    /// <summary>
    /// Gets the Tailwind color with the value of #ff6900.
    /// </summary>
    public static Color Orange500 { get; } = GetColor("#ff6900");

    /// <summary>
    /// Gets the Tailwind color with the value of #f54900.
    /// </summary>
    public static Color Orange600 { get; } = GetColor("#f54900");

    /// <summary>
    /// Gets the Tailwind color with the value of #ca3500.
    /// </summary>
    public static Color Orange700 { get; } = GetColor("#ca3500");

    /// <summary>
    /// Gets the Tailwind color with the value of #9f2d00.
    /// </summary>
    public static Color Orange800 { get; } = GetColor("#9f2d00");

    /// <summary>
    /// Gets the Tailwind color with the value of #7e2a0c.
    /// </summary>
    public static Color Orange900 { get; } = GetColor("#7e2a0c");

    /// <summary>
    /// Gets the Tailwind color with the value of #441306.
    /// </summary>
    public static Color Orange950 { get; } = GetColor("#441306");

    /// <summary>
    /// Gets the Tailwind color with the value of #fffbeb.
    /// </summary>
    public static Color Amber50 { get; } = GetColor("#fffbeb");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef3c6.
    /// </summary>
    public static Color Amber100 { get; } = GetColor("#fef3c6");

    /// <summary>
    /// Gets the Tailwind color with the value of #fee685.
    /// </summary>
    public static Color Amber200 { get; } = GetColor("#fee685");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffd230.
    /// </summary>
    public static Color Amber300 { get; } = GetColor("#ffd230");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffb900.
    /// </summary>
    public static Color Amber400 { get; } = GetColor("#ffb900");

    /// <summary>
    /// Gets the Tailwind color with the value of #fe9a00.
    /// </summary>
    public static Color Amber500 { get; } = GetColor("#fe9a00");

    /// <summary>
    /// Gets the Tailwind color with the value of #e17100.
    /// </summary>
    public static Color Amber600 { get; } = GetColor("#e17100");

    /// <summary>
    /// Gets the Tailwind color with the value of #bb4d00.
    /// </summary>
    public static Color Amber700 { get; } = GetColor("#bb4d00");

    /// <summary>
    /// Gets the Tailwind color with the value of #973c00.
    /// </summary>
    public static Color Amber800 { get; } = GetColor("#973c00");

    /// <summary>
    /// Gets the Tailwind color with the value of #7b3306.
    /// </summary>
    public static Color Amber900 { get; } = GetColor("#7b3306");

    /// <summary>
    /// Gets the Tailwind color with the value of #461901.
    /// </summary>
    public static Color Amber950 { get; } = GetColor("#461901");

    /// <summary>
    /// Gets the Tailwind color with the value of #fefce8.
    /// </summary>
    public static Color Yellow50 { get; } = GetColor("#fefce8");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef9c2.
    /// </summary>
    public static Color Yellow100 { get; } = GetColor("#fef9c2");

    /// <summary>
    /// Gets the Tailwind color with the value of #fff085.
    /// </summary>
    public static Color Yellow200 { get; } = GetColor("#fff085");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffdf20.
    /// </summary>
    public static Color Yellow300 { get; } = GetColor("#ffdf20");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdc700.
    /// </summary>
    public static Color Yellow400 { get; } = GetColor("#fdc700");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0b100.
    /// </summary>
    public static Color Yellow500 { get; } = GetColor("#f0b100");

    /// <summary>
    /// Gets the Tailwind color with the value of #d08700.
    /// </summary>
    public static Color Yellow600 { get; } = GetColor("#d08700");

    /// <summary>
    /// Gets the Tailwind color with the value of #a65f00.
    /// </summary>
    public static Color Yellow700 { get; } = GetColor("#a65f00");

    /// <summary>
    /// Gets the Tailwind color with the value of #894b00.
    /// </summary>
    public static Color Yellow800 { get; } = GetColor("#894b00");

    /// <summary>
    /// Gets the Tailwind color with the value of #733e0a.
    /// </summary>
    public static Color Yellow900 { get; } = GetColor("#733e0a");

    /// <summary>
    /// Gets the Tailwind color with the value of #432004.
    /// </summary>
    public static Color Yellow950 { get; } = GetColor("#432004");

    /// <summary>
    /// Gets the Tailwind color with the value of #f7fee7.
    /// </summary>
    public static Color Lime50 { get; } = GetColor("#f7fee7");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfcca.
    /// </summary>
    public static Color Lime100 { get; } = GetColor("#ecfcca");

    /// <summary>
    /// Gets the Tailwind color with the value of #d8f999.
    /// </summary>
    public static Color Lime200 { get; } = GetColor("#d8f999");

    /// <summary>
    /// Gets the Tailwind color with the value of #bbf451.
    /// </summary>
    public static Color Lime300 { get; } = GetColor("#bbf451");

    /// <summary>
    /// Gets the Tailwind color with the value of #9ae600.
    /// </summary>
    public static Color Lime400 { get; } = GetColor("#9ae600");

    /// <summary>
    /// Gets the Tailwind color with the value of #7ccf00.
    /// </summary>
    public static Color Lime500 { get; } = GetColor("#7ccf00");

    /// <summary>
    /// Gets the Tailwind color with the value of #5ea500.
    /// </summary>
    public static Color Lime600 { get; } = GetColor("#5ea500");

    /// <summary>
    /// Gets the Tailwind color with the value of #497d00.
    /// </summary>
    public static Color Lime700 { get; } = GetColor("#497d00");

    /// <summary>
    /// Gets the Tailwind color with the value of #3c6300.
    /// </summary>
    public static Color Lime800 { get; } = GetColor("#3c6300");

    /// <summary>
    /// Gets the Tailwind color with the value of #35530e.
    /// </summary>
    public static Color Lime900 { get; } = GetColor("#35530e");

    /// <summary>
    /// Gets the Tailwind color with the value of #192e03.
    /// </summary>
    public static Color Lime950 { get; } = GetColor("#192e03");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0fdf4.
    /// </summary>
    public static Color Green50 { get; } = GetColor("#f0fdf4");

    /// <summary>
    /// Gets the Tailwind color with the value of #dcfce7.
    /// </summary>
    public static Color Green100 { get; } = GetColor("#dcfce7");

    /// <summary>
    /// Gets the Tailwind color with the value of #b9f8cf.
    /// </summary>
    public static Color Green200 { get; } = GetColor("#b9f8cf");

    /// <summary>
    /// Gets the Tailwind color with the value of #7bf1a8.
    /// </summary>
    public static Color Green300 { get; } = GetColor("#7bf1a8");

    /// <summary>
    /// Gets the Tailwind color with the value of #05df72.
    /// </summary>
    public static Color Green400 { get; } = GetColor("#05df72");

    /// <summary>
    /// Gets the Tailwind color with the value of #00c950.
    /// </summary>
    public static Color Green500 { get; } = GetColor("#00c950");

    /// <summary>
    /// Gets the Tailwind color with the value of #00a63e.
    /// </summary>
    public static Color Green600 { get; } = GetColor("#00a63e");

    /// <summary>
    /// Gets the Tailwind color with the value of #008236.
    /// </summary>
    public static Color Green700 { get; } = GetColor("#008236");

    /// <summary>
    /// Gets the Tailwind color with the value of #016630.
    /// </summary>
    public static Color Green800 { get; } = GetColor("#016630");

    /// <summary>
    /// Gets the Tailwind color with the value of #0d542b.
    /// </summary>
    public static Color Green900 { get; } = GetColor("#0d542b");

    /// <summary>
    /// Gets the Tailwind color with the value of #032e15.
    /// </summary>
    public static Color Green950 { get; } = GetColor("#032e15");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfdf5.
    /// </summary>
    public static Color Emerald50 { get; } = GetColor("#ecfdf5");

    /// <summary>
    /// Gets the Tailwind color with the value of #d0fae5.
    /// </summary>
    public static Color Emerald100 { get; } = GetColor("#d0fae5");

    /// <summary>
    /// Gets the Tailwind color with the value of #a4f4cf.
    /// </summary>
    public static Color Emerald200 { get; } = GetColor("#a4f4cf");

    /// <summary>
    /// Gets the Tailwind color with the value of #5ee9b5.
    /// </summary>
    public static Color Emerald300 { get; } = GetColor("#5ee9b5");

    /// <summary>
    /// Gets the Tailwind color with the value of #00d492.
    /// </summary>
    public static Color Emerald400 { get; } = GetColor("#00d492");

    /// <summary>
    /// Gets the Tailwind color with the value of #00bc7d.
    /// </summary>
    public static Color Emerald500 { get; } = GetColor("#00bc7d");

    /// <summary>
    /// Gets the Tailwind color with the value of #009966.
    /// </summary>
    public static Color Emerald600 { get; } = GetColor("#009966");

    /// <summary>
    /// Gets the Tailwind color with the value of #007a55.
    /// </summary>
    public static Color Emerald700 { get; } = GetColor("#007a55");

    /// <summary>
    /// Gets the Tailwind color with the value of #006045.
    /// </summary>
    public static Color Emerald800 { get; } = GetColor("#006045");

    /// <summary>
    /// Gets the Tailwind color with the value of #004f3b.
    /// </summary>
    public static Color Emerald900 { get; } = GetColor("#004f3b");

    /// <summary>
    /// Gets the Tailwind color with the value of #002c22.
    /// </summary>
    public static Color Emerald950 { get; } = GetColor("#002c22");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0fdfa.
    /// </summary>
    public static Color Teal50 { get; } = GetColor("#f0fdfa");

    /// <summary>
    /// Gets the Tailwind color with the value of #cbfbf1.
    /// </summary>
    public static Color Teal100 { get; } = GetColor("#cbfbf1");

    /// <summary>
    /// Gets the Tailwind color with the value of #96f7e4.
    /// </summary>
    public static Color Teal200 { get; } = GetColor("#96f7e4");

    /// <summary>
    /// Gets the Tailwind color with the value of #46ecd5.
    /// </summary>
    public static Color Teal300 { get; } = GetColor("#46ecd5");

    /// <summary>
    /// Gets the Tailwind color with the value of #00d5be.
    /// </summary>
    public static Color Teal400 { get; } = GetColor("#00d5be");

    /// <summary>
    /// Gets the Tailwind color with the value of #00bba7.
    /// </summary>
    public static Color Teal500 { get; } = GetColor("#00bba7");

    /// <summary>
    /// Gets the Tailwind color with the value of #009689.
    /// </summary>
    public static Color Teal600 { get; } = GetColor("#009689");

    /// <summary>
    /// Gets the Tailwind color with the value of #00786f.
    /// </summary>
    public static Color Teal700 { get; } = GetColor("#00786f");

    /// <summary>
    /// Gets the Tailwind color with the value of #005f5a.
    /// </summary>
    public static Color Teal800 { get; } = GetColor("#005f5a");

    /// <summary>
    /// Gets the Tailwind color with the value of #0b4f4a.
    /// </summary>
    public static Color Teal900 { get; } = GetColor("#0b4f4a");

    /// <summary>
    /// Gets the Tailwind color with the value of #022f2e.
    /// </summary>
    public static Color Teal950 { get; } = GetColor("#022f2e");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfeff.
    /// </summary>
    public static Color Cyan50 { get; } = GetColor("#ecfeff");

    /// <summary>
    /// Gets the Tailwind color with the value of #cefafe.
    /// </summary>
    public static Color Cyan100 { get; } = GetColor("#cefafe");

    /// <summary>
    /// Gets the Tailwind color with the value of #a2f4fd.
    /// </summary>
    public static Color Cyan200 { get; } = GetColor("#a2f4fd");

    /// <summary>
    /// Gets the Tailwind color with the value of #53eafd.
    /// </summary>
    public static Color Cyan300 { get; } = GetColor("#53eafd");

    /// <summary>
    /// Gets the Tailwind color with the value of #00d3f2.
    /// </summary>
    public static Color Cyan400 { get; } = GetColor("#00d3f2");

    /// <summary>
    /// Gets the Tailwind color with the value of #00b8db.
    /// </summary>
    public static Color Cyan500 { get; } = GetColor("#00b8db");

    /// <summary>
    /// Gets the Tailwind color with the value of #0092b8.
    /// </summary>
    public static Color Cyan600 { get; } = GetColor("#0092b8");

    /// <summary>
    /// Gets the Tailwind color with the value of #007595.
    /// </summary>
    public static Color Cyan700 { get; } = GetColor("#007595");

    /// <summary>
    /// Gets the Tailwind color with the value of #005f78.
    /// </summary>
    public static Color Cyan800 { get; } = GetColor("#005f78");

    /// <summary>
    /// Gets the Tailwind color with the value of #104e64.
    /// </summary>
    public static Color Cyan900 { get; } = GetColor("#104e64");

    /// <summary>
    /// Gets the Tailwind color with the value of #053345.
    /// </summary>
    public static Color Cyan950 { get; } = GetColor("#053345");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0f9ff.
    /// </summary>
    public static Color Sky50 { get; } = GetColor("#f0f9ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #dff2fe.
    /// </summary>
    public static Color Sky100 { get; } = GetColor("#dff2fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #b8e6fe.
    /// </summary>
    public static Color Sky200 { get; } = GetColor("#b8e6fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #74d4ff.
    /// </summary>
    public static Color Sky300 { get; } = GetColor("#74d4ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #00bcff.
    /// </summary>
    public static Color Sky400 { get; } = GetColor("#00bcff");

    /// <summary>
    /// Gets the Tailwind color with the value of #00a6f4.
    /// </summary>
    public static Color Sky500 { get; } = GetColor("#00a6f4");

    /// <summary>
    /// Gets the Tailwind color with the value of #0084d1.
    /// </summary>
    public static Color Sky600 { get; } = GetColor("#0084d1");

    /// <summary>
    /// Gets the Tailwind color with the value of #0069a8.
    /// </summary>
    public static Color Sky700 { get; } = GetColor("#0069a8");

    /// <summary>
    /// Gets the Tailwind color with the value of #00598a.
    /// </summary>
    public static Color Sky800 { get; } = GetColor("#00598a");

    /// <summary>
    /// Gets the Tailwind color with the value of #024a70.
    /// </summary>
    public static Color Sky900 { get; } = GetColor("#024a70");

    /// <summary>
    /// Gets the Tailwind color with the value of #052f4a.
    /// </summary>
    public static Color Sky950 { get; } = GetColor("#052f4a");

    /// <summary>
    /// Gets the Tailwind color with the value of #eff6ff.
    /// </summary>
    public static Color Blue50 { get; } = GetColor("#eff6ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #dbeafe.
    /// </summary>
    public static Color Blue100 { get; } = GetColor("#dbeafe");

    /// <summary>
    /// Gets the Tailwind color with the value of #bedbff.
    /// </summary>
    public static Color Blue200 { get; } = GetColor("#bedbff");

    /// <summary>
    /// Gets the Tailwind color with the value of #8ec5ff.
    /// </summary>
    public static Color Blue300 { get; } = GetColor("#8ec5ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #51a2ff.
    /// </summary>
    public static Color Blue400 { get; } = GetColor("#51a2ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #2b7fff.
    /// </summary>
    public static Color Blue500 { get; } = GetColor("#2b7fff");

    /// <summary>
    /// Gets the Tailwind color with the value of #155dfc.
    /// </summary>
    public static Color Blue600 { get; } = GetColor("#155dfc");

    /// <summary>
    /// Gets the Tailwind color with the value of #1447e6.
    /// </summary>
    public static Color Blue700 { get; } = GetColor("#1447e6");

    /// <summary>
    /// Gets the Tailwind color with the value of #193cb8.
    /// </summary>
    public static Color Blue800 { get; } = GetColor("#193cb8");

    /// <summary>
    /// Gets the Tailwind color with the value of #1c398e.
    /// </summary>
    public static Color Blue900 { get; } = GetColor("#1c398e");

    /// <summary>
    /// Gets the Tailwind color with the value of #162456.
    /// </summary>
    public static Color Blue950 { get; } = GetColor("#162456");

    /// <summary>
    /// Gets the Tailwind color with the value of #eef2ff.
    /// </summary>
    public static Color Indigo50 { get; } = GetColor("#eef2ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e0e7ff.
    /// </summary>
    public static Color Indigo100 { get; } = GetColor("#e0e7ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #c6d2ff.
    /// </summary>
    public static Color Indigo200 { get; } = GetColor("#c6d2ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #a3b3ff.
    /// </summary>
    public static Color Indigo300 { get; } = GetColor("#a3b3ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #7c86ff.
    /// </summary>
    public static Color Indigo400 { get; } = GetColor("#7c86ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #615fff.
    /// </summary>
    public static Color Indigo500 { get; } = GetColor("#615fff");

    /// <summary>
    /// Gets the Tailwind color with the value of #4f39f6.
    /// </summary>
    public static Color Indigo600 { get; } = GetColor("#4f39f6");

    /// <summary>
    /// Gets the Tailwind color with the value of #432dd7.
    /// </summary>
    public static Color Indigo700 { get; } = GetColor("#432dd7");

    /// <summary>
    /// Gets the Tailwind color with the value of #372aac.
    /// </summary>
    public static Color Indigo800 { get; } = GetColor("#372aac");

    /// <summary>
    /// Gets the Tailwind color with the value of #312c85.
    /// </summary>
    public static Color Indigo900 { get; } = GetColor("#312c85");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e1a4d.
    /// </summary>
    public static Color Indigo950 { get; } = GetColor("#1e1a4d");

    /// <summary>
    /// Gets the Tailwind color with the value of #f5f3ff.
    /// </summary>
    public static Color Violet50 { get; } = GetColor("#f5f3ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #ede9fe.
    /// </summary>
    public static Color Violet100 { get; } = GetColor("#ede9fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #ddd6ff.
    /// </summary>
    public static Color Violet200 { get; } = GetColor("#ddd6ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #c4b4ff.
    /// </summary>
    public static Color Violet300 { get; } = GetColor("#c4b4ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #a684ff.
    /// </summary>
    public static Color Violet400 { get; } = GetColor("#a684ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #8e51ff.
    /// </summary>
    public static Color Violet500 { get; } = GetColor("#8e51ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #7f22fe.
    /// </summary>
    public static Color Violet600 { get; } = GetColor("#7f22fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #7008e7.
    /// </summary>
    public static Color Violet700 { get; } = GetColor("#7008e7");

    /// <summary>
    /// Gets the Tailwind color with the value of #5d0ec0.
    /// </summary>
    public static Color Violet800 { get; } = GetColor("#5d0ec0");

    /// <summary>
    /// Gets the Tailwind color with the value of #4d179a.
    /// </summary>
    public static Color Violet900 { get; } = GetColor("#4d179a");

    /// <summary>
    /// Gets the Tailwind color with the value of #2f0d68.
    /// </summary>
    public static Color Violet950 { get; } = GetColor("#2f0d68");

    /// <summary>
    /// Gets the Tailwind color with the value of #faf5ff.
    /// </summary>
    public static Color Purple50 { get; } = GetColor("#faf5ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f3e8ff.
    /// </summary>
    public static Color Purple100 { get; } = GetColor("#f3e8ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e9d4ff.
    /// </summary>
    public static Color Purple200 { get; } = GetColor("#e9d4ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #dab2ff.
    /// </summary>
    public static Color Purple300 { get; } = GetColor("#dab2ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #c27aff.
    /// </summary>
    public static Color Purple400 { get; } = GetColor("#c27aff");

    /// <summary>
    /// Gets the Tailwind color with the value of #ad46ff.
    /// </summary>
    public static Color Purple500 { get; } = GetColor("#ad46ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #9810fa.
    /// </summary>
    public static Color Purple600 { get; } = GetColor("#9810fa");

    /// <summary>
    /// Gets the Tailwind color with the value of #8200db.
    /// </summary>
    public static Color Purple700 { get; } = GetColor("#8200db");

    /// <summary>
    /// Gets the Tailwind color with the value of #6e11b0.
    /// </summary>
    public static Color Purple800 { get; } = GetColor("#6e11b0");

    /// <summary>
    /// Gets the Tailwind color with the value of #59168b.
    /// </summary>
    public static Color Purple900 { get; } = GetColor("#59168b");

    /// <summary>
    /// Gets the Tailwind color with the value of #3c0366.
    /// </summary>
    public static Color Purple950 { get; } = GetColor("#3c0366");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdf4ff.
    /// </summary>
    public static Color Fuchsia50 { get; } = GetColor("#fdf4ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #fae8ff.
    /// </summary>
    public static Color Fuchsia100 { get; } = GetColor("#fae8ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f6cfff.
    /// </summary>
    public static Color Fuchsia200 { get; } = GetColor("#f6cfff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f4a8ff.
    /// </summary>
    public static Color Fuchsia300 { get; } = GetColor("#f4a8ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #ed6aff.
    /// </summary>
    public static Color Fuchsia400 { get; } = GetColor("#ed6aff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e12afb.
    /// </summary>
    public static Color Fuchsia500 { get; } = GetColor("#e12afb");

    /// <summary>
    /// Gets the Tailwind color with the value of #c800de.
    /// </summary>
    public static Color Fuchsia600 { get; } = GetColor("#c800de");

    /// <summary>
    /// Gets the Tailwind color with the value of #a800b7.
    /// </summary>
    public static Color Fuchsia700 { get; } = GetColor("#a800b7");

    /// <summary>
    /// Gets the Tailwind color with the value of #8a0194.
    /// </summary>
    public static Color Fuchsia800 { get; } = GetColor("#8a0194");

    /// <summary>
    /// Gets the Tailwind color with the value of #721378.
    /// </summary>
    public static Color Fuchsia900 { get; } = GetColor("#721378");

    /// <summary>
    /// Gets the Tailwind color with the value of #4b004f.
    /// </summary>
    public static Color Fuchsia950 { get; } = GetColor("#4b004f");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdf2f8.
    /// </summary>
    public static Color Pink50 { get; } = GetColor("#fdf2f8");

    /// <summary>
    /// Gets the Tailwind color with the value of #fce7f3.
    /// </summary>
    public static Color Pink100 { get; } = GetColor("#fce7f3");

    /// <summary>
    /// Gets the Tailwind color with the value of #fccee8.
    /// </summary>
    public static Color Pink200 { get; } = GetColor("#fccee8");

    /// <summary>
    /// Gets the Tailwind color with the value of #fda5d5.
    /// </summary>
    public static Color Pink300 { get; } = GetColor("#fda5d5");

    /// <summary>
    /// Gets the Tailwind color with the value of #fb64b6.
    /// </summary>
    public static Color Pink400 { get; } = GetColor("#fb64b6");

    /// <summary>
    /// Gets the Tailwind color with the value of #f6339a.
    /// </summary>
    public static Color Pink500 { get; } = GetColor("#f6339a");

    /// <summary>
    /// Gets the Tailwind color with the value of #e60076.
    /// </summary>
    public static Color Pink600 { get; } = GetColor("#e60076");

    /// <summary>
    /// Gets the Tailwind color with the value of #c6005c.
    /// </summary>
    public static Color Pink700 { get; } = GetColor("#c6005c");

    /// <summary>
    /// Gets the Tailwind color with the value of #a3004c.
    /// </summary>
    public static Color Pink800 { get; } = GetColor("#a3004c");

    /// <summary>
    /// Gets the Tailwind color with the value of #861043.
    /// </summary>
    public static Color Pink900 { get; } = GetColor("#861043");

    /// <summary>
    /// Gets the Tailwind color with the value of #510424.
    /// </summary>
    public static Color Pink950 { get; } = GetColor("#510424");

    /// <summary>
    /// Gets the Tailwind color with the value of #fff1f2.
    /// </summary>
    public static Color Rose50 { get; } = GetColor("#fff1f2");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffe4e6.
    /// </summary>
    public static Color Rose100 { get; } = GetColor("#ffe4e6");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffccd3.
    /// </summary>
    public static Color Rose200 { get; } = GetColor("#ffccd3");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffa1ad.
    /// </summary>
    public static Color Rose300 { get; } = GetColor("#ffa1ad");

    /// <summary>
    /// Gets the Tailwind color with the value of #ff637e.
    /// </summary>
    public static Color Rose400 { get; } = GetColor("#ff637e");

    /// <summary>
    /// Gets the Tailwind color with the value of #ff2056.
    /// </summary>
    public static Color Rose500 { get; } = GetColor("#ff2056");

    /// <summary>
    /// Gets the Tailwind color with the value of #ec003f.
    /// </summary>
    public static Color Rose600 { get; } = GetColor("#ec003f");

    /// <summary>
    /// Gets the Tailwind color with the value of #c70036.
    /// </summary>
    public static Color Rose700 { get; } = GetColor("#c70036");

    /// <summary>
    /// Gets the Tailwind color with the value of #a50036.
    /// </summary>
    public static Color Rose800 { get; } = GetColor("#a50036");

    /// <summary>
    /// Gets the Tailwind color with the value of #8b0836.
    /// </summary>
    public static Color Rose900 { get; } = GetColor("#8b0836");

    /// <summary>
    /// Gets the Tailwind color with the value of #4d0218.
    /// </summary>
    public static Color Rose950 { get; } = GetColor("#4d0218");
}
