using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
  public class KerbalScienceExchange : MonoBehaviour
  {
    private ApplicationLauncherButton button;
    private bool initStyle       = false;
    private double lastTime      = 0;
    private double modifier      = 0;
    private float tooltipHeight;
    private GUIStyle buttonStyle;
    private GUIStyle numberFieldStyle;
    private GUIStyle textStyle;
    private GUIStyle tooltipStyle;
    private GUIStyle windowStyle;
    private Rect tooltipRect     = new Rect(0, 0, 230, 20);
    private Rect windowPosition  = new Rect(0, 0, 0, 0);
    private settings currentSettings;
    private string CurrentTooltip;
    private string modName       = "KerbalScienceExchange";
    private string sellBuyString = "0";

    private void Awake()
    {
      if (!Utilities.checkUtilitiesSupport(new Version(1, 0, 0), modName))
      {
        Destroy(this);
        return;
      }
      GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
    }

    private void Start()
    {
      currentSettings = new settings();
      currentSettings.load(modName, "settings", modName);
      currentSettings.setDefault("delta", "10");
      currentSettings.setDefault("repLow", "0");
      currentSettings.setDefault("repHigh", "10000");
      currentSettings.setDefault("conversionRate", "1000");
      currentSettings.setDefault("tax", "1");
      currentSettings.setDefault("showWindow", "false");
      currentSettings.setDefault("windowX", "99999");
      currentSettings.setDefault("windowY", "38");
      windowPosition.x = currentSettings.getFloat("windowX");
      windowPosition.y = currentSettings.getFloat("windowY");
    }

    private void OnDestroy()
    {
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.set("windowX", windowPosition.x);
        currentSettings.set("windowY", windowPosition.y);
        currentSettings.save();
      }
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
      if (button != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(button);
      }
    }

    private double calcModifier()
    {
      if (Planetarium.GetUniversalTime() - 5 <= lastTime)
        return modifier;
      float repHigh = currentSettings.getFloat("repHigh");
      float repLow = currentSettings.getFloat("repLow");
      float repScale = ((Mathf.Min(Reputation.UnitRep, repHigh) - repHigh) / (repLow - repHigh));
      lastTime = Planetarium.GetUniversalTime();
      modifier = (double)(1 + (repScale * currentSettings.getFloat("delta") + currentSettings.getFloat("tax")) / 100);
      return modifier;
    }

    private void OnGuiAppLauncherReady()
    {
      button = ApplicationLauncher.Instance.AddModApplication(
          toggleWindow, 	//RUIToggleButton.onTrue
          toggleWindow,	//RUIToggleButton.onFalse
          null, //RUIToggleButton.OnHover
          null, //RUIToggleButton.onHoverOut
          null, //RUIToggleButton.onEnable
          null, //RUIToggleButton.onDisable
          ApplicationLauncher.AppScenes.SPACECENTER, //visibleInScenes
          Utilities.getTexture("icon", "KerbalScienceExchange/Textures")//texture
      );
    }

    private void toggleWindow()
    {
      if (currentSettings.getBool("showWindow"))
      {
        currentSettings.set("showWindow", false);
      }
      else
      {
        currentSettings.set("showWindow", true);
      }
    }

    public void OnGUI()
    {
      if (HighLogic.LoadedScene == GameScenes.SPACECENTER && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) && !initStyle) InitStyle();

      if (HighLogic.LoadedScene == GameScenes.SPACECENTER && currentSettings.getBool("showWindow") && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
      {
        windowPosition = GUILayout.Window(6381183, windowPosition, MainWindow, "Kerbal Science Exchange", windowStyle);
        Utilities.clampToScreen(ref windowPosition);
        if (!String.IsNullOrEmpty(CurrentTooltip))
        {
          tooltipRect.x = Input.mousePosition.x + 10;
          tooltipRect.y = Screen.height - Input.mousePosition.y + 10;
          Utilities.clampToScreen(ref tooltipRect);
          tooltipRect.height = tooltipHeight;
          GUI.Label(tooltipRect, CurrentTooltip, tooltipStyle);
          GUI.depth = 0;
        }
      }
    }

    private void InitStyle()
    {
      windowStyle                    = new GUIStyle(HighLogic.Skin.window);
      windowStyle.fixedWidth         = 250f;
      windowStyle.padding.left       = 0;

      textStyle                      = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth           = 190f;
      textStyle.margin.left          = 10;

      numberFieldStyle               = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth    = 52f;
      numberFieldStyle.fixedHeight   = 22f;
      numberFieldStyle.alignment     = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top    = 5;

      buttonStyle                    = new GUIStyle(GUI.skin.button);
      buttonStyle.fixedWidth         = 75f;

      tooltipStyle                   = new GUIStyle(HighLogic.Skin.label);
      tooltipStyle.fixedWidth        = 230f;
      tooltipStyle.padding.top       = 5;
      tooltipStyle.padding.left      = 5;
      tooltipStyle.padding.right     = 5;
      tooltipStyle.padding.bottom    = 5;
      tooltipStyle.fontSize          = 10;
      tooltipStyle.normal.background = Utilities.getTexture("tooltipBG", "Textures");
      tooltipStyle.normal.textColor  = Color.white;
      tooltipStyle.border.top        = 1;
      tooltipStyle.border.bottom     = 1;
      tooltipStyle.border.left       = 8;
      tooltipStyle.border.right      = 8;
      tooltipStyle.stretchHeight     = true;

      initStyle                      = true;
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
      if (Utilities.createButton("Buy", buttonStyle, "calcBuy", !Funding.CanAfford(buyScienceAmount) || buyScienceAmount==0))
      {
        buyScience();
      }
      GUILayout.FlexibleSpace();
      if (Utilities.createButton("Sell", buttonStyle, "calcSell", !ResearchAndDevelopment.CanAfford(sellScienceAmount) || sellScienceAmount==0))
      {
        sellScience();
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      if (GUI.tooltip == "calcBuy")
      {
        CurrentTooltip = "Buy science for " + Math.Round(buyScience(true));
      }
      else if (GUI.tooltip == "calcSell")
      {
        CurrentTooltip = "Sell science for " + Math.Round(sellScience(true));
      }
      else
      {
        CurrentTooltip = GUI.tooltip;
      }
      tooltipHeight = tooltipStyle.CalcHeight(new GUIContent(CurrentTooltip), tooltipStyle.fixedWidth);
      GUILayout.EndVertical();
      GUI.DragWindow();
    }

    private double sellScience(bool ret = false, bool retScience = false)
    {
      double sellScience;
      if (double.TryParse(sellBuyString, out sellScience))
      {
        if (retScience)
          return sellScience;
        double addFunding = sellScience * currentSettings.getDouble("conversionRate") / calcModifier();
        if (ret)
          return addFunding;
        if (!ResearchAndDevelopment.CanAfford((float)sellScience))
        {
          sellBuyString = "0";
          return 0;
        }
        ResearchAndDevelopment.Instance.AddScience((float)-sellScience, TransactionReasons.None);
        Funding.Instance.AddFunds(addFunding, TransactionReasons.None);
        sellBuyString = "0";
      }
      return 0;
    }

    private double buyScience(bool ret = false)
    {
      double buyScience;
      if (double.TryParse(sellBuyString, out buyScience))
      {
        double removeFunding = buyScience * calcModifier() * currentSettings.getDouble("conversionRate");
        if (ret)
          return removeFunding;
        if (!Funding.CanAfford((float)removeFunding))
        {
          sellBuyString = "0";
          return 0;
        }
        ResearchAndDevelopment.Instance.AddScience((float)buyScience, TransactionReasons.None);
        Funding.Instance.AddFunds(-removeFunding, TransactionReasons.None);
        sellBuyString = "0";
      }
      return 0;
    }
  }
}