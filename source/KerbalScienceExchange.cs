using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
  public partial class KerbalScienceExchange : KerboKatzBase
  {
    private bool initStyle       = false;
    private double lastTime      = 0;
    private double modifier      = 0;
    private Rect windowPosition  = new Rect(0, 0, 0, 0);
    private string sellBuyString = "0";

    public KerbalScienceExchange()
    {
      modName = "KerbalScienceExchange";
      requiresUtilities = new Version(1, 0, 4);
    }

    public override void Start()
    {
      if (!(HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
      {
        Destroy(this);
        return;
      }
      base.Start();
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


    public override void OnGuiAppLauncherReady()
    {
      base.OnGuiAppLauncherReady();
      button.Setup(toggleWindow, toggleWindow, Utilities.getTexture("icon", "KerbalScienceExchange/Textures"));
      button.VisibleInScenes = ApplicationLauncher.AppScenes.SPACECENTER;
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
    public override void OnDestroy()
    {
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.set("windowX", windowPosition.x);
        currentSettings.set("windowY", windowPosition.y);
      }
      base.OnDestroy();
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