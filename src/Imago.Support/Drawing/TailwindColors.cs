namespace Imago.Support.Drawing;

/// <summary>
/// Provides access to the colors in the official Tailwind CSS color palette from v3.4.13.
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
    /// Gets the Tailwind color with the value of #cbd5e1.
    /// </summary>
    public static Color Slate300 { get; } = GetColor("#cbd5e1");

    /// <summary>
    /// Gets the Tailwind color with the value of #94a3b8.
    /// </summary>
    public static Color Slate400 { get; } = GetColor("#94a3b8");

    /// <summary>
    /// Gets the Tailwind color with the value of #64748b.
    /// </summary>
    public static Color Slate500 { get; } = GetColor("#64748b");

    /// <summary>
    /// Gets the Tailwind color with the value of #475569.
    /// </summary>
    public static Color Slate600 { get; } = GetColor("#475569");

    /// <summary>
    /// Gets the Tailwind color with the value of #334155.
    /// </summary>
    public static Color Slate700 { get; } = GetColor("#334155");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e293b.
    /// </summary>
    public static Color Slate800 { get; } = GetColor("#1e293b");

    /// <summary>
    /// Gets the Tailwind color with the value of #0f172a.
    /// </summary>
    public static Color Slate900 { get; } = GetColor("#0f172a");

    /// <summary>
    /// Gets the Tailwind color with the value of #020617.
    /// </summary>
    public static Color Slate950 { get; } = GetColor("#020617");

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
    /// Gets the Tailwind color with the value of #d1d5db.
    /// </summary>
    public static Color Gray300 { get; } = GetColor("#d1d5db");

    /// <summary>
    /// Gets the Tailwind color with the value of #9ca3af.
    /// </summary>
    public static Color Gray400 { get; } = GetColor("#9ca3af");

    /// <summary>
    /// Gets the Tailwind color with the value of #6b7280.
    /// </summary>
    public static Color Gray500 { get; } = GetColor("#6b7280");

    /// <summary>
    /// Gets the Tailwind color with the value of #4b5563.
    /// </summary>
    public static Color Gray600 { get; } = GetColor("#4b5563");

    /// <summary>
    /// Gets the Tailwind color with the value of #374151.
    /// </summary>
    public static Color Gray700 { get; } = GetColor("#374151");

    /// <summary>
    /// Gets the Tailwind color with the value of #1f2937.
    /// </summary>
    public static Color Gray800 { get; } = GetColor("#1f2937");

    /// <summary>
    /// Gets the Tailwind color with the value of #111827.
    /// </summary>
    public static Color Gray900 { get; } = GetColor("#111827");

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
    /// Gets the Tailwind color with the value of #a1a1aa.
    /// </summary>
    public static Color Zinc400 { get; } = GetColor("#a1a1aa");

    /// <summary>
    /// Gets the Tailwind color with the value of #71717a.
    /// </summary>
    public static Color Zinc500 { get; } = GetColor("#71717a");

    /// <summary>
    /// Gets the Tailwind color with the value of #52525b.
    /// </summary>
    public static Color Zinc600 { get; } = GetColor("#52525b");

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
    /// Gets the Tailwind color with the value of #a3a3a3.
    /// </summary>
    public static Color Neutral400 { get; } = GetColor("#a3a3a3");

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
    /// Gets the Tailwind color with the value of #a8a29e.
    /// </summary>
    public static Color Stone400 { get; } = GetColor("#a8a29e");

    /// <summary>
    /// Gets the Tailwind color with the value of #78716c.
    /// </summary>
    public static Color Stone500 { get; } = GetColor("#78716c");

    /// <summary>
    /// Gets the Tailwind color with the value of #57534e.
    /// </summary>
    public static Color Stone600 { get; } = GetColor("#57534e");

    /// <summary>
    /// Gets the Tailwind color with the value of #44403c.
    /// </summary>
    public static Color Stone700 { get; } = GetColor("#44403c");

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
    /// Gets the Tailwind color with the value of #fee2e2.
    /// </summary>
    public static Color Red100 { get; } = GetColor("#fee2e2");

    /// <summary>
    /// Gets the Tailwind color with the value of #fecaca.
    /// </summary>
    public static Color Red200 { get; } = GetColor("#fecaca");

    /// <summary>
    /// Gets the Tailwind color with the value of #fca5a5.
    /// </summary>
    public static Color Red300 { get; } = GetColor("#fca5a5");

    /// <summary>
    /// Gets the Tailwind color with the value of #f87171.
    /// </summary>
    public static Color Red400 { get; } = GetColor("#f87171");

    /// <summary>
    /// Gets the Tailwind color with the value of #ef4444.
    /// </summary>
    public static Color Red500 { get; } = GetColor("#ef4444");

    /// <summary>
    /// Gets the Tailwind color with the value of #dc2626.
    /// </summary>
    public static Color Red600 { get; } = GetColor("#dc2626");

    /// <summary>
    /// Gets the Tailwind color with the value of #b91c1c.
    /// </summary>
    public static Color Red700 { get; } = GetColor("#b91c1c");

    /// <summary>
    /// Gets the Tailwind color with the value of #991b1b.
    /// </summary>
    public static Color Red800 { get; } = GetColor("#991b1b");

    /// <summary>
    /// Gets the Tailwind color with the value of #7f1d1d.
    /// </summary>
    public static Color Red900 { get; } = GetColor("#7f1d1d");

    /// <summary>
    /// Gets the Tailwind color with the value of #450a0a.
    /// </summary>
    public static Color Red950 { get; } = GetColor("#450a0a");

    /// <summary>
    /// Gets the Tailwind color with the value of #fff7ed.
    /// </summary>
    public static Color Orange50 { get; } = GetColor("#fff7ed");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffedd5.
    /// </summary>
    public static Color Orange100 { get; } = GetColor("#ffedd5");

    /// <summary>
    /// Gets the Tailwind color with the value of #fed7aa.
    /// </summary>
    public static Color Orange200 { get; } = GetColor("#fed7aa");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdba74.
    /// </summary>
    public static Color Orange300 { get; } = GetColor("#fdba74");

    /// <summary>
    /// Gets the Tailwind color with the value of #fb923c.
    /// </summary>
    public static Color Orange400 { get; } = GetColor("#fb923c");

    /// <summary>
    /// Gets the Tailwind color with the value of #f97316.
    /// </summary>
    public static Color Orange500 { get; } = GetColor("#f97316");

    /// <summary>
    /// Gets the Tailwind color with the value of #ea580c.
    /// </summary>
    public static Color Orange600 { get; } = GetColor("#ea580c");

    /// <summary>
    /// Gets the Tailwind color with the value of #c2410c.
    /// </summary>
    public static Color Orange700 { get; } = GetColor("#c2410c");

    /// <summary>
    /// Gets the Tailwind color with the value of #9a3412.
    /// </summary>
    public static Color Orange800 { get; } = GetColor("#9a3412");

    /// <summary>
    /// Gets the Tailwind color with the value of #7c2d12.
    /// </summary>
    public static Color Orange900 { get; } = GetColor("#7c2d12");

    /// <summary>
    /// Gets the Tailwind color with the value of #431407.
    /// </summary>
    public static Color Orange950 { get; } = GetColor("#431407");

    /// <summary>
    /// Gets the Tailwind color with the value of #fffbeb.
    /// </summary>
    public static Color Amber50 { get; } = GetColor("#fffbeb");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef3c7.
    /// </summary>
    public static Color Amber100 { get; } = GetColor("#fef3c7");

    /// <summary>
    /// Gets the Tailwind color with the value of #fde68a.
    /// </summary>
    public static Color Amber200 { get; } = GetColor("#fde68a");

    /// <summary>
    /// Gets the Tailwind color with the value of #fcd34d.
    /// </summary>
    public static Color Amber300 { get; } = GetColor("#fcd34d");

    /// <summary>
    /// Gets the Tailwind color with the value of #fbbf24.
    /// </summary>
    public static Color Amber400 { get; } = GetColor("#fbbf24");

    /// <summary>
    /// Gets the Tailwind color with the value of #f59e0b.
    /// </summary>
    public static Color Amber500 { get; } = GetColor("#f59e0b");

    /// <summary>
    /// Gets the Tailwind color with the value of #d97706.
    /// </summary>
    public static Color Amber600 { get; } = GetColor("#d97706");

    /// <summary>
    /// Gets the Tailwind color with the value of #b45309.
    /// </summary>
    public static Color Amber700 { get; } = GetColor("#b45309");

    /// <summary>
    /// Gets the Tailwind color with the value of #92400e.
    /// </summary>
    public static Color Amber800 { get; } = GetColor("#92400e");

    /// <summary>
    /// Gets the Tailwind color with the value of #78350f.
    /// </summary>
    public static Color Amber900 { get; } = GetColor("#78350f");

    /// <summary>
    /// Gets the Tailwind color with the value of #451a03.
    /// </summary>
    public static Color Amber950 { get; } = GetColor("#451a03");

    /// <summary>
    /// Gets the Tailwind color with the value of #fefce8.
    /// </summary>
    public static Color Yellow50 { get; } = GetColor("#fefce8");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef9c3.
    /// </summary>
    public static Color Yellow100 { get; } = GetColor("#fef9c3");

    /// <summary>
    /// Gets the Tailwind color with the value of #fef08a.
    /// </summary>
    public static Color Yellow200 { get; } = GetColor("#fef08a");

    /// <summary>
    /// Gets the Tailwind color with the value of #fde047.
    /// </summary>
    public static Color Yellow300 { get; } = GetColor("#fde047");

    /// <summary>
    /// Gets the Tailwind color with the value of #facc15.
    /// </summary>
    public static Color Yellow400 { get; } = GetColor("#facc15");

    /// <summary>
    /// Gets the Tailwind color with the value of #eab308.
    /// </summary>
    public static Color Yellow500 { get; } = GetColor("#eab308");

    /// <summary>
    /// Gets the Tailwind color with the value of #ca8a04.
    /// </summary>
    public static Color Yellow600 { get; } = GetColor("#ca8a04");

    /// <summary>
    /// Gets the Tailwind color with the value of #a16207.
    /// </summary>
    public static Color Yellow700 { get; } = GetColor("#a16207");

    /// <summary>
    /// Gets the Tailwind color with the value of #854d0e.
    /// </summary>
    public static Color Yellow800 { get; } = GetColor("#854d0e");

    /// <summary>
    /// Gets the Tailwind color with the value of #713f12.
    /// </summary>
    public static Color Yellow900 { get; } = GetColor("#713f12");

    /// <summary>
    /// Gets the Tailwind color with the value of #422006.
    /// </summary>
    public static Color Yellow950 { get; } = GetColor("#422006");

    /// <summary>
    /// Gets the Tailwind color with the value of #f7fee7.
    /// </summary>
    public static Color Lime50 { get; } = GetColor("#f7fee7");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfccb.
    /// </summary>
    public static Color Lime100 { get; } = GetColor("#ecfccb");

    /// <summary>
    /// Gets the Tailwind color with the value of #d9f99d.
    /// </summary>
    public static Color Lime200 { get; } = GetColor("#d9f99d");

    /// <summary>
    /// Gets the Tailwind color with the value of #bef264.
    /// </summary>
    public static Color Lime300 { get; } = GetColor("#bef264");

    /// <summary>
    /// Gets the Tailwind color with the value of #a3e635.
    /// </summary>
    public static Color Lime400 { get; } = GetColor("#a3e635");

    /// <summary>
    /// Gets the Tailwind color with the value of #84cc16.
    /// </summary>
    public static Color Lime500 { get; } = GetColor("#84cc16");

    /// <summary>
    /// Gets the Tailwind color with the value of #65a30d.
    /// </summary>
    public static Color Lime600 { get; } = GetColor("#65a30d");

    /// <summary>
    /// Gets the Tailwind color with the value of #4d7c0f.
    /// </summary>
    public static Color Lime700 { get; } = GetColor("#4d7c0f");

    /// <summary>
    /// Gets the Tailwind color with the value of #3f6212.
    /// </summary>
    public static Color Lime800 { get; } = GetColor("#3f6212");

    /// <summary>
    /// Gets the Tailwind color with the value of #365314.
    /// </summary>
    public static Color Lime900 { get; } = GetColor("#365314");

    /// <summary>
    /// Gets the Tailwind color with the value of #1a2e05.
    /// </summary>
    public static Color Lime950 { get; } = GetColor("#1a2e05");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0fdf4.
    /// </summary>
    public static Color Green50 { get; } = GetColor("#f0fdf4");

    /// <summary>
    /// Gets the Tailwind color with the value of #dcfce7.
    /// </summary>
    public static Color Green100 { get; } = GetColor("#dcfce7");

    /// <summary>
    /// Gets the Tailwind color with the value of #bbf7d0.
    /// </summary>
    public static Color Green200 { get; } = GetColor("#bbf7d0");

    /// <summary>
    /// Gets the Tailwind color with the value of #86efac.
    /// </summary>
    public static Color Green300 { get; } = GetColor("#86efac");

    /// <summary>
    /// Gets the Tailwind color with the value of #4ade80.
    /// </summary>
    public static Color Green400 { get; } = GetColor("#4ade80");

    /// <summary>
    /// Gets the Tailwind color with the value of #22c55e.
    /// </summary>
    public static Color Green500 { get; } = GetColor("#22c55e");

    /// <summary>
    /// Gets the Tailwind color with the value of #16a34a.
    /// </summary>
    public static Color Green600 { get; } = GetColor("#16a34a");

    /// <summary>
    /// Gets the Tailwind color with the value of #15803d.
    /// </summary>
    public static Color Green700 { get; } = GetColor("#15803d");

    /// <summary>
    /// Gets the Tailwind color with the value of #166534.
    /// </summary>
    public static Color Green800 { get; } = GetColor("#166534");

    /// <summary>
    /// Gets the Tailwind color with the value of #14532d.
    /// </summary>
    public static Color Green900 { get; } = GetColor("#14532d");

    /// <summary>
    /// Gets the Tailwind color with the value of #052e16.
    /// </summary>
    public static Color Green950 { get; } = GetColor("#052e16");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfdf5.
    /// </summary>
    public static Color Emerald50 { get; } = GetColor("#ecfdf5");

    /// <summary>
    /// Gets the Tailwind color with the value of #d1fae5.
    /// </summary>
    public static Color Emerald100 { get; } = GetColor("#d1fae5");

    /// <summary>
    /// Gets the Tailwind color with the value of #a7f3d0.
    /// </summary>
    public static Color Emerald200 { get; } = GetColor("#a7f3d0");

    /// <summary>
    /// Gets the Tailwind color with the value of #6ee7b7.
    /// </summary>
    public static Color Emerald300 { get; } = GetColor("#6ee7b7");

    /// <summary>
    /// Gets the Tailwind color with the value of #34d399.
    /// </summary>
    public static Color Emerald400 { get; } = GetColor("#34d399");

    /// <summary>
    /// Gets the Tailwind color with the value of #10b981.
    /// </summary>
    public static Color Emerald500 { get; } = GetColor("#10b981");

    /// <summary>
    /// Gets the Tailwind color with the value of #059669.
    /// </summary>
    public static Color Emerald600 { get; } = GetColor("#059669");

    /// <summary>
    /// Gets the Tailwind color with the value of #047857.
    /// </summary>
    public static Color Emerald700 { get; } = GetColor("#047857");

    /// <summary>
    /// Gets the Tailwind color with the value of #065f46.
    /// </summary>
    public static Color Emerald800 { get; } = GetColor("#065f46");

    /// <summary>
    /// Gets the Tailwind color with the value of #064e3b.
    /// </summary>
    public static Color Emerald900 { get; } = GetColor("#064e3b");

    /// <summary>
    /// Gets the Tailwind color with the value of #022c22.
    /// </summary>
    public static Color Emerald950 { get; } = GetColor("#022c22");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0fdfa.
    /// </summary>
    public static Color Teal50 { get; } = GetColor("#f0fdfa");

    /// <summary>
    /// Gets the Tailwind color with the value of #ccfbf1.
    /// </summary>
    public static Color Teal100 { get; } = GetColor("#ccfbf1");

    /// <summary>
    /// Gets the Tailwind color with the value of #99f6e4.
    /// </summary>
    public static Color Teal200 { get; } = GetColor("#99f6e4");

    /// <summary>
    /// Gets the Tailwind color with the value of #5eead4.
    /// </summary>
    public static Color Teal300 { get; } = GetColor("#5eead4");

    /// <summary>
    /// Gets the Tailwind color with the value of #2dd4bf.
    /// </summary>
    public static Color Teal400 { get; } = GetColor("#2dd4bf");

    /// <summary>
    /// Gets the Tailwind color with the value of #14b8a6.
    /// </summary>
    public static Color Teal500 { get; } = GetColor("#14b8a6");

    /// <summary>
    /// Gets the Tailwind color with the value of #0d9488.
    /// </summary>
    public static Color Teal600 { get; } = GetColor("#0d9488");

    /// <summary>
    /// Gets the Tailwind color with the value of #0f766e.
    /// </summary>
    public static Color Teal700 { get; } = GetColor("#0f766e");

    /// <summary>
    /// Gets the Tailwind color with the value of #115e59.
    /// </summary>
    public static Color Teal800 { get; } = GetColor("#115e59");

    /// <summary>
    /// Gets the Tailwind color with the value of #134e4a.
    /// </summary>
    public static Color Teal900 { get; } = GetColor("#134e4a");

    /// <summary>
    /// Gets the Tailwind color with the value of #042f2e.
    /// </summary>
    public static Color Teal950 { get; } = GetColor("#042f2e");

    /// <summary>
    /// Gets the Tailwind color with the value of #ecfeff.
    /// </summary>
    public static Color Cyan50 { get; } = GetColor("#ecfeff");

    /// <summary>
    /// Gets the Tailwind color with the value of #cffafe.
    /// </summary>
    public static Color Cyan100 { get; } = GetColor("#cffafe");

    /// <summary>
    /// Gets the Tailwind color with the value of #a5f3fc.
    /// </summary>
    public static Color Cyan200 { get; } = GetColor("#a5f3fc");

    /// <summary>
    /// Gets the Tailwind color with the value of #67e8f9.
    /// </summary>
    public static Color Cyan300 { get; } = GetColor("#67e8f9");

    /// <summary>
    /// Gets the Tailwind color with the value of #22d3ee.
    /// </summary>
    public static Color Cyan400 { get; } = GetColor("#22d3ee");

    /// <summary>
    /// Gets the Tailwind color with the value of #06b6d4.
    /// </summary>
    public static Color Cyan500 { get; } = GetColor("#06b6d4");

    /// <summary>
    /// Gets the Tailwind color with the value of #0891b2.
    /// </summary>
    public static Color Cyan600 { get; } = GetColor("#0891b2");

    /// <summary>
    /// Gets the Tailwind color with the value of #0e7490.
    /// </summary>
    public static Color Cyan700 { get; } = GetColor("#0e7490");

    /// <summary>
    /// Gets the Tailwind color with the value of #155e75.
    /// </summary>
    public static Color Cyan800 { get; } = GetColor("#155e75");

    /// <summary>
    /// Gets the Tailwind color with the value of #164e63.
    /// </summary>
    public static Color Cyan900 { get; } = GetColor("#164e63");

    /// <summary>
    /// Gets the Tailwind color with the value of #083344.
    /// </summary>
    public static Color Cyan950 { get; } = GetColor("#083344");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0f9ff.
    /// </summary>
    public static Color Sky50 { get; } = GetColor("#f0f9ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e0f2fe.
    /// </summary>
    public static Color Sky100 { get; } = GetColor("#e0f2fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #bae6fd.
    /// </summary>
    public static Color Sky200 { get; } = GetColor("#bae6fd");

    /// <summary>
    /// Gets the Tailwind color with the value of #7dd3fc.
    /// </summary>
    public static Color Sky300 { get; } = GetColor("#7dd3fc");

    /// <summary>
    /// Gets the Tailwind color with the value of #38bdf8.
    /// </summary>
    public static Color Sky400 { get; } = GetColor("#38bdf8");

    /// <summary>
    /// Gets the Tailwind color with the value of #0ea5e9.
    /// </summary>
    public static Color Sky500 { get; } = GetColor("#0ea5e9");

    /// <summary>
    /// Gets the Tailwind color with the value of #0284c7.
    /// </summary>
    public static Color Sky600 { get; } = GetColor("#0284c7");

    /// <summary>
    /// Gets the Tailwind color with the value of #0369a1.
    /// </summary>
    public static Color Sky700 { get; } = GetColor("#0369a1");

    /// <summary>
    /// Gets the Tailwind color with the value of #075985.
    /// </summary>
    public static Color Sky800 { get; } = GetColor("#075985");

    /// <summary>
    /// Gets the Tailwind color with the value of #0c4a6e.
    /// </summary>
    public static Color Sky900 { get; } = GetColor("#0c4a6e");

    /// <summary>
    /// Gets the Tailwind color with the value of #082f49.
    /// </summary>
    public static Color Sky950 { get; } = GetColor("#082f49");

    /// <summary>
    /// Gets the Tailwind color with the value of #eff6ff.
    /// </summary>
    public static Color Blue50 { get; } = GetColor("#eff6ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #dbeafe.
    /// </summary>
    public static Color Blue100 { get; } = GetColor("#dbeafe");

    /// <summary>
    /// Gets the Tailwind color with the value of #bfdbfe.
    /// </summary>
    public static Color Blue200 { get; } = GetColor("#bfdbfe");

    /// <summary>
    /// Gets the Tailwind color with the value of #93c5fd.
    /// </summary>
    public static Color Blue300 { get; } = GetColor("#93c5fd");

    /// <summary>
    /// Gets the Tailwind color with the value of #60a5fa.
    /// </summary>
    public static Color Blue400 { get; } = GetColor("#60a5fa");

    /// <summary>
    /// Gets the Tailwind color with the value of #3b82f6.
    /// </summary>
    public static Color Blue500 { get; } = GetColor("#3b82f6");

    /// <summary>
    /// Gets the Tailwind color with the value of #2563eb.
    /// </summary>
    public static Color Blue600 { get; } = GetColor("#2563eb");

    /// <summary>
    /// Gets the Tailwind color with the value of #1d4ed8.
    /// </summary>
    public static Color Blue700 { get; } = GetColor("#1d4ed8");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e40af.
    /// </summary>
    public static Color Blue800 { get; } = GetColor("#1e40af");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e3a8a.
    /// </summary>
    public static Color Blue900 { get; } = GetColor("#1e3a8a");

    /// <summary>
    /// Gets the Tailwind color with the value of #172554.
    /// </summary>
    public static Color Blue950 { get; } = GetColor("#172554");

    /// <summary>
    /// Gets the Tailwind color with the value of #eef2ff.
    /// </summary>
    public static Color Indigo50 { get; } = GetColor("#eef2ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e0e7ff.
    /// </summary>
    public static Color Indigo100 { get; } = GetColor("#e0e7ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #c7d2fe.
    /// </summary>
    public static Color Indigo200 { get; } = GetColor("#c7d2fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #a5b4fc.
    /// </summary>
    public static Color Indigo300 { get; } = GetColor("#a5b4fc");

    /// <summary>
    /// Gets the Tailwind color with the value of #818cf8.
    /// </summary>
    public static Color Indigo400 { get; } = GetColor("#818cf8");

    /// <summary>
    /// Gets the Tailwind color with the value of #6366f1.
    /// </summary>
    public static Color Indigo500 { get; } = GetColor("#6366f1");

    /// <summary>
    /// Gets the Tailwind color with the value of #4f46e5.
    /// </summary>
    public static Color Indigo600 { get; } = GetColor("#4f46e5");

    /// <summary>
    /// Gets the Tailwind color with the value of #4338ca.
    /// </summary>
    public static Color Indigo700 { get; } = GetColor("#4338ca");

    /// <summary>
    /// Gets the Tailwind color with the value of #3730a3.
    /// </summary>
    public static Color Indigo800 { get; } = GetColor("#3730a3");

    /// <summary>
    /// Gets the Tailwind color with the value of #312e81.
    /// </summary>
    public static Color Indigo900 { get; } = GetColor("#312e81");

    /// <summary>
    /// Gets the Tailwind color with the value of #1e1b4b.
    /// </summary>
    public static Color Indigo950 { get; } = GetColor("#1e1b4b");

    /// <summary>
    /// Gets the Tailwind color with the value of #f5f3ff.
    /// </summary>
    public static Color Violet50 { get; } = GetColor("#f5f3ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #ede9fe.
    /// </summary>
    public static Color Violet100 { get; } = GetColor("#ede9fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #ddd6fe.
    /// </summary>
    public static Color Violet200 { get; } = GetColor("#ddd6fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #c4b5fd.
    /// </summary>
    public static Color Violet300 { get; } = GetColor("#c4b5fd");

    /// <summary>
    /// Gets the Tailwind color with the value of #a78bfa.
    /// </summary>
    public static Color Violet400 { get; } = GetColor("#a78bfa");

    /// <summary>
    /// Gets the Tailwind color with the value of #8b5cf6.
    /// </summary>
    public static Color Violet500 { get; } = GetColor("#8b5cf6");

    /// <summary>
    /// Gets the Tailwind color with the value of #7c3aed.
    /// </summary>
    public static Color Violet600 { get; } = GetColor("#7c3aed");

    /// <summary>
    /// Gets the Tailwind color with the value of #6d28d9.
    /// </summary>
    public static Color Violet700 { get; } = GetColor("#6d28d9");

    /// <summary>
    /// Gets the Tailwind color with the value of #5b21b6.
    /// </summary>
    public static Color Violet800 { get; } = GetColor("#5b21b6");

    /// <summary>
    /// Gets the Tailwind color with the value of #4c1d95.
    /// </summary>
    public static Color Violet900 { get; } = GetColor("#4c1d95");

    /// <summary>
    /// Gets the Tailwind color with the value of #2e1065.
    /// </summary>
    public static Color Violet950 { get; } = GetColor("#2e1065");

    /// <summary>
    /// Gets the Tailwind color with the value of #faf5ff.
    /// </summary>
    public static Color Purple50 { get; } = GetColor("#faf5ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f3e8ff.
    /// </summary>
    public static Color Purple100 { get; } = GetColor("#f3e8ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #e9d5ff.
    /// </summary>
    public static Color Purple200 { get; } = GetColor("#e9d5ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #d8b4fe.
    /// </summary>
    public static Color Purple300 { get; } = GetColor("#d8b4fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #c084fc.
    /// </summary>
    public static Color Purple400 { get; } = GetColor("#c084fc");

    /// <summary>
    /// Gets the Tailwind color with the value of #a855f7.
    /// </summary>
    public static Color Purple500 { get; } = GetColor("#a855f7");

    /// <summary>
    /// Gets the Tailwind color with the value of #9333ea.
    /// </summary>
    public static Color Purple600 { get; } = GetColor("#9333ea");

    /// <summary>
    /// Gets the Tailwind color with the value of #7e22ce.
    /// </summary>
    public static Color Purple700 { get; } = GetColor("#7e22ce");

    /// <summary>
    /// Gets the Tailwind color with the value of #6b21a8.
    /// </summary>
    public static Color Purple800 { get; } = GetColor("#6b21a8");

    /// <summary>
    /// Gets the Tailwind color with the value of #581c87.
    /// </summary>
    public static Color Purple900 { get; } = GetColor("#581c87");

    /// <summary>
    /// Gets the Tailwind color with the value of #3b0764.
    /// </summary>
    public static Color Purple950 { get; } = GetColor("#3b0764");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdf4ff.
    /// </summary>
    public static Color Fuchsia50 { get; } = GetColor("#fdf4ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #fae8ff.
    /// </summary>
    public static Color Fuchsia100 { get; } = GetColor("#fae8ff");

    /// <summary>
    /// Gets the Tailwind color with the value of #f5d0fe.
    /// </summary>
    public static Color Fuchsia200 { get; } = GetColor("#f5d0fe");

    /// <summary>
    /// Gets the Tailwind color with the value of #f0abfc.
    /// </summary>
    public static Color Fuchsia300 { get; } = GetColor("#f0abfc");

    /// <summary>
    /// Gets the Tailwind color with the value of #e879f9.
    /// </summary>
    public static Color Fuchsia400 { get; } = GetColor("#e879f9");

    /// <summary>
    /// Gets the Tailwind color with the value of #d946ef.
    /// </summary>
    public static Color Fuchsia500 { get; } = GetColor("#d946ef");

    /// <summary>
    /// Gets the Tailwind color with the value of #c026d3.
    /// </summary>
    public static Color Fuchsia600 { get; } = GetColor("#c026d3");

    /// <summary>
    /// Gets the Tailwind color with the value of #a21caf.
    /// </summary>
    public static Color Fuchsia700 { get; } = GetColor("#a21caf");

    /// <summary>
    /// Gets the Tailwind color with the value of #86198f.
    /// </summary>
    public static Color Fuchsia800 { get; } = GetColor("#86198f");

    /// <summary>
    /// Gets the Tailwind color with the value of #701a75.
    /// </summary>
    public static Color Fuchsia900 { get; } = GetColor("#701a75");

    /// <summary>
    /// Gets the Tailwind color with the value of #4a044e.
    /// </summary>
    public static Color Fuchsia950 { get; } = GetColor("#4a044e");

    /// <summary>
    /// Gets the Tailwind color with the value of #fdf2f8.
    /// </summary>
    public static Color Pink50 { get; } = GetColor("#fdf2f8");

    /// <summary>
    /// Gets the Tailwind color with the value of #fce7f3.
    /// </summary>
    public static Color Pink100 { get; } = GetColor("#fce7f3");

    /// <summary>
    /// Gets the Tailwind color with the value of #fbcfe8.
    /// </summary>
    public static Color Pink200 { get; } = GetColor("#fbcfe8");

    /// <summary>
    /// Gets the Tailwind color with the value of #f9a8d4.
    /// </summary>
    public static Color Pink300 { get; } = GetColor("#f9a8d4");

    /// <summary>
    /// Gets the Tailwind color with the value of #f472b6.
    /// </summary>
    public static Color Pink400 { get; } = GetColor("#f472b6");

    /// <summary>
    /// Gets the Tailwind color with the value of #ec4899.
    /// </summary>
    public static Color Pink500 { get; } = GetColor("#ec4899");

    /// <summary>
    /// Gets the Tailwind color with the value of #db2777.
    /// </summary>
    public static Color Pink600 { get; } = GetColor("#db2777");

    /// <summary>
    /// Gets the Tailwind color with the value of #be185d.
    /// </summary>
    public static Color Pink700 { get; } = GetColor("#be185d");

    /// <summary>
    /// Gets the Tailwind color with the value of #9d174d.
    /// </summary>
    public static Color Pink800 { get; } = GetColor("#9d174d");

    /// <summary>
    /// Gets the Tailwind color with the value of #831843.
    /// </summary>
    public static Color Pink900 { get; } = GetColor("#831843");

    /// <summary>
    /// Gets the Tailwind color with the value of #500724.
    /// </summary>
    public static Color Pink950 { get; } = GetColor("#500724");

    /// <summary>
    /// Gets the Tailwind color with the value of #fff1f2.
    /// </summary>
    public static Color Rose50 { get; } = GetColor("#fff1f2");

    /// <summary>
    /// Gets the Tailwind color with the value of #ffe4e6.
    /// </summary>
    public static Color Rose100 { get; } = GetColor("#ffe4e6");

    /// <summary>
    /// Gets the Tailwind color with the value of #fecdd3.
    /// </summary>
    public static Color Rose200 { get; } = GetColor("#fecdd3");

    /// <summary>
    /// Gets the Tailwind color with the value of #fda4af.
    /// </summary>
    public static Color Rose300 { get; } = GetColor("#fda4af");

    /// <summary>
    /// Gets the Tailwind color with the value of #fb7185.
    /// </summary>
    public static Color Rose400 { get; } = GetColor("#fb7185");

    /// <summary>
    /// Gets the Tailwind color with the value of #f43f5e.
    /// </summary>
    public static Color Rose500 { get; } = GetColor("#f43f5e");

    /// <summary>
    /// Gets the Tailwind color with the value of #e11d48.
    /// </summary>
    public static Color Rose600 { get; } = GetColor("#e11d48");

    /// <summary>
    /// Gets the Tailwind color with the value of #be123c.
    /// </summary>
    public static Color Rose700 { get; } = GetColor("#be123c");

    /// <summary>
    /// Gets the Tailwind color with the value of #9f1239.
    /// </summary>
    public static Color Rose800 { get; } = GetColor("#9f1239");

    /// <summary>
    /// Gets the Tailwind color with the value of #881337.
    /// </summary>
    public static Color Rose900 { get; } = GetColor("#881337");

    /// <summary>
    /// Gets the Tailwind color with the value of #4c0519.
    /// </summary>
    public static Color Rose950 { get; } = GetColor("#4c0519");
}
