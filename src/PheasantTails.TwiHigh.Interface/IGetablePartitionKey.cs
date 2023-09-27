namespace PheasantTails.TwiHigh.Interface;

using Microsoft.Azure.Cosmos;

public interface IGetablePartitionKey
{
    /// <summary>
    /// Get <see cref="PartitionKey"/> of item.
    /// </summary>
    /// <returns>This imes's <see cref="PartitionKey"/></returns>
    public string GetPartitionKeyString();
}
