using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

/// <summary>
/// The two generators a run relies on, on their own: the map builder and the
/// card reward generator. Both are plain C# and repeat exactly from a seed.
/// </summary>
public sealed class MapAndRewardTests
{
    private RunTestRig rig;

    [SetUp]
    public void SetUp()
    {
        rig = new RunTestRig();
    }

    [TearDown]
    public void TearDown()
    {
        rig.Dispose();
    }

    private RunMap Build(int seed)
    {
        return MapBuilder.Build(seed, RunTestRig.GameSizedMap, rig.Library);
    }

    /// <summary>A map's paths and node types as text, for comparing maps.</summary>
    private static string Describe(RunMap map)
    {
        StringBuilder text = new StringBuilder();

        foreach (MapNodeData node in map.GetAllReachableNodes())
        {
            text.Append(node.Floor).Append(':').Append(node.Index)
                .Append('=').Append(node.AssignedType).Append('>');

            foreach (MapNodeData child in node.Children)
            {
                text.Append(child.Index).Append(',');
            }

            text.Append(';');
        }

        return text.ToString();
    }

    #region Map

    [Test]
    public void SameSeedBuildsTheSameMap()
    {
        Assert.That(Describe(Build(77)), Is.EqualTo(Describe(Build(77))));
    }

    [Test]
    public void DifferentSeedsBuildDifferentMaps()
    {
        Assert.That(Describe(Build(1)), Is.Not.EqualTo(Describe(Build(2))));
    }

    [Test]
    public void MapHasTheRequestedSize()
    {
        RunMap map = Build(77);

        Assert.That(map.FloorCount, Is.EqualTo(15));
        Assert.That(map.NodesPerFloor, Is.EqualTo(7));
        Assert.That(map.Seed, Is.EqualTo(77));
    }

    [Test]
    public void FixedFloorsHaveTheirNodeTypes()
    {
        RunMap map = Build(77);

        foreach (MapNodeData node in map.GetReachableNodes(0))
            Assert.That(node.AssignedType, Is.EqualTo(NodeType.CombatNonElite));

        foreach (MapNodeData node in map.GetReachableNodes(8))
            Assert.That(node.AssignedType, Is.EqualTo(NodeType.Treasure));

        foreach (MapNodeData node in map.GetReachableNodes(14))
            Assert.That(node.AssignedType, Is.EqualTo(NodeType.RestSite));
    }

    [Test]
    public void EveryPathNodeHasSomethingToPlayAndAWayUp()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            RunMap map = Build(seed);

