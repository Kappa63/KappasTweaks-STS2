using Godot;
using KappasTweaks.KappasTweaksCode.Extensions;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Art lookup: a card's or power's own image wins, else its family's shared image, else the mod placeholder.
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

    private static readonly Dictionary<string, string> _powerFamilies = Invert(new()
    {
        ["armor"] = "BonePlatingPower ThickSkullPower BlankCheckPower OssuaryPower",
        ["flame"] = "SoreLoserPower BadBeatPower RigorMortisPower LuckyStreakPower HighRollerPower ChainReactionPower PipCounterPower",
        ["link"] = "SafetyNetPower PerfectMatchPower EndlessChainPower LooseBonesPower BoneyardKingPower DominoTheoryPower SteadyHandPower",
        ["dice"] = "WeightedDicePower TwoOfAKindPower DoubleTroublePower",
    });

    public static string Portrait(string name, string type, bool big) =>
        Resolve(big ? "card_portraits/big" : "card_portraits", name, _cardFamilies.GetValueOrDefault(type),
            big ? "card_portraits/big/card.png" : "card_portraits/card.png");

    public static string PowerIcon(string name, string type, bool big) =>
        Resolve(big ? "powers/big" : "powers", name, _powerFamilies.GetValueOrDefault(type),
            big ? "powers/big/power.png" : "powers/power.png");

    private static string Resolve(string folder, string name, string? family, string placeholder)
    {
        string own = $"{folder}/{name}.png".ImagePath();
        if (ResourceLoader.Exists(own))
            return own;
        string shared = $"{folder}/families/{family}.png".ImagePath();
        return family != null && ResourceLoader.Exists(shared) ? shared : placeholder.ImagePath();
    }

    private static Dictionary<string, string> Invert(Dictionary<string, string> families) =>
        families.SelectMany(f => f.Value.Split(' ').Select(type => (type, f.Key))).ToDictionary(p => p.type, p => p.Key);
}
