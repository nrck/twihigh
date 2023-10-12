namespace PheasantTails.TwiHigh.Interface;

using Microsoft.Azure.Cosmos;

public interface IPatchOperationable
{
    /// <summary>
    /// Get <see cref="PatchOperation"/>
    /// </summary>
    /// <returns></returns>
    public PatchOperation[] GetPatchOperations();
}
