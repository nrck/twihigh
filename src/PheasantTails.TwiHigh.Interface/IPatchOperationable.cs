namespace PheasantTails.TwiHigh.Interface;

using Microsoft.Azure.Cosmos;

public interface IPatchOperationable
{
    public PatchOperation[] GetPatchOperations();
}
