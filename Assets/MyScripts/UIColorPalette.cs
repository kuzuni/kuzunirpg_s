using UnityEngine;
// 색상 팔레트
public static class UIColorPalette
{
    public static Color Background = new Color(0.05f, 0.05f, 0.05f);
    public static Color PanelDark = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    public static Color PanelLight = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    public static Color Gold = new Color(1f, 0.84f, 0f);
    public static Color Diamond = new Color(0.4f, 0.8f, 1f);
    public static Color Energy = new Color(0.4f, 1f, 0.4f);

    public static Color ButtonNormal = new Color(0.3f, 0.3f, 0.3f);
    public static Color ButtonHighlight = new Color(0.5f, 0.5f, 0.5f);
    public static Color ButtonPressed = new Color(0.2f, 0.2f, 0.2f);

    public static Color TextWhite = Color.white;
    public static Color TextGreen = new Color(0.4f, 1f, 0.4f);
    public static Color TextRed = new Color(1f, 0.4f, 0.4f);
    public static Color TextYellow = new Color(1f, 1f, 0.4f);
}


// 폰트 크기
public static class UIFontSize
{
    public const int Small = 14;
    public const int Normal = 18;
    public const int Large = 24;
    public const int XLarge = 36;
}