using BaseLib.Abstracts;
using Godot;
using KappasTweaks.KappasTweaksCode.Cards;
using KappasTweaks.KappasTweaksCode.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace KappasTweaks.KappasTweaksCode.RestSiteOptions;

public class FuseRestSiteOption(Player owner) : KappaTweaksRestSiteOption(owner)
{
    public override string OptionId => "FUSE";
    
    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2){
            RequireManualConfirmation = true
        };
        List<CardModel> cardsAandB = (await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList();

        Polymerization custom = Owner.RunState.CreateCard<Polymerization>(Owner);
        if (cardsAandB.Count >= 2)
            custom.SetContents(cardsAandB[0], cardsAandB[1]);

        await CardPileCmd.RemoveFromDeck(cardsAandB);
        CardPileAddResult res = await CardPileCmd.Add(custom, PileType.Deck);
        CardCmd.PreviewCardPileAdd(res, 2f);
        return true;
    }
    
    public override Task DoLocalPostSelectVfx(CancellationToken ct = default )
    {
        NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
        return Task.CompletedTask;
    }

    public override Task DoRemotePostSelectVfx()
    {
        NRestSiteRoom? instance = NRestSiteRoom.Instance;
        NRestSiteCharacter? parent = instance?.Characters.First(c => c.Player == Owner);
        parent?.Shake();
        NRelicFlashVfx? child = NRelicFlashVfx.Create(ModelDb.Relic<FusionGate>());
        if (child == null)
            return Task.CompletedTask;
        parent?.AddChildSafely(child);
        child.Position = Vector2.Zero;
        return Task.CompletedTask;
    }
}