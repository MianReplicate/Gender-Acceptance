namespace GenderAcceptance.Mian;

public struct TransphobicStatus
{
    public bool GenerallyTransphobic { get; set; }
    public bool ChaserAttributeCounts { get; set; }
    public bool HasTransphobicTrait { get; set; }
    public bool TransphobicPreceptCounts { get; set; }

    public override string ToString()
    {
        return GenerallyTransphobic.ToString();
    }
}