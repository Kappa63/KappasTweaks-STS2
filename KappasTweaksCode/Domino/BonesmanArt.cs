using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Art lookup: a card's or power's own image wins, else its family's shared image, else a vanilla stand-in (cards)
// or the mod placeholder.
// Own images:   images/card_portraits/[big/]<name>.png   images/powers/[big/]<name>.png
// Family images: same folders under families/<family>.png
internal static class BonesmanArt
{
    private static readonly Dictionary<string, string> _cardFamilies = Invert(new()
    {
        ["swing"] = "StrikeBonesman Bonecrusher BoneSaw Clatter Breaker TwinSpin Endgame LastTile",
        ["fist"] = "Knucklebone Clack BlankStare ColdRead BoxedIn FemurFling HotStreak NearMiss OddManOut EvenOdds",
        ["throw"] = "TileToss PipShot SnakeEyes LongChain FullHouse BoneStorm",
        ["topple"] = "DominoEffect FaultLine CleanSweep PipStorm Topple DeathRattle SnakeTrain DominoAvalanche",
        ["double"] = "DoubleSix Boxcars SetPiece DoubleBack DoublingDown",
        ["blank"] = "BlankSlate Wildcard BoneyardShuffle ResetTheBoard ChippedTile",
        ["bone_arm"] = "DefendBonesman Harden Ossify BobAndWeave Feint Stall OssifiedHide",
        ["bone_wall"] = "BoneWall BoneCage RibCage BoneWard BoneFortress Insurance",
        ["draw"] = "MatchPoint QuickDraw Tally DealOut LayDown BoneDust",
        ["board"] = "CountThePips LinedUp PatientStack LongGame PerfectMatch HedgeBet",
        ["gamble"] = "LoadedBones ShuffleUp AllIn",
        ["aura"] = "BonePlating ThickSkull SoreLoser BadBeat RigorMortis BoneyardKing Ossuary SteadyHand LooseBones EndlessChain",
        ["idol"] = "BlankCheck DoubleTrouble LuckyStreak SafetyNet DominoTheory HighRoller PipCounter TwoOfAKind WeightedDice ChainReaction",
    });

    // Vanilla art borrowed per family until the family image exists. Read from the game's own files, never copied.
    private static readonly Dictionary<string, string> _standIns = new()
    {
        ["swing"] = "necrobinder/strike_necrobinder", ["fist"] = "colorless/fisticuffs", ["throw"] = "necrobinder/bone_shards",
        ["topple"] = "colorless/rolling_boulder", ["double"] = "colorless/jackpot", ["blank"] = "necrobinder/cleanse",
        ["bone_arm"] = "necrobinder/defend_necrobinder", ["bone_wall"] = "necrobinder/legion_of_bone", ["draw"] = "necrobinder/fetch",
        ["board"] = "colorless/master_of_strategy", ["gamble"] = "silent/calculated_gamble", ["aura"] = "necrobinder/reaper_form",
        ["idol"] = "necrobinder/forbidden_grimoire",
    };

    private static readonly Dictionary<string, string> _powerFamilies = Invert(new()
    {
        ["armor"] = "BonePlatingPower ThickSkullPower BlankCheckPower OssuaryPower",
        ["flame"] = "SoreLoserPower BadBeatPower RigorMortisPower LuckyStreakPower HighRollerPower ChainReactionPower PipCounterPower",
        ["link"] = "SafetyNetPower PerfectMatchPower EndlessChainPower LooseBonesPower BoneyardKingPower DominoTheoryPower SteadyHandPower",
        ["dice"] = "WeightedDicePower TwoOfAKindPower DoubleTroublePower",
    });

    public static string Portrait(string name, string type, bool big)
    {
        string? family = _cardFamilies.GetValueOrDefault(type);
        string placeholder = family != null && _standIns.TryGetValue(family, out string? standIn)
            ? $"res://images/packed/card_portraits/{standIn}.png"
            : (big ? "card_portraits/big/card.png" : "card_portraits/card.png").ImagePath();
        return Resolve(big ? "card_portraits/big" : "card_portraits", name, family, placeholder);
    }

    public static string PowerIcon(string name, string type, bool big) =>
        Resolve(big ? "powers/big" : "powers", name, _powerFamilies.GetValueOrDefault(type),
            (big ? "powers/big/power.png" : "powers/power.png").ImagePath());

    private static string Resolve(string folder, string name, string? family, string placeholder)
    {
        string own = $"{folder}/{name}.png".ImagePath();
        if (ResourceLoader.Exists(own))
            return own;
        string shared = $"{folder}/families/{family}.png".ImagePath();
        return family != null && ResourceLoader.Exists(shared) ? shared : placeholder;
    }

    private static Dictionary<string, string> Invert(Dictionary<string, string> families) =>
        families.SelectMany(f => f.Value.Split(' ').Select(type => (type, f.Key))).ToDictionary(p => p.type, p => p.Key);
}
