using System.Collections.Generic;

public interface IBodyPart
{
    string Name { get; }
    float Health { get; set; }
    float MaxHealth { get; }
}

public static class BodyParts
{
  public static List<IBodyPart> All() => new List<IBodyPart>
  {
    new AdamsApple(),
    new WishBone(),
    new FunnyBone(),
    new SpareRibs(),
    new BrokenHeart(),
    new ChoppedLiver(),
    new LastKidney()
  };

  private class AdamsApple: IBodyPart
  {
    public string Name { get; private set; } = "Adam's Apple";
    public float Health { get; set; } = 0.1f;
    public float MaxHealth { get; } = 0.1f;
  }

  private class WishBone: IBodyPart
  {
    public string Name { get; private set; } = "Wish Bone";
    public float Health { get; set; } = 0.1f;
    public float MaxHealth { get; } = 0.1f;
  }

  private class FunnyBone: IBodyPart
  {
    public string Name { get; private set; } = "Funny Bone";
    public float Health { get; set; } = 0.14f;
    public float MaxHealth { get; } = 0.14f;
  }

  private class SpareRibs: IBodyPart
  {
    public string Name { get; private set; } = "Spare Ribs";
    public float Health { get; set; } = 0.18f;
    public float MaxHealth { get; } = 0.18f;
  }

  private class BrokenHeart: IBodyPart
  {
    public string Name { get; private set; } = "Broken Heart";
    public float Health { get; set; } = 0.2f;
    public float MaxHealth { get; } = 0.2f;
  }

  private class ChoppedLiver: IBodyPart
  {
    public string Name { get; private set; } = "Chopped Liver";
    public float Health { get; set; } = 0.3f;
    public float MaxHealth { get; } = 0.3f;
  }

  private class LastKidney: IBodyPart
  {
    public string Name { get; private set; } = "Last Kidney";
    public float Health { get; set; } = 0.2f;
    public float MaxHealth { get; } = 0.3f;
  }
}
