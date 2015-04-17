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
    public void OnGUI()
    {
      if (!initStyle) InitStyle();

      if (currentSettings.getBool("showWindow"))
      {
        windowPosition = GUILayout.Window(6381183, windowPosition, MainWindow, "Kerbal Science Exchange", windowStyle);
        Utilities.clampToScreen(ref windowPosition);
        Utilities.showTooltip();
      }
    }
    private void InitStyle()
    {
      windowStyle = new GUIStyle(HighLogic.Skin.window);
      windowStyle.fixedWidth = 250f;
      windowStyle.padding.left = 0;

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 190f;
      textStyle.margin.left = 10;

      numberFieldStyle = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth = 52f;
      numberFieldStyle.fixedHeight = 22f;
      numberFieldStyle.alignment = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top = 5;

      buttonStyle = new GUIStyle(GUI.skin.button);
      buttonStyle.fixedWidth = 75f;

      initStyle = true;
    }

    private void MainWindow(int windowID)
    {
      GUILayout.BeginVertical();
      GUILayout.Space(25);
      GUILayout.BeginHorizontal();
      Utilities.createLabel("Science amount:", textStyle);
      sellBuyString = GUILayout.TextField(Utilities.getOnlyNumbers(sellBuyString), 5, numberFieldStyle);
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      Utilities.createLabel("Conversion rate", textStyle);
      Utilities.createLabel(currentSettings.getFloat("conversionRate").ToString("N0"), textStyle);
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      Utilities.createLabel("Tax & conversion fee", textStyle, "The conversion fee depends on your current reputation(" + Math.Round(Reputation.CurrentRep).ToString("N0") + "). At " + currentSettings.getFloat("repHigh").ToString("N0") + " the conversion fee is 0% and at " + currentSettings.getFloat("repLow").ToString("N0") + " the fee is " + currentSettings.getString("delta") + "%");
      Utilities.createLabel(Utilities.round(calcModifier(), 2).ToString(), textStyle);
      GUILayout.EndHorizontal();
      GUILayout.Space(12);
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      float buyScienceAmount = (float)buyScience(true);
      float sellScienceAmount = (float)sellScience(false, true);
      if (Utilities.createButton("Buy", buttonStyle, ("Buy science for " + Math.Round(buyScience(true))), !Funding.CanAfford(buyScienceAmount) || buyScienceAmount == 0))
      {
        buyScience();
      }
      GUILayout.FlexibleSpace();
      if (Utilities.createButton("Sell", buttonStyle, ("Sell science for " + Math.Round(sellScience(true))), !ResearchAndDevelopment.CanAfford(sellScienceAmount) || sellScienceAmount == 0))
      {
        sellScience();
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
      Utilities.updateTooltipAndDrag();
    }
  }
}
