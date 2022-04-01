using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS1591
namespace Tippy
{
    public static class Tips
    {
        public static readonly string[] GeneralTips = new[] {
            "Vuln stacks really don't affect damage \nyou receive by that much, so ignore\nany healer that complains about you\nnot doing mechanics correctly.",
            "Wiping the party is an excellent\nmethod to clear away bad status\neffects, including death.",
            "Players in your party do not pay \nyour sub.\n\nPlay the game however you like and\nreport harassment.",
            "In a big pull, always use the ability with\nthe highest potency number on your bar.",
            "Make sure to avoid the stack marker\nso that your healers have less people\nto heal during raids!",
            "Put macro'd dialogue on all of your\nattacks as a tank to also gain enmity\nfrom your party members.",
            "Make sure to save your LB until the\nboss is at 1 pct.\n\nThis will lead to the greatest effect.",
            "If you want to leave your party quickly\nand blame disconnect, just change\nyour PC time!",
            "Also try the OwO plugin!",
            "I will never leave you!",
            "You cannot hide any longer.",
            "Powered by XIVLauncher!",
            "When playing Hunter, specialize your\npet into taunting to help out your tank!",
            "It doesn't matter if you play BRD or MCH, \nit comes down to personal choice!",
            "Much like doing a \"brake check\" on the\nroad, you can do a \"heal check\"\nin-game! \n\nJust pop Superbolide at a random time,\npreferably about five seconds before \nraidwide damage.",
            "This text is powered by duck energy!",
            "Goat is my original developer, so you can blame him for this.",
            "Twee is defined as excessively or \naffectedly quaint, pretty, \nor sentimental.",
        };

        public static readonly string Intro =
            "Hi, I'm Tippy!\n\nI'm your new friend and assistant.\n\nI will help you get better at FFXIV!";

        public static readonly string GoodSong = "Man, this song is great!";

        public static readonly string[] PldTips =
        {
            "Just don't.",
            "Always save your cooldowns for boss\nfights.",
            "Piety matters as much as tenacity.",
            "Meld piety to maximize your DPS.",
        };

        public static readonly string[] AstTips =
        {
            "Always use Benediction on cooldown\nto maximize healing power!",
            "Remember, Rescue works over gaps\nin the floor.\n\nUse it to save fellow players.",
        };

        public static readonly string[] MnkTips =
        {
            "Always use Six-Sided Star on CD.\n\nIt's your highest potency ability.",
            "Use Fists of Fire Earth to mitigate big\ndamage during fights.\n\nYour healers will thank you.",
        };

        public static readonly string[] DrgTips =
        {
            "Always make sure to use Mirage Dive\ndirectly after Jump so you don't forget\nto use it.",
        };

        public static readonly string[] NinTips =
        {
            "A ninja always appears raiton time!",
            "Tiger Palm is your most important GCD\nfor Brew and Chi generation.\n\nMake sure to cast it in favor of other\nenergy-spending abilities!",
        };

        public static readonly string[] SamTips =
        {
            "Increase Midare damage by shouting\n\"BANKAI\" in chat.\n\nThis can be accomplished through the\nuse of macros.\n",
        };

        public static readonly string[] DncTips =
        {
            "Only give Dance Partner to people\nafter they used a dance emote.",
        };

        public static readonly string[] BlmTips =
        {
            "Tired of casting fire so much?\n\nTry out using only ice spells for a\nchange of pace!",
        };

        public static readonly string[] SmnTips =
        {
            "Titan-Egi can maximize your DPS by\nshielding you from interrupting\ndamage!",
        };

        public static readonly string[] BluTips =
        {
            "Did you know that Blue Mage is\nthrowaway content?",
        };

        public static readonly string[] WhmTips =
        {
            "Always use Benediction on cooldown\nto maximize healing power!",
            "Fluid Aura is a DPS gain!",
            "Always use Cure immediately so you\ncan get Freecure procs!",
            "Cure 1 is more mana-efficient than\nCure 2, so use that instead of Glare!",
            "Remember, Rescue works over gaps\nin the floor.\n\nUse it to save fellow players.",
        };

        public static readonly string[] SchTips =
        {
            "Attach Eos to the BRD so they receive\nhealing when at max range.",
            "Swiftcast Succor before raidwide for\nheals and shield mitigation, allowing\nyou to weave in an oGCD!",
            "Remember, Rescue works over gaps\nin the floor.\n\nUse it to save fellow players.",
        };

        public static readonly string[] BrdTips =
        {
            "Use TBN on your co-DRK so they don't\nhave to!",
            "Always save your cooldowns for boss\nfights.",
            "Piety matters as much as tenacity.",
        };

        public static readonly string[] WarTips =
        {
            "Infuriate before Inner Release to\nguarantee a Direct Hit Critical Inner\nChaos.",
            "Apply Nascent Flash to yourself to gain\nNascent Glint for 10% damage \nmitigation.",
            "Always save your cooldowns for boss\nfights.",
            "Piety matters as much as tenacity.",
        };

        public static readonly string[] GunTips =
        {
            "Much like doing a \"brake check\" on the\nroad, you can do a \"heal check\"\nin-game! \n\nJust pop Superbolide at a random time,\npreferably about five seconds before \nraidwide damage.",
        };

        public static Dictionary<uint, string[]> jobTipDict = new ()
        {
            { 1, PldTips },
            { 2, MnkTips },
            { 3, WarTips },
            { 4, DrgTips },
            { 5, BrdTips },
            { 6, WhmTips },
            { 7, BlmTips },
            { 19, PldTips },
            { 20, MnkTips },
            { 21, WarTips },
            { 22, DrgTips },
            { 23, BrdTips },
            { 24, WhmTips },
            { 25, BlmTips },
            { 26, SchTips.Concat(SmnTips).ToArray() },
            { 27, SmnTips },
            { 28, SchTips },
            { 29, NinTips },
            { 30, NinTips },
            { 31, BrdTips },
            { 33, AstTips },
            { 34, SamTips },
            { 36, BluTips },
            { 37, GunTips },
            { 38, DncTips },
        };
    }
}
