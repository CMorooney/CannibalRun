using System.Collections.Generic;

// inherit Godot.Object so it can be marshalled through c++
public abstract class BodyPart : Godot.Object, INameable
{
    public abstract string Name { get; }

    public abstract float Health { get; set; }
    public abstract float MaxHealth { get; }

    public bool Equals(INameable other) => other.Name == Name;
}

public static class BodyParts
{
  public static List<BodyPart> All() => new List<BodyPart>
  {
    new AdamsApple(),
    new WishBone(),
    new FunnyBone(),
    new SpareRibs(),
    new BrokenHeart(),
    new ChoppedLiver(),
    new LastKidney()
  };

  private class AdamsApple: BodyPart
  {
    public override string Name { get; } = "Adam's Apple";
    public override float Health { get; set; } = 0.1f;
    public override float MaxHealth { get; } = 0.1f;
  }

  private class WishBone: BodyPart
  {
    public override string Name { get; } = "Wish Bone";
    public override float Health { get; set; } = 0.1f;
    public override float MaxHealth { get; } = 0.1f;
  }

  private class FunnyBone: BodyPart
  {
    public override string Name { get; } = "Funny Bone";
    public override float Health { get; set; } = 0.14f;
    public override float MaxHealth { get; } = 0.14f;
  }

  private class SpareRibs: BodyPart
  {
    public override string Name { get; } = "Spare Ribs";
    public override float Health { get; set; } = 0.18f;
    public override float MaxHealth { get; } = 0.18f;
  }

  private class BrokenHeart: BodyPart
  {
    public override string Name { get; } = "Broken Heart";
    public override float Health { get; set; } = 0.2f;
    public override float MaxHealth { get; } = 0.2f;
  }

  private class ChoppedLiver: BodyPart
  {
    public override string Name { get; } = "Chopped Liver";
    public override float Health { get; set; } = 0.3f;
    public override float MaxHealth { get; } = 0.3f;
  }

  private class LastKidney: BodyPart
  {
    public override string Name { get; } = "Last Kidney";
    public override float Health { get; set; } = 0.2f;
    public override float MaxHealth { get; } = 0.3f;
  }
}
