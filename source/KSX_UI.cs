using KerboKatz.Extensions;
using System;
using UnityEngine;

namespace KerboKatz
{
  partial class KerbalScienceExchange : KerboKatzBase
  {
    private GUIStyle buttonStyle;
    private GUIStyle numberFieldStyle;
    private GUIStyle textStyle;
    private GUIStyle windowStyle;
    private Rectangle windowPosition = new Rectangle(Rectangle.updateType.Cursor);
    private int windowID = 1702000200;
    private GUIStyle toolbarOptionLabelStyle;
    public void OnGUI()
    {
      if (!initStyle) InitStyle();

      Utilities.UI.createWindow(currentSettings.getBool("showWindow"), windowID, ref windowPosition, MainWindow, "Kerbal Science Exchange", windowStyle);
      Utilities.UI.showTooltip();
    }

    private void InitStyle()
    {
      windowStyle = new GUIStyle(HighLogic.Skin.window);
      windowStyle.fixedWidth = 250;
      windowStyle.padding.left = 0;

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 190;
      textStyle.margin.left = 10;

      numberFieldStyle = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth = 52;
      numberFieldStyle.fixedHeight = 22;
      numberFieldStyle.alignment = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top = 5;

      buttonStyle = new GUIStyle(GUI.skin.button);
      buttonStyle.fixedWidth = 75f;

      if (Utilities.UI.sortTextStyle == null)
        Utilities.UI.getTooltipStyle();
      toolbarOptionLabelStyle = new GUIStyle(Utilities.UI.sortTextStyle);
      toolbarOptionLabelStyle.padding.left += 6;

      initStyle = true;
    }

    private void MainWindow(int windowID)
    {
      GUILayout.BeginVertical();
      GUILayout.Space(25);
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("Science amount:", textStyle);
      sellBuyString = GUILayout.TextField(Utilities.getOnlyNumbers(sellBuyString), 5, numberFieldStyle);
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("Conversion rate", textStyle);
      Utilities.UI.createLabel(currentSettings.getFloat("conversionRate").ToString("N0"), textStyle);
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("Tax & conversion fee", textStyle, "The conversion fee depends on your current reputation(" + Math.Round(Reputation.CurrentRep).ToString("N0") + "). At " + currentSettings.getFloat("repHigh").ToString("N0") + " the conversion fee is 0% and at " + currentSettings.getFloat("repLow").ToString("N0") + " the fee is " + currentSettings.getString("delta") + "%");
      Utilities.UI.createLabel(Utilities.round(calcModifier(), 2).ToString(), textStyle);
      GUILayout.EndHorizontal();
      GUILayout.Space(12);
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      float buyScienceAmount = (float)buyScience(true);
      float sellScienceAmount = (float)sellScience(false, true);
      if (Utilities.UI.createButton("Buy", buttonStyle, ("Buy science for " + Math.Round(buyScience(true))), !Funding.CanAfford(buyScienceAmount) || buyScienceAmount == 0))
      {
        buyScience();
      }
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Sell", buttonStyle, ("Sell science for " + Math.Round(sellScience(true))), !ResearchAndDevelopment.CanAfford(sellScienceAmount) || sellScienceAmount == 0))
      {
        sellScience();
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      Utilities.UI.createOptionSwitcher("Use:", Toolbar.toolbarOptions, ref toolbarSelected, toolbarOptionLabelStyle);
      updateToolbarBool();
      GUILayout.EndVertical();
      Utilities.UI.updateTooltipAndDrag();
    }
  }
}