            for (int floor = 0; floor < map.FloorCount; floor++)
            {
                List<MapNodeData> nodes = map.GetReachableNodes(floor);
                Assert.That(nodes, Is.Not.Empty, $"seed {seed} floor {floor}");

                foreach (MapNodeData node in nodes)
                {
                    string where = $"seed {seed} floor {floor} index {node.Index}";

                    Assert.That(node.RuntimeNode, Is.Not.Null, where);
                    Assert.That(
                        node.AssignedType,
                        Is.Not.EqualTo(NodeType.None),
                        where);

                    if (floor < map.FloorCount - 1)
                    {
                        Assert.That(node.Children, Is.Not.Empty, where);
                    }

                    foreach (MapNodeData child in node.Children)
                    {
                        Assert.That(child.Floor, Is.EqualTo(floor + 1), where);
                        Assert.That(
                            Mathf.Abs(child.Index - node.Index),
                            Is.LessThanOrEqualTo(1),
                            where);
                        Assert.That(child.Parents, Has.Member(node), where);
                    }
                }
            }
        }
    }

    [Test]
    public void EliteAndRestStayOffTheFirstFiveFloors()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            RunMap map = Build(seed);

            for (int floor = 0; floor < 5; floor++)
            {
                foreach (MapNodeData node in map.GetReachableNodes(floor))
                {
                    Assert.That(
                        node.AssignedType,
                        Is.Not.EqualTo(NodeType.CombatElite)
                            .And.Not.EqualTo(NodeType.RestSite),
                        $"seed {seed} floor {floor}");
                }
            }
        }
    }

    [Test]
    public void InvalidSizeBuildsNothing()
    {
        LogAssert.Expect(LogType.Error, new Regex("Cannot build a map"));

        Assert.That(
            MapBuilder.Build(1, new MapSettings(0, 15), rig.Library),
            Is.Null);
    }

    [Test]
    public void UnknownNodeResolvesFromTheSeedAlone()
    {
        UnknownNode unknown = null;

        for (int seed = 0; seed < 50 && unknown == null; seed++)
        {
            foreach (MapNodeData node in Build(seed).GetAllReachableNodes())
            {
                if (node.RuntimeNode is UnknownNode found)
                {
                    unknown = found;
                    break;
                }
            }
        }

        Assert.That(unknown, Is.Not.Null, "No Unknown node in fifty maps.");
        Assert.That(unknown.ResolvedNode, Is.SameAs(unknown));

        unknown.OnEnter();

        Assert.That(unknown.ResolvedNode, Is.Not.SameAs(unknown));
        Assert.That(
            unknown.ResolvedNode.Type,
            Is.Not.EqualTo(NodeType.Unknown));
        Assert.That(
            Object.FindObjectsByType<RunManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None),
            Is.Empty);
    }

    #endregion

    #region Rewards

    private CardRewardGenerator Generator()
    {
        CardRewardGenerator generator = new CardRewardGenerator();
        generator.SetPools(
            new Dictionary<CardClass, List<CardData>>
            {
                { CardClass.Ironclad, rig.RewardCards },
                {
                    CardClass.Silent,
                    new List<CardData>
                    {
                        rig.CreateCard(
                            "Silent Card",
                            1,
                            5,
                            CardRarity.Common,
                            CardClass.Silent),
                    }
                },
            });
        return generator;
    }

    [Test]
    public void OffersTheRequestedNumberOfDifferentCards()
    {
        List<CardData> offer = Generator().Generate(
            CardClass.Ironclad,
            3,
            false,
            false,
            new System.Random(1));

        Assert.That(offer.Count, Is.EqualTo(3));
        Assert.That(offer, Is.Unique);
        Assert.That(offer, Is.SubsetOf(rig.RewardCards));
    }

    [Test]
    public void SameSeedGivesTheSameOffer()
    {
        List<CardData> first = Generator().Generate(
            CardClass.Ironclad, 3, false, false, new System.Random(9));
        List<CardData> second = Generator().Generate(
            CardClass.Ironclad, 3, false, false, new System.Random(9));

        CollectionAssert.AreEqual(first, second);
    }

    [Test]
    public void ClassOverrideDrawsFromEveryPool()
    {
        CardRewardGenerator generator = Generator();

        // Seven cards exist across both pools; asking for all of them must
        // reach into the other class's pool.
        List<CardData> offer = generator.Generate(
            CardClass.Ironclad,
            7,
            false,
            true,
            new System.Random(3));

        Assert.That(offer.Count, Is.EqualTo(7));
        Assert.That(
            offer.Exists(card => card.CardClass == CardClass.Silent),
            Is.True);
    }

    [Test]
    public void SmallPoolOffersOnlyWhatItHas()
    {
        List<CardData> offer = Generator().Generate(
            CardClass.Silent,
            3,
            false,
            false,
            new System.Random(4));

        Assert.That(offer.Count, Is.EqualTo(1));
    }

    [Test]
    public void ClassWithoutAPoolOffersNothing()
    {
        LogAssert.Expect(LogType.Error, new Regex("Reward pool for class"));

        List<CardData> offer = Generator().Generate(
            CardClass.Necro,
            3,
            false,
            false,
            new System.Random(5));

        Assert.That(offer, Is.Empty);
    }

    [Test]
    public void ChangingThePoolsChangesWhatIsOffered()
    {
        CardRewardGenerator generator = Generator();
        CardData only = rig.CreateCard("Only", 1, 5);

        generator.SetPools(
            new Dictionary<CardClass, List<CardData>>
            {
                { CardClass.Ironclad, new List<CardData> { only } },
            });

        List<CardData> offer = generator.Generate(
            CardClass.Ironclad,
            3,
            false,
            false,
            new System.Random(6));

        CollectionAssert.AreEqual(new[] { only }, offer);
    }

    #endregion
}